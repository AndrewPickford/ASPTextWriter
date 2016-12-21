using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ASPPainter : KIS.ModuleKISItem
    {
        private PaintPointer _pointer;
        private PartResourceDefinition _paintResource = null;
        private PartResource _paint = null;

        [KSPEvent(name = "paintEvent", guiName = "Paint", active = false, guiActiveEditor = false, guiActive = true)]
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


        public override void OnEquip(KIS.KIS_Item item)
        {
            if (Global.Debug3) Utils.Log("start");

            if (item.equippedPart)
            {
                ASPPainter painter = item.equippedPart.GetComponent<ASPPainter>();
                if (painter) painter.Events["paintEvent"].active = true;
            }
        }

        public override void OnUnEquip(KIS.KIS_Item item)
        {
            if (Global.Debug3) Utils.Log("start");

            if (item.equippedPart)
            {
                ASPPainter painter = item.equippedPart.GetComponent<ASPPainter>();
                if (painter) painter.Events["paintEvent"].active = false;
            }
        }

        public void usePaint()
        {
            if (Global.Debug3) Utils.Log("start");

            _paint.amount -= 1;
            if (_paint.amount < 0) _paint.amount = 0;
        }

        public override void OnStart(StartState state)
        {
            if (Global.Debug3) Utils.Log("start");
            base.OnStart(state);

            _paintResource = PartResourceLibrary.Instance.GetDefinition("SpacePaint");
            _paint = this.part.Resources.Get(_paintResource.id);
        }
    }
}
