using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    [RequireComponent(typeof(Collider))]
    public class MoveOnMouseDown : MonoBehaviour
    {
        public Color m_touchedColor = Color.red;
        public string m_colorName = "_BaseColor";

        [Tooltip("The layers that can be hit, set to None if you are not using a plane to set your object on")]
        public LayerMask hitLayers;

        Color m_originalColor;
        MeshRenderer m_material;
        Collider m_collider;

        Vector3 screenPosition;
        Vector3 offset;

        private void Awake()
        {
            m_material = this.GetComponentInChildren<MeshRenderer>();
            m_originalColor = m_material.material.GetColor(m_colorName);
            m_collider = this.GetComponent<Collider>();

        }

        private void OnMouseDrag()
        {
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;

            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, hitLayers))
            {
                this.transform.position = hit.point;
            }
            else
            {
                //track mouse position.
                Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);

                //convert screen position to world position with offset changes.
                Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;

                this.transform.position = currentPosition;
            }

        }

        void OnMouseDown()
        {
            m_collider.enabled = false;
            //Convert world position to screen position.
            screenPosition = Camera.main.WorldToScreenPoint(this.transform.position);

            offset = this.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));

            m_material.material.SetColor(m_colorName, m_touchedColor);
        }

        private void OnMouseUp()
        {
            m_collider.enabled = true;
            m_material.material.SetColor(m_colorName, m_originalColor);

        }
    }

}