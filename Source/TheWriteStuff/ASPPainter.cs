using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ASPPainter : PartModule
    {
        private PaintPointer _pointer;

        [KSPEvent(name = "paintEvent", guiName = "Paint", active = false, guiActive = true, guiActiveEditor = false, externalToEVAOnly = true, guiActiveUnfocused = false)]
        public void paintEvent()
        {
            _pointer = gameObject.GetComponent<PaintPointer>();
            if (_pointer == null)
            {
                _pointer = gameObject.AddComponent<PaintPointer>();
                _pointer.initialise(this);
            }
        }

        void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(new EventData<Vessel>.OnEvent(this.OnVesselChange));
        }

        void OnVesselChange(Vessel vesselChange)
        {
            if (FlightGlobals.ActiveVessel.isEVA) Events["paintEvent"].active = true;
            else Events["paintEvent"].active = true;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(this.OnVesselChange));

            if (state == StartState.Editor)
            {
                Events["paintEvent"].active = false;
                return;
            }

            if (part.vessel.isEVA) Events["paintEvent"].active = true;
            else Events["paintEvent"].active = false;
        }
    }
}
