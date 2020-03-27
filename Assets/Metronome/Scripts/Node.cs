using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Beats
{

    public class Node : BeatEmitter
    {
        public Color m_beatColor = Color.white;
        public Color m_downBeatColor = Color.red;
        public float m_disconnectDistance = .25f;
        public Renderer m_nodeRenderer;
        [Tooltip("Use this area to set up the Node and Node values. Setting the volume at 0 will move the node away from the parent so it is not connected at launch. A higher value will connnect it and play at launch.")]
        public List<Note> m_connectedNotes;

        public UnityEvent m_onBeat;
        public UnityEvent m_onDownBeat;

        Color m_materialColor = Color.black;
        string m_colorToChange = "_EmissionColor";
        List<GameObject> m_orbiters;

        public override void Awake()
        {
            m_pattern = new List<bool>();

            m_beatMachine = FindObjectOfType<BeatMachine>();
            m_metronome = FindObjectOfType<Metronome>();

            if (m_nodeRenderer == null)
                m_nodeRenderer = this.GetComponentInChildren<Renderer>();

            m_materialColor = m_nodeRenderer.material.GetColor(m_colorToChange);
            m_nodeRenderer.material.EnableKeyword("_EMISSION");

            m_orbiters = new List<GameObject>();
        }

        public override void OnEnable()
        {
            Metronome.OnBeat += Beat;
            Metronome.OnDownBeat += DownBeat;
            BeatMachine.OnPatternChange += UpdatePattern;

            Invoke("SetUpConnnectedNotes", .1f);
            //SetUpConnnectedNotes();
        }

        public override void OnDisable()
        {
            Metronome.OnBeat -= Beat;
            Metronome.OnDownBeat -= DownBeat;
            BeatMachine.OnPatternChange -= UpdatePattern;
        }

        public override void Update()
        {
            this.transform.LookAt(Camera.main.transform.position);

            Color c = m_nodeRenderer.material.GetColor(m_colorToChange);

            if (c != m_materialColor)
                m_nodeRenderer.material.SetColor(m_colorToChange, Color.Lerp(c, m_materialColor, Time.deltaTime * 20));

        }

        private void LateUpdate()
        {
            if (m_orbiters.Count == 0)
                return;

            for (int i = 0; i < m_orbiters.Count; i++)
            {
                float m_radius = Mathf.Lerp(.25f, .1f, m_connectedNotes[i]._volume);
                float m_speed = Mathf.Lerp(0f, (float)m_metronome.bpm * .02f, m_connectedNotes[i]._volume);

                //if (i % 2 == 0)
                //    m_orbiters[i].transform.position = new Vector3(this.transform.position.x + Mathf.Cos(Time.time * m_speed * m_radius) * m_radius, this.transform.position.y + Mathf.Sin(Time.time * m_speed * m_radius) * m_radius, this.transform.position.z);
                //else
                m_orbiters[i].transform.position = new Vector3(this.transform.position.x + Mathf.Cos(Time.time * m_speed) * m_radius, this.transform.position.y, this.transform.position.z + Mathf.Sin(Time.time * m_speed) * m_radius);

            }
        }

        void SetUpConnnectedNotes()
        {
            m_orbiters = new List<GameObject>();

            foreach (Note cn in m_connectedNotes)
            {
                cn._connector.m_note = cn;
                cn._connector.InitialSetup();

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = new Vector3(.01f, .01f, .01f);
                sphere.transform.parent = this.transform;
                m_orbiters.Add(sphere);
            }

        }

        public override void Beat()
        {

            //If we have a beat pattern, do stuff here (makes sense to NOT use randomness if we are using patterns, but YMMV
            if (m_pattern.Count > 0 && !ShallWePlay())
                return;

            m_onBeat.Invoke();

            foreach (Note cn in m_connectedNotes)
            {
                //Checking the sqrMagnitude is fastest distance check, if the actual distance is about 3, the sqrMagnitude is .3
                cn._connector.m_isConnected = Vector3.Distance(this.transform.position, cn._connector.transform.position) < m_disconnectDistance;

                if (cn._connector.m_isConnected && cn._connector.m_note._clip != null)
                    cn._connector.PlayTheClip(m_beatColor, m_metronome.m_audioSource, cn._volume);


            }

            m_nodeRenderer.material.SetColor(m_colorToChange, m_beatColor);

        }

        public override void DownBeat()
        {

            if (m_pattern.Count > 0 && !ShallWePlay())
                return;

            m_onDownBeat.Invoke();

            foreach (Note cn in m_connectedNotes)
            {
                cn._connector.m_isConnected = Vector3.Distance(this.transform.position, cn._connector.transform.position) < m_disconnectDistance;

                if (cn._connector.m_isConnected)
                    cn._connector.PlayTheClip(m_beatColor, m_metronome.m_audioSource, cn._volume * 2);
            }

            m_nodeRenderer.material.SetColor(m_colorToChange, m_downBeatColor);

        }
    }

    [System.Serializable]
    public class Note
    {
        public Connector _connector;
        public AudioClip _clip;
        public float _volume = 0;
    }
}

