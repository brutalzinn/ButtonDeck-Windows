﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickAc.Backend.Objects.Implementation.DeckActions
{
    public class ExecutableRunAction : AbstractDeckAction
    {
        [ActionPropertyInclude]
        [ActionPropertyDescription("To Execute")]
        public string ToExecute { get; set; } = "";

        public override AbstractDeckAction CloneAction()
        {
            return new ExecutableRunAction();
        }

        public override DeckActionCategory GetActionCategory() => DeckActionCategory.General;

        public override string GetActionName() => "Run Executable";

        public override bool OnButtonClick(DeckDevice deckDevice)
        {
            var proc = new ProcessStartInfo("cmd.exe", "/c " + ToExecute)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            Process.Start(proc);
            return true;
        }

        public override void OnButtonDown(DeckDevice deckDevice)
        {
        }

        public override void OnButtonUp(DeckDevice deckDevice)
        {
        }
    }
}
