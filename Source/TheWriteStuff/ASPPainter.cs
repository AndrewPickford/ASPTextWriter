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
        private PartResourceDefinition _paintResource = null;
        private PartResource _paint = null;

        [KSPEvent(name = "paintEvent", guiName = "Paint", active = false, guiActive = true, guiActiveEditor = false, externalToEVAOnly = true, guiActiveUnfocused = false)]
        public void paintEvent()
        {
            if (_paint == null)
            {
                ScreenMessages.PostScreenMessage("No space paint resource defined", 5, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            if (_paint.amount < 0.95)
            {
                ScreenMessages.PostScreenMessage("Not enough paint left!", 5, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            _pointer = gameObject.GetComponent<PaintPointer>();
            if (_pointer == null)
            {
                _pointer = gameObject.AddComponent<PaintPointer>();
                _pointer.initialise(this);
            }
        }

        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(new EventData<Vessel>.OnEvent(this.OnVesselChange));
        }

        public void OnVesselChange(Vessel vesselChange)
        {
            setPaintState();
        }

        private void setPaintState()
        {
            if (_paint == null)
            {
                if (Global.Debug1) Utils.Log("Part has no space paint resource.");
                Events["paintEvent"].active = false;
                return;
            }

            if (_paint.amount == 0)
            {
                if (Global.Debug3) Utils.Log("Part has no space paint left.");
                Events["paintEvent"].active = false;
                return;
            }

            if (this.part.vessel.isEVA) Events["paintEvent"].active = true;
            else Events["paintEvent"].active = false;

            if (Global.Debug3) Utils.Log("painter state {0}", Events["paintEvent"].active);
        }

        // KAS part grabbed message
        public void OnPartGrabbed(Vessel vessel)
        {
            setPaintState();
        }

        // KAS part dropped message
        public void OnPartDropped(Vessel vessel)
        {
            setPaintState();
        }

        public void usePaint()
        {
            _paint.amount -= 1;
            if (_paint.amount < 0) _paint.amount = 0;
            setPaintState();
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

            _paintResource = PartResourceLibrary.Instance.GetDefinition("SpacePaint");
            _paint = this.part.Resources.Get(_paintResource.id);
            setPaintState();

            if (Global.Debug3) Utils.Log("paint resource id: {0}", _paintResource);
            if (Global.Debug3) Utils.Log("paint: {0}/{1}", _paint.amount, _paint.maxAmount);
        }
    }
}
