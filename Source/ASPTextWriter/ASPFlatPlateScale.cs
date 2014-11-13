using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ASPFlatPlateScale : PartModule
    {
        [KSPField(isPersistant = true)]
        public float size = 1f;

        [KSPField(isPersistant = true)]
        public float sizeMin = 0.1f;

        [KSPField(isPersistant = true)]
        public float sizeMax = 8f;

        [KSPField(isPersistant = true)]
        public float sizeStep = 0.1f;

        [KSPField(isPersistant = true)]
        public float thickness = 1f;

        [KSPField(isPersistant = true)]
        public float thicknessMin = 0.1f;

        [KSPField(isPersistant = true)]
        public float thicknessMax = 5f;

        [KSPField(isPersistant = true)]
        public float thicknessStep = 0.1f;

        [KSPField(isPersistant = true)]
        public float baseMass = -1f;

        Vector3 _originalScale;

        [KSPEvent(name = "Increase Size Event", guiName = "Increase Size", guiActive = false, guiActiveEditor = true)]
        public void increaseSizeEvent()
        {
            size += sizeStep;
            if (size > sizeMax) size = sizeMax;
            rescale();
        }

        [KSPEvent(name = "Decrease Size Event", guiName = "Decrease Size", guiActive = false, guiActiveEditor = true)]
        public void decreaseSizeEvent()
        {
            size -= sizeStep;
            if (size < sizeMin) size = sizeMin;
            rescale();
        }

        [KSPEvent(name = "Increase Thickness Event", guiName = "Increase Thickness", guiActive = false, guiActiveEditor = true)]
        public void increaseThicknessEvent()
        {
            thickness += thicknessStep;
            if (thickness > thicknessMax) thickness = thicknessMax;
            rescale();
        }

        [KSPEvent(name = "Decrease Thickness Event", guiName = "Decrease Thickness", guiActive = false, guiActiveEditor = true)]
        public void decreaseThicknessEvent()
        {
            thickness -= thicknessStep;
            if (thickness < thicknessMin) thickness = thicknessMin;
            rescale();
        }

        private void rescale()
        {
            Vector3 scale = new Vector3(size * _originalScale.x, thickness * size * _originalScale.y, size * _originalScale.z);
            part.transform.localScale = scale;
            part.transform.hasChanged = true;
            part.mass = baseMass * scale.x * scale.y * scale.z;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (baseMass == -1f)
            {
                baseMass = part.mass;
            }

            _originalScale = part.transform.localScale;

            rescale();
        }
    }
}
