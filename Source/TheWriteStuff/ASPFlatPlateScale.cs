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
        public float baseMass = -1f;

        [KSPField(isPersistant = true)]
        public string thicknessAxis = "z";

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Size", guiFormat = "0.000")]
        [UI_ScaleEdit(scene = UI_Scene.Editor)]
        public float size = 1f;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Thickness", guiFormat = "0.000")]
        [UI_ScaleEdit(scene = UI_Scene.Editor)]
        public float thickness = 1f;

        private List<Vector3> _originalScales;
        private List<string> _transformNames;
        private List<Transform> _transforms;

        private static float[] _SizeIntervals = { 0.1f, 1f, 2f, 4f, 8f };
        private static float[] _SizeIncrements = { 0.01f, 0.01f, 0.02f, 0.04f, 0.08f };
        private static float[] _ThicknessIntervals = { 0.1f, 1f, 2f, 4f, 8f };
        private static float[] _ThicknessIncrements = { 0.01f, 0.01f, 0.02f, 0.04f, 0.08f };

        private void OnScaleChanged(BaseField field, object obj)
        {
            if (_transforms == null) return;

            if (Global.Debug3) Utils.Log("size {0}, thickness {1}", size, thickness);

            for (int i = 0; i < _transforms.Count; ++i)
            {
                if (Global.Debug3) Utils.Log("rescaling transform {0}", _transformNames[i]);

                Vector3 scale = new Vector3(_originalScales[i].x, _originalScales[i].y, _originalScales[i].z);
                if (thicknessAxis == "x") scale.x *= thickness;
                else scale.x *= size;
                if (thicknessAxis == "y") scale.y *= thickness;
                else scale.y *= size;
                if (thicknessAxis == "z") scale.z *= thickness;
                else scale.z *= size;

                if (Global.Debug3) Utils.Log("new scale {0}, {1}, {2}", scale.x, scale.y, scale.z);

                _transforms[i].localScale = scale;
                _transforms[i].hasChanged = true;
            }
            part.mass = baseMass * size * size * thickness;
        }

        public override void OnSave(ConfigNode node)
        {
            saveConfig(node);
        }

        private void saveConfig(ConfigNode node)
        {
            if (_transformNames != null)
            {
                node.AddValue("transforms", _transformNames.Count);
                for (int i = 0; i < _transformNames.Count; ++i)
                    node.AddValue("transform" + i.ToString(), _transformNames[i]);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            loadConfig(node);
        }

        private void loadConfig(ConfigNode node)
        {
            if (node.HasValue("transforms"))
            {
                int trans = int.Parse(node.GetValue("transforms"));
                _transformNames = new List<string>(trans);

                for (int i = 0; i < trans; ++i)
                    if (node.HasValue("transform" + i.ToString())) _transformNames.Add(node.GetValue("transform" + i.ToString()));
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (baseMass == -1f)
            {
                baseMass = part.mass;
            }

            if (_transformNames == null)
            {
                ConfigNode node = new ConfigNode();
                (part.partInfo.partPrefab.Modules["ASPFlatPlateScale"] as ASPFlatPlateScale).saveConfig(node);
                loadConfig(node);
            }

            AvailablePart apart = PartLoader.getPartInfoByName(this.part.partInfo.name);
            _transforms = new List<Transform>(_transformNames.Count);
            _originalScales = new List<Vector3>(_transformNames.Count);
            for (int i = 0; i < _transformNames.Count; ++i)
            {
                if (Global.Debug3) Utils.Log("Ädding transform {0}", _transformNames[i]);

                Transform t = apart.partPrefab.FindModelTransform(_transformNames[i]);
                if (t == null)
                {
                    Utils.LogError("Failed to add transform {0}", _transformNames[i]);
                    continue;
                }

                _transforms.Add(this.part.FindModelTransform(_transformNames[i]));
                Vector3 s = new Vector3(t.localScale.x, t.localScale.y, t.localScale.z);
                _originalScales.Add(s);
            }

            UI_ScaleEdit scaleEdit = (UI_ScaleEdit)Fields["size"].uiControlEditor;
            scaleEdit.intervals = _SizeIntervals;
            scaleEdit.incrementSlide = _SizeIncrements;
            scaleEdit.sigFigs = 3;
            scaleEdit.onFieldChanged = OnScaleChanged;

            scaleEdit = (UI_ScaleEdit)Fields["thickness"].uiControlEditor;
            scaleEdit.intervals = _ThicknessIntervals;
            scaleEdit.incrementSlide = _ThicknessIncrements;
            scaleEdit.sigFigs = 3;
            scaleEdit.onFieldChanged = OnScaleChanged;

            OnScaleChanged(null, null);
        }
    }
}
