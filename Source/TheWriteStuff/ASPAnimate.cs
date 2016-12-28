using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ASPAnimate : ModuleAnimateGeneric
    {
        [KSPEvent(name = "RaiseMaxHeightQuick", guiName = "Raise Max Height ++", active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true)]
        public void RaiseMaxHeightQuick()
        {
            deployPercent += 1f;
            if (deployPercent > 100f) deployPercent = 100f;
        }

        [KSPEvent(name = "RaiseMaxHeight", guiName = "Raise Max Height +", active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true)]
        public void RaiseMaxHeight()
        {
            deployPercent += 0.1f;
            if (deployPercent > 100f) deployPercent = 100f;
        }

        [KSPEvent(name = "LowerMaxHeight", guiName = "Lower Max Height -", active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true)]
        public void LowerMaxHeight()
        {
            deployPercent -= 0.1f;
            if (deployPercent < 0f) deployPercent = 0f;
        }

        [KSPEvent(name = "LowerMaxHeightQuick", guiName = "Lower Max Height --", active = true, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true)]
        public void LowerMaxHeightQuick()
        {
            deployPercent -= 1f;
            if (deployPercent < 0f) deployPercent = 0f;
        }
    }
}
