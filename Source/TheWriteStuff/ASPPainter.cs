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

        [KSPEvent(name = "Paint", guiName = "Paint", guiActive = true, guiActiveEditor = false, externalToEVAOnly = true, guiActiveUnfocused = false)]
        public void paintEvent()
        {
            _pointer = gameObject.GetComponent<PaintPointer>();
            if (_pointer == null)
            {
                _pointer = gameObject.AddComponent<PaintPointer>();
                _pointer.initialise(this);
            }
        }
    }
}
