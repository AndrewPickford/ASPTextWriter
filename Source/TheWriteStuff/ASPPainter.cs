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

        [KSPEvent(name = "paintEvent", guiName = "Paint", active = false, guiActive = true)]
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

        public override void OnItemUse(KIS.KIS_Item item, KIS.KIS_Item.UseFrom useFrom)
        {
            if (Global.Debug3) Utils.Log("start");

            ASPPainter[] painterMods = item.equippedPart.GetComponents<ASPPainter>();
            if (painterMods.Length == 1) painterMods[0].paintEvent();
            else Utils.LogError("expected one ASPPainter module found {0}", painterMods.Length);
        }

        public override void OnEquip(KIS.KIS_Item item)
        {
            if (Global.Debug3) Utils.Log("start");

            ASPPainter[] painterMods = item.equippedPart.GetComponents<ASPPainter>();
            if (painterMods.Length == 1) painterMods[0].paintEventOn();
            else Utils.LogError("expected one ASPPainter module found {0}", painterMods.Length);
        }

        public void usePaint()
        {
            _paint.amount -= 1;
            if (_paint.amount < 0) _paint.amount = 0;
        }

        public void paintEventOn()
        {
            Events["paintEvent"].active = true;
        }

        public override void OnLoad(ConfigNode node)
        {
            if (Global.Debug3) Utils.Log("start");
            Events["paintEvent"].active = false;
        }

        public override void OnStart(StartState state)
        {
            if (Global.Debug3) Utils.Log("start");
            base.OnStart(state);

            if (state == StartState.Editor)
            {
                Events["paintEvent"].active = false;
            }

            _paintResource = PartResourceLibrary.Instance.GetDefinition("SpacePaint");
            _paint = this.part.Resources.Get(_paintResource.id);
        }
    }
}
