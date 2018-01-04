﻿using NickAc.Backend.Networking.TcpLib;
using NickAc.Backend.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickAc.Backend.Utils
{
    public static class DevicePersistManager
    {
        private const string DEVICES_FILENAME = "devices.xml";
        private static IDictionary<Guid, IDeckDevice> deckDevicesFromConnection = new Dictionary<Guid, IDeckDevice>();

        public static IDictionary<Guid, IDeckDevice> DeckDevicesFromConnection {
            get {
                return deckDevicesFromConnection;
            }
        }

        public class DeviceEventArgs : EventArgs
        {
            public DeviceEventArgs(IDeckDevice device)
            {
                Device = device;
            }

            public IDeckDevice Device { get; set; }
        }

        /// <summary>
        /// Called to signal to subscribers that a device was connected
        /// </summary>
        public static event EventHandler<DeviceEventArgs> DeviceConnected;
        public static void OnDeviceConnected(object sender, IDeckDevice e)
        {
            var eh = DeviceConnected;

            eh?.Invoke(sender, new DeviceEventArgs(e));
        }

        /// <summary>
        /// Called to signal to subscribers that a device was disconnected
        /// </summary>
        public static event EventHandler<DeviceEventArgs> DeviceDisconnected;
        public static void OnDeviceDisconnected(object sender, IDeckDevice e)
        {
            var eh = DeviceDisconnected;

            eh?.Invoke(sender, new DeviceEventArgs(e));
        }


        public static ICollection<Guid> GuidsFromConnections {
            get {
                return deckDevicesFromConnection.Keys;
            }
        }

        private static List<IDeckDevice> persistedDevices;

        public static List<IDeckDevice> PersistedDevices {
            get {
                return persistedDevices;
            }
        }
        
        public static Guid GetConnectionGuidFromDeckDevice(IDeckDevice device)
        {
            return deckDevicesFromConnection.FirstOrDefault(m => m.Value.DeviceGuid == device.DeviceGuid).Key;
        }

        public static IDeckDevice GetDeckDeviceFromConnectionGuid(Guid connection)
        {
            return deckDevicesFromConnection.FirstOrDefault(m => m.Key == connection).Value;
        }


        public static bool IsDeviceOnline(IDeckDevice device)
        {
            return deckDevicesFromConnection.Values.Any(m => m.DeviceGuid == device.DeviceGuid);
        }


        public static bool IsDeviceOnline(Guid device)
        {
            return deckDevicesFromConnection.Values.Any(m => m.DeviceGuid == device);
        }

        public static bool IsDeviceConnected(Guid device)
        {
            return deckDevicesFromConnection.Keys.Contains(device);
        }

        public static bool IsDevicePersisted(IDeckDevice device)
        {
            return IsDevicePersisted(device.DeviceGuid);
        }

        private static bool IsDevicePersisted(Guid deviceGuid)
        {
            return PersistedDevices.Any(w => w.DeviceGuid == deviceGuid);
        }

        public static void PersistDevice(IDeckDevice device)
        {
            if (IsDevicePersisted(device)) {
                device.DeviceName = PersistedDevices.First(m => m.DeviceGuid == device.DeviceGuid).DeviceName;
                device.MainFolder = PersistedDevices.First(m => m.DeviceGuid == device.DeviceGuid).MainFolder;
                PersistedDevices.RemoveAll(m => m.DeviceGuid == device.DeviceGuid);
            }
            PersistedDevices.Add(device);
        }

        public static void RemoveConnectionState(ConnectionState state)
        {
            if (deckDevicesFromConnection.Keys.Contains(state.ConnectionGuid)) {
                var device = GetDeckDeviceFromConnectionGuid(state.ConnectionGuid);
                OnDeviceDisconnected(new object(), device);
            }
            deckDevicesFromConnection.Remove(state.ConnectionGuid);
        }

        public static void RemoveConnectionState(Guid state)
        {
            deckDevicesFromConnection.Remove(state);
        }

        public static void ChangeConnectedState(ConnectionState state, IDeckDevice device)
        {
            if (device == null || state == null) return;
            if (!deckDevicesFromConnection.ContainsKey(state.ConnectionGuid)) {
                deckDevicesFromConnection.Add(state.ConnectionGuid, device);
            }
        }

        public static void LoadDevices()
        {
            if (File.Exists(DEVICES_FILENAME)) {
                var newPersistedDevices = XMLUtils.FromXML<List<IDeckDevice>>(File.ReadAllText(DEVICES_FILENAME));
                if (persistedDevices == null) persistedDevices = new List<IDeckDevice>();
                persistedDevices.AddRange(newPersistedDevices.Where(m => !IsDevicePersisted(m.DeviceGuid)));
            } else {
                persistedDevices = new List<IDeckDevice>();
            }
        }

        public static void SaveDevices()
        {
            if (persistedDevices != null) {
                File.WriteAllText(DEVICES_FILENAME, XMLUtils.ToXML(persistedDevices));
            } else {
                File.Delete(DEVICES_FILENAME);
            }
        }
    }
}
