using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Beats
{
    [RequireComponent(typeof(LineRenderer))]
    public class Connector : MonoBehaviour
    {
        public bool m_isConnected = false;
        public float m_maxLineWidth = .1f;
        public float m_diconnectDistance = .25f;
        public TMP_Text m_text;
        LineRenderer m_line;
        public Renderer m_noteRenderer;

        //[HideInInspector]
        public Note m_note; //This gets set at launch time from Node.cs
        Node m_node;


        Color m_materialColor = Color.black;
        string m_colorToChange = "_EmissionColor";

        private void Awake()
        {
            m_node = this.transform.parent.GetComponentInParent<Node>();

            if (m_node == null)
            {
                Debug.LogError("This script requires:  Node/NodeCenter/Connector . With the Node having a Node.cs script attached. Destroying myself now.");
                Destroy(this);
            }
            if (m_noteRenderer == null)
                m_noteRenderer = this.GetComponentInChildren<Renderer>();

            m_materialColor = m_noteRenderer.material.GetColor(m_colorToChange);
            m_noteRenderer.material.EnableKeyword("_EMISSION");


            m_line = this.GetComponent<LineRenderer>();

            if (m_text)
                m_text.gameObject.SetActive(false);

        }

        public void InitialSetup()
        {
            this.transform.LookAt(m_node.transform);

            this.transform.localPosition = Vector3.zero;

            float dist = Mathf.Lerp(m_diconnectDistance, .1f, m_note._volume);
            //Debug.Log(this.transform.name + " of " + " should be " + dist.ToString("F4") + " from " + m_node.name);

            this.transform.Translate(Vector3.back * dist);
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

            Color c = m_noteRenderer.material.GetColor(m_colorToChange);

            if (c != m_materialColor)
                m_noteRenderer.material.SetColor(m_colorToChange, Color.Lerp(c, m_materialColor, Time.deltaTime * 20));
        }

        public float GetDistance()
        {
            return Vector3.Distance(this.transform.position, m_node.transform.position);
        }

        public void PlayTheClip(Color c, AudioSource audioSrc, float volume)
        {
            if (!m_isConnected)
                return;

            if (m_note._clip == null)
            {
                Debug.LogWarning(this.transform.parent.name + " did not pass an audio clip to " + this.name);
                return;
            }
            AudioClipMaker.PlayClipAtPoint(audioSrc, m_note._clip, this.transform.position, volume);
            m_noteRenderer.material.SetColor(m_colorToChange, c);

        }

        void SetLineWidth()
        {

            m_note._volume = Mathf.InverseLerp(m_diconnectDistance, .1f, GetDistance());

            if (m_text != null)
            {
                if (!m_text.gameObject.activeSelf)
                    m_text.gameObject.SetActive(true);

                m_text.transform.LookAt(Camera.main.transform);
                if (m_note != null && m_note._clip)
                    m_text.text = m_note._clip.name + "\nvol " + m_note._volume.ToString("F2");
            }

            float w = Mathf.Lerp(0f, m_maxLineWidth, m_note._volume);
            //Debug.Log(w);

            m_line.SetPosition(0, m_node.transform.position);
            m_line.SetPosition(1, this.transform.position);

            m_line.startWidth = w;
            m_line.endWidth = w;
        }
    }


}
