using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class PaintPointer : MonoBehaviour
    {
        static private string _pointerModel = "ASP/TheWriteStuff/Parts/PaintBottle/PaintBrush";
        static private float _offset = 0.2f;
        static private Vector3 _rotation = new Vector3(0f, -1f, 0f);
        static float _maxDist = 5.0f;

        private ASPPainter _painter;
        private GameObject _pointer = null;
        private MeshRenderer[] _meshRenderers;
        private ASPTextWriter _textWriter;

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
        }

        void Update()
        {
            RaycastHit rayCastHit = new RaycastHit();
            Ray ray = FlightCamera.fetch.mainCamera.ScreenPointToRay(Input.mousePosition);
            bool hit = Physics.Raycast(ray, out rayCastHit, 50);
            Color color = Color.blue;
            float dist = 99f;

            if (hit)
            {
                _pointer.transform.position = rayCastHit.point - (ray.direction * _offset);
                dist = (_pointer.transform.position - _painter.part.transform.position).sqrMagnitude;

                if (rayCastHit.rigidbody && dist < _maxDist)
                {
                    _textWriter = rayCastHit.rigidbody.GetComponentInChildren<ASPTextWriter>() as ASPTextWriter;
                    if (_textWriter == null) color = Color.red;
                    else color = Color.green;

                    Vector3 direction = rayCastHit.rigidbody.transform.position - _pointer.transform.position;
                    Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.LookRotation(-_rotation);
                    _pointer.transform.rotation = rotation;
                }
            }
            else _pointer.transform.position = ray.GetPoint(50);

            color.a = 0.5f;
            foreach (MeshRenderer mesh in _meshRenderers)
            {
                mesh.material.color = color;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (hit && rayCastHit.rigidbody && dist < _maxDist && _textWriter != null)
                {
                    _textWriter.editTextEvent();
                    Destroy(_pointer);
                    Destroy(this);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                ScreenMessages.PostScreenMessage("Mouse 1 pressed", 5, ScreenMessageStyle.UPPER_CENTER);
                Destroy(_pointer);
                Destroy(this);
            }
        }
    }
}
