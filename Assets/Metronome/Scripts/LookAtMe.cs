using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    [RequireComponent(typeof(Collider))]
    public class LookAtMe : MonoBehaviour
    {
        public Color m_touchedColor = Color.red;
        public GameObject m_objectToLookAt;

        bool m_isTarget = false;
        Color m_originalColor;
        SpriteRenderer m_spriteRenderer;

        SmoothFollow m_smoothFollow;
        LookToggle m_lookToggle;


        private void Awake()
        {
            m_spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
            m_originalColor = m_spriteRenderer.color;
            m_lookToggle = this.GetComponent<LookToggle>();

            if (m_objectToLookAt == null)
                m_objectToLookAt = this.gameObject;

            m_smoothFollow = FindObjectOfType<SmoothFollow>();

        }

        private void OnMouseUp()
        {
            ToggleConnection();

        }


        void ToggleConnection()
        {
            m_isTarget = !m_isTarget;

            if (m_isTarget)
            {
                if (m_smoothFollow)
                    m_smoothFollow.MakeTarget(m_objectToLookAt, this);
                m_spriteRenderer.color = m_touchedColor;
                if (m_lookToggle)
                    m_lookToggle.ToggleToClickedState();
            }
            else
            {
                m_spriteRenderer.color = m_originalColor;
                if (m_smoothFollow)
                    m_smoothFollow.RemoveTarget();
                if (m_lookToggle)
                    m_lookToggle.ToggleToStartState();
            }
        }

        public void ReleaseMe()
        {
            m_spriteRenderer.color = m_originalColor;
            m_isTarget = false;
            if (m_lookToggle)
                m_lookToggle.ToggleToStartState();
        }

    }

}