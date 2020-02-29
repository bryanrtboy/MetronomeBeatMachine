using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    [RequireComponent(typeof(Collider))]
    public class MoveOnMouseDown : MonoBehaviour
    {
        //public float m_cameraPlaneOffset = 1f;
        public Color m_touchedColor = Color.red;
        public string m_colorName = "_BaseColor";

        private Collider m_collider;
        private bool m_isGrabbed = false;
        private Camera cam;
        float zOffset = 1;
        Color m_originalColor;
        public MeshRenderer m_material;

        private void Awake()
        {
            m_collider = this.GetComponent<Collider>();
            cam = Camera.main;

            m_material = this.GetComponentInChildren<MeshRenderer>();
            m_originalColor = m_material.material.GetColor(m_colorName);

            //ScreenDrawing s = FindObjectOfType<ScreenDrawing>();
            //if (s)
            //    m_cameraPlaneOffset = s.m_zOffset;
        }


        void OnDisable()
        {
            m_collider.enabled = false;
        }


        void Update()
        {
            if (m_isGrabbed)
            {

                Vector2 mousePos = new Vector2();

                mousePos.x = Input.mousePosition.x;
                mousePos.y = Input.mousePosition.y;

                this.transform.position = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, zOffset));
                //this.transform.position = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            }

        }

        void OnMouseDown()
        {
            m_isGrabbed = true;
            zOffset = Vector3.Distance(cam.transform.position, this.transform.position);
            m_material.material.SetColor(m_colorName, m_touchedColor);

        }

        private void OnMouseUp()
        {
            m_isGrabbed = false;
            m_material.material.SetColor(m_colorName, m_originalColor);
        }
    }

}