using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beats
{
    [RequireComponent(typeof(Text))]
    public class FadeTextAfterDelay : MonoBehaviour
    {
        public float m_delay = 2f;
        public float m_fadeTime = 1.0f;
        float m_startTime = 0;

        Text m_text;
        bool m_isFading = false;
        Color m_startColor;

        // Use this for initialization
        void Awake()
        {
            m_text = this.GetComponent<Text>();
            m_startColor = m_text.color;
        }

        private void OnEnable()
        {
            Invoke("StartFade", m_delay);
        }

        private void OnDisable()
        {
            m_isFading = false;
            m_text.color = m_startColor;
        }

        void StartFade()
        {
            m_isFading = true;
            m_startTime = Time.time;
        }
        void Update()
        {
            if (!m_isFading)
                return;

            m_text.transform.Translate(0, Time.deltaTime * 1.0f, 0);

            //Compute and set the alpha value
            float newAlpha = 1.0f - (Time.time - m_startTime) / m_fadeTime;
            m_text.color = new Color(m_startColor.r, m_startColor.g, m_startColor.b, newAlpha);

            if (newAlpha <= 0)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
