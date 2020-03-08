using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    [RequireComponent(typeof(LineRenderer))]
    public class Connector : MonoBehaviour
    {
        public bool m_isConnected = false;
        public float m_maxLineWidth = .2f;

        [HideInInspector]
        public Note m_note; //This gets set at launch time from Node.cs
        Node m_node;
        Renderer m_renderer;
        LineRenderer m_line;
        Color m_materialColor = Color.black;
        string m_colorToChange = "_EmissionColor";

        private void Awake()
        {
            m_node = this.GetComponentInParent<Node>();

            if (m_node == null)
            {
                Debug.LogError("This script requires that the parent have a Node.cs script attached. Destroying myself now.");
                Destroy(this);
            }

            m_renderer = this.GetComponent<Renderer>();
            m_materialColor = m_renderer.material.GetColor(m_colorToChange);
            m_renderer.material.EnableKeyword("_EMISSION");
            m_line = this.GetComponent<LineRenderer>();

        }

        public void InitialSetup()
        {
            this.transform.LookAt(m_node.transform);

            float dist = Mathf.Lerp(3.5f, 0, m_note._volume);
            //Debug.Log(this.transform.parent.name + " " + this.transform.name + " is at " + dist.ToString("F4"));

            this.transform.Translate(Vector3.forward * (dist * this.transform.localScale.z), Space.Self);
        }

        private void Update()
        {
            if (m_isConnected)
            {
                if (!m_line.enabled)
                    m_line.enabled = true;

                SetLineWidth();

            }
            else
            {

                m_line.enabled = false;
            }

            Color c = m_renderer.material.GetColor(m_colorToChange);

            if (c != m_materialColor)
                m_renderer.material.SetColor(m_colorToChange, Color.Lerp(c, m_materialColor, Time.deltaTime * 20));
        }

        float GetDistance()
        {
            return Vector3.Distance(this.transform.position, m_node.transform.position);
        }

        public void PlayTheClip(Color c, AudioSource audioSrc)
        {
            if (m_note._clip == null)
            {
                Debug.LogError(this.transform.parent.name + " did not pass an audio clip to " + this.name);
                return;
            }
            AudioClipMaker.PlayClipAtPoint(audioSrc, m_note._clip, this.transform.position, m_note._volume);
            m_renderer.material.SetColor(m_colorToChange, c);

        }

        void SetLineWidth()
        {
            m_note._volume = Mathf.InverseLerp(1f, 0f, GetDistance() * 2f);

            float w = Mathf.Lerp(0.001f, m_maxLineWidth, m_note._volume);
            //Debug.Log(w);

            m_line.SetPosition(0, m_node.transform.position);
            m_line.SetPosition(1, this.transform.position);

            m_line.startWidth = w;
            m_line.endWidth = w;
        }
    }


}
