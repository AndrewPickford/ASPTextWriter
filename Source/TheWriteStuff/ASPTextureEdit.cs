using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ASPTextureEdit : PartModule
    {
        [KSPField(isPersistant = true)]
        public string transformNames = string.Empty;

        public BoundingBox boundingBox;
        public TransformOption transformsOption = TransformOption.USE_FIRST;

        public string[] transformNamesArray { get; private set; }

        private bool _ok = false;
        private Transform[] _transformsArray = null;

        private void OnEditorDestroy()
        {
            //if (_gui != null) GameObject.Destroy(_gui);
        }

        private string findTransformName(Transform[] transforms, bool validTexture, bool validNormalMap)
        {
            foreach (Transform child in transforms)
            {
                if (child.gameObject.renderer != null && child.gameObject.renderer.material != null)
                {
                    if (validTexture == false) return child.name;

                    Texture2D main = child.gameObject.renderer.material.mainTexture as Texture2D;

                    if (main != null && main.name != string.Empty)
                    {
                        if (validNormalMap)
                        {
                            Texture2D normal = child.gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;

                            if (normal != null && normal.name != string.Empty) return child.name;
                        }
                        else return child.name;
                    }
                }
            }

            return string.Empty;
        }

        private string findFirstUseableTransform()
        {
            string transformName = string.Empty;
            Transform[] children = this.part.partInfo.partPrefab.GetComponentsInChildren<Transform>(true);

            // use the first transform with both a texture and a normal map with non empty names
            transformName = findTransformName(children, true, true);
            if (Global.Debug2) Utils.Log("findTransformName,true,true = {0}", transformName);
            if (transformName != string.Empty) return transformName;

            // if not use the first tranform with a texture and non empty name
            transformName = findTransformName(children, true, false);
            if (Global.Debug2) Utils.Log("findTransformName,true,false = {0}", transformName);
            if (transformName != string.Empty) return transformName;

            // fall back to the first object with a material
            transformName = findTransformName(children, false, false);
            if (Global.Debug2) Utils.Log("findTransformName,false,false = {0}", transformName);
            return transformName;
        }

        private void fillTransformNameArray()
        {
            if (transformNames == string.Empty) transformNames = findFirstUseableTransform();
            if (transformNames == string.Empty)
            {
                Utils.LogError("unable to find transform with material, part {0}.", this.part.name);
                throw new ArgumentException("unable to find transform");
            }

            transformNamesArray = Utils.SplitString(transformNames);
            if (transformNamesArray == null || transformNamesArray.Length == 0 || transformNamesArray[0] == string.Empty)
            {
                Utils.LogError("transformNames empty, part {0}.", this.part.name);
                throw new ArgumentException("transformNames empty");
            }
        }

        private int countUseableTransforms()
        {
            int count = 0;
            for (int i = 0; i < transformNamesArray.Length; ++i)
            {
                Transform[] transforms = this.part.FindModelTransforms(transformNamesArray[i]);
                if (transforms.Length == 0 || transforms[0] == null) Utils.LogError("unable to find transform {0}", transformNamesArray[i]);
                else
                {
                    int c = 0;
                    for (int j = 0; j < transforms.Length; ++j)
                    {
                        if (transforms[j] != null)
                        {
                            ++count;
                            ++c;
                            if (transformsOption == TransformOption.USE_FIRST) break;
                        }
                    }
                    if (Global.Debug1) Utils.Log("found transform {0}, {1} times", transformNamesArray[i], c);
                }
            }
            if (Global.Debug1) Utils.Log("found {0} usable transforms", count);

            return count;
        }

        private void fillTransformsArray()
        {
            int count = 0;
            for (int i = 0; i < transformNamesArray.Length; ++i)
            {
                Transform[] transforms = this.part.FindModelTransforms(transformNamesArray[i]);
                for (int j = 0; j < transforms.Length; ++j)
                {
                    if (transforms[j] != null)
                    {
                        _transformsArray[count] = transforms[j];
                        ++count;
                        if (transformsOption == TransformOption.USE_FIRST) break;
                    }
                }
            }
        }

        private void findTransforms()
        {
            fillTransformNameArray();

            int useableTransforms = countUseableTransforms();
            _transformsArray = new Transform[useableTransforms];

            fillTransformsArray();
        }

        public override void OnSave(ConfigNode node)
        {
            if (Global.Debug2) Utils.Log("OnSave start");

            if (boundingBox != null) boundingBox.save(node);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (Global.Debug2) Utils.Log("OnLoad start");

            if (node.HasNode("ASP_BOUNDINGBOX"))
            {
                if (boundingBox == null) boundingBox = new BoundingBox();
                ConfigNode bbNode = node.GetNode("ASP_BOUNDINGBOX");
                boundingBox.load(bbNode);
            }
        }

        public override void OnStart(StartState state)
        {
            if (Global.Debug2) Utils.Log("OnStart start");
            base.OnStart(state);

            if (state == StartState.Editor)
            {
                this.part.OnEditorDestroy += OnEditorDestroy;
            }

            _ok = false;
            try
            {
                if (Global.Debug1) Utils.Log("part {0}", this.part.name);

                findTransforms();

                if (boundingBox == null) boundingBox = new BoundingBox();

                _ok = true;
            }
            catch
            {
                Utils.LogError("Something went wrong in OnStart disabling plugin");
            }
        }
    }
}
