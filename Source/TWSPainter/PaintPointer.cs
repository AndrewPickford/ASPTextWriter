using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class PaintPointer : MonoBehaviour
    {
        static private string _pointerModel = "ASP/TheWriteStuff/Parts/PaintBottle/ASP_PaintBrush";
        static private float _offset = 0.2f;
        static private Vector3 _rotation = new Vector3(0f, -1f, 0f);
        static float _maxDist = 5.0f;

        private ASPPainter _painter;
        private GameObject _pointer = null;
        private MeshRenderer[] _meshRenderers;
        ASPTextureEdit[] _textureEditors = null;

        public void initialise(ASPPainter painter)
        {
            _painter = painter;

            ScreenMessages.PostScreenMessage("Paint Pointer - RMB or Escape to exit", 5, ScreenMessageStyle.UPPER_CENTER);

            if (_pointer == null)
            {
                GameObject modelObject = GameDatabase.Instance.GetModel(_pointerModel).gameObject;
                _pointer = Instantiate(modelObject) as GameObject;
                
                Collider[] colliders = _pointer.GetComponentsInChildren<Collider>(true);
                foreach (Collider collider in colliders)
                {
                    Destroy(collider);
                }

                _meshRenderers = _pointer.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mesh in _meshRenderers)
                {
                    mesh.material = new Material(Shader.Find("Transparent/Diffuse"));
                }

                _pointer.SetActive(true);
            }

            GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(this.OnVesselChange));
        }

        void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(new EventData<Vessel>.OnEvent(this.OnVesselChange));
        }

        void OnVesselChange(Vessel vesselChange)
        {
            Destroy(_pointer);
            Destroy(this);
        }

        void Update()
        {
            RaycastHit rayCastHit = new RaycastHit();
            Ray ray = FlightCamera.fetch.mainCamera.ScreenPointToRay(Input.mousePosition);
            bool hit = Physics.Raycast(ray, out rayCastHit, 50);
            Color color = Color.yellow;
            float dist = 99f;

            if (hit)
            {
                _pointer.transform.position = rayCastHit.point - (ray.direction * _offset);
                dist = (_pointer.transform.position - _painter.part.transform.position).sqrMagnitude;

                if (rayCastHit.rigidbody && dist < _maxDist)
                {
                    _textureEditors = rayCastHit.rigidbody.GetComponentsInChildren<ASPTextureEdit>() as ASPTextureEdit[];
                    if (_textureEditors == null) color = Color.red;
                    else
                    {
                        if (_textureEditors.Length == 0) color = Color.red;
                        else
                        {
                            if (_textureEditors[0] == null) color = Color.red;
                            else color = Color.green;
                        }
                    }

                    Vector3 direction = rayCastHit.rigidbody.transform.position - _pointer.transform.position;
                    Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.LookRotation(-_rotation);
                    _pointer.transform.rotation = rotation;
                }
            }
            else
            {
                _pointer.transform.position = ray.GetPoint(50);
                _textureEditors = null;
            }

            color.a = 0.5f;
            foreach (MeshRenderer mesh in _meshRenderers)
            {
                mesh.material.color = color;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (hit && rayCastHit.rigidbody && dist < _maxDist && _textureEditors != null)
                {
                    if (Global.Debug2) Utils.Log("found {0} ASPTextureEdit modules", _textureEditors.Length);
                    foreach (ASPTextureEdit textureEdit in _textureEditors)
                    {
                        if (textureEdit != null)
                        {
                            if (Global.Debug3) Utils.Log("opening texture edit gui {0}", textureEdit.transforms);
                            textureEdit.setPainter(_painter.gameObject);
                            textureEdit.editTextureEvent();
                        }
                    }
                    Destroy(_pointer);
                    Destroy(this);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape))
            {
                ScreenMessages.PostScreenMessage("Paint mode cancelled", 5, ScreenMessageStyle.UPPER_CENTER);
                Destroy(_pointer);
                Destroy(this);
            }
        }
    }
}
