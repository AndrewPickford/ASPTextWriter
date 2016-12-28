using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ASPEvaRename : PartModule
    {
        [KSPEvent(name = "renameVesselEvent", guiName = "Rename", guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true)]
        public void renameVesselEvent()
        {
            vessel.RenameVessel();
        }
    }
}
