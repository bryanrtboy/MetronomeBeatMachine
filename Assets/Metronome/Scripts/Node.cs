using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Beats
{

    public class Node : BeatEmitter
    {
        public Color m_beatColor = Color.white;
        public Color m_downBeatColor = Color.red;
        public float m_disconnectDistance = .35f;

        Vector3 m_startScale = Vector3.one;
        Renderer m_renderer;
        Color m_materialColor = Color.black;
        string m_colorToChange = "_EmissionColor";
        [Tooltip("Use this area to set up the Node and Node values. Setting the volume at 0 will move the node away from the parent so it is not connected at launch. A higher value will connnect it and play at launch.")]
        public List<Note> m_connectedNotes;


        public override void Awake()
        {
            m_pattern = new List<bool>();
            m_beatMachine = FindObjectOfType<BeatMachine>();
            m_metronome = FindObjectOfType<Metronome>();
            m_startScale = this.transform.localScale;
            m_renderer = this.GetComponent<Renderer>();
            m_materialColor = m_renderer.material.GetColor(m_colorToChange);
            m_renderer.material.EnableKeyword("_EMISSION");

        }

        public override void OnEnable()
        {
            Metronome.OnBeat += Beat;
            Metronome.OnDownBeat += DownBeat;
            BeatMachine.OnPatternChange += UpdatePattern;

            Invoke("SetUpConnnectedNotes", .1f);
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

            Color c = m_renderer.material.GetColor(m_colorToChange);

            if (c != m_materialColor)
                m_renderer.material.SetColor(m_colorToChange, Color.Lerp(c, m_materialColor, Time.deltaTime * 20));

        }

        void SetUpConnnectedNotes()
        {
            foreach (Note cn in m_connectedNotes)
            {
                cn._connector.m_note = cn;
                cn._connector.InitialSetup();
            }

        }

        public override void Beat()
        {

            //If we have a beat pattern, do stuff here (makes sense to NOT use randomness if we are using patterns, but YMMV
            if (m_pattern.Count > 0 && !ShallWePlay())
                return;


            foreach (Note cn in m_connectedNotes)
            {
                //Checking the sqrMagnitude is fastest distance check, if the actual distance is about 3, the sqrMagnitude is .3
                cn._connector.m_isConnected = (this.transform.position - cn._connector.transform.position).sqrMagnitude < m_disconnectDistance;

                if (cn._connector.m_isConnected && cn._connector.m_note._clip != null)
                    cn._connector.PlayTheClip(m_beatColor, m_metronome.m_audioSource);


            }

            m_renderer.material.SetColor(m_colorToChange, m_beatColor);

        }

        public override void DownBeat()
        {
            if (!m_playOnDownBeat)
                return;

            if (m_pattern.Count > 0 && !ShallWePlay())
                return;

            foreach (Note cn in m_connectedNotes)
            {
                cn._connector.m_isConnected = (this.transform.position - cn._connector.transform.position).sqrMagnitude < .4f;

                if (cn._connector.m_isConnected)
                    cn._connector.PlayTheClip(m_beatColor, m_metronome.m_audioSource);
            }

            m_renderer.material.SetColor(m_colorToChange, m_downBeatColor);

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

