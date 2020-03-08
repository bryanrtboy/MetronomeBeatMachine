using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    [RequireComponent(typeof(Collider))]
    public class PlaceOnGround : MonoBehaviour
    {
        public Color m_touchedColor = Color.red;
        public string m_colorName = "_BaseColor";
        public GameObject m_objectToPlace;

        [Tooltip("The layer that contains the Ground Plane")]
        public LayerMask groundLayer;

        Color m_originalColor;
        MeshRenderer m_material;


        private void Awake()
        {
            if (m_objectToPlace == null)
            {
                Debug.LogError("We can't place any objects, because you didn't specify an object to place!");
                Destroy(this);
            }

            m_objectToPlace.SetActive(false);
            m_material = this.GetComponentInChildren<MeshRenderer>();
            m_originalColor = m_material.material.GetColor(m_colorName);

        }

        private void OnMouseDrag()
        {
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;

            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, groundLayer))
            {
                m_objectToPlace.transform.position = hit.point;
            }

        }

        void OnMouseDown()
        {
            m_objectToPlace.SetActive(true);
            m_material.material.SetColor(m_colorName, m_touchedColor);
        }

        private void OnMouseUp()
        {
            //m_objectToPlace.SendMessage("OnReleased", SendMessageOptions.DontRequireReceiver);
            m_material.material.SetColor(m_colorName, m_originalColor);

        }
    }

}