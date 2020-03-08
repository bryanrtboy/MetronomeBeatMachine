using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    [RequireComponent(typeof(Collider))]
    public class LookAtMe : MonoBehaviour
    {
        public Color m_touchedColor = Color.red;

        bool isTarget = false;
        Color m_originalColor;
        SpriteRenderer m_spriteRenderer;

        SmoothFollow m_smoothFollow;


        private void Awake()
        {
            m_spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
            m_originalColor = m_spriteRenderer.color;

            m_smoothFollow = FindObjectOfType<SmoothFollow>();

            if (m_smoothFollow == null)
                Destroy(this);
        }

        private void OnMouseUp()
        {
            ToggleConnection();

        }


        void ToggleConnection()
        {
            isTarget = !isTarget;

            if (isTarget)
            {
                m_smoothFollow.MakeTarget(this.transform.position, this);
                m_spriteRenderer.color = m_touchedColor;
            }
            else
            {
                m_spriteRenderer.color = m_originalColor;
                m_smoothFollow.RemoveTarget();
            }
        }

        public void ReleaseMe()
        {
            m_spriteRenderer.color = m_originalColor;
            isTarget = false;
        }

    }

}