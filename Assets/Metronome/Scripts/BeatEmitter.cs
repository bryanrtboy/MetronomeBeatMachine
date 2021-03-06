﻿//Bryan Leister Feb. 2020
//This script is attached to each prefab that we use to generate our notes
//It needs the note:  m_clip
//And subscribes to the Beat and DownBeat events
//The scene must include a Metronome and a BeatMachine
//THe BeatMachine generates the notes and tells them when a new
//beat pattern exists.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    public class BeatEmitter : MonoBehaviour
    {
        public ParticleSystem m_vfx;
        public AudioClip m_clip;
        public bool m_useRandom = true;
        [Range(0.01f, .99f)]
        public float m_randomChance = .5f;
        public int m_patternID = 0;

        public List<bool> m_pattern;
        public BeatMachine m_beatMachine;
        public Metronome m_metronome;

        //Use these strings to call VFX Graph System Properties
        //public string m_beats = "BeatParticles";
        //public string m_downbeat = "DownBeatParticles";

        Light m_light;

        public virtual void Awake()
        {
            m_vfx = this.GetComponentInChildren<ParticleSystem>();
            m_light = this.GetComponentInChildren<Light>();
            m_pattern = new List<bool>();
            m_beatMachine = FindObjectOfType<BeatMachine>();
            m_metronome = FindObjectOfType<Metronome>();

        }

        public virtual void OnEnable()
        {
            Metronome.OnBeat += Beat;
            Metronome.OnDownBeat += DownBeat;
            BeatMachine.OnPatternChange += UpdatePattern;

            StartCoroutine(ResetBurst());
        }

        public virtual void OnDisable()
        {
            Metronome.OnBeat -= Beat;
            Metronome.OnDownBeat -= DownBeat;
            BeatMachine.OnPatternChange -= UpdatePattern;
        }

        public virtual void Update()
        {
            if (m_light != null && m_light.intensity > 0.01f)
                m_light.intensity = Mathf.Lerp(m_light.intensity, 0, Time.deltaTime * 20f);
        }

        public virtual void Beat()
        {
            //If we are using randomness, do stuff here
            if (m_useRandom && Random.Range(0.0f, 1.0f) > m_randomChance)
                return;

            //If we have a beat pattern, do stuff here (makes sense to NOT use randomness if we are using patterns, but YMMV
            if (m_pattern.Count > 0 && !ShallWePlay())
            {
                return;
            }

            AudioClipMaker.PlayClipAtPoint(m_metronome.m_audioSource, m_clip, this.transform.position, .5f);

            if (m_vfx)
                m_vfx.Emit(100);

            //m_vfx.SetInt(m_downbeat, 300);
            //StartCoroutine(ResetBurst());

            LightFlash(.5f, new Color(.1f, .1f, 1f));

        }

        public virtual void DownBeat()
        {

            if (m_useRandom && Random.Range(0.0f, 1.0f) > m_randomChance)
                return;

            if (m_pattern.Count > 0 && !ShallWePlay())
                return;

            AudioClipMaker.PlayClipAtPoint(m_metronome.m_audioSource, m_clip, this.transform.position, .5f);


            if (m_vfx)
                m_vfx.Emit(1000);

            //m_vfx.SetInt(m_downbeat, 3000);
            //StartCoroutine(ResetBurst());

            LightFlash(1f, Color.white);
        }

        IEnumerator ResetBurst()
        {
            yield return new WaitForEndOfFrame();

            //If using a VFX Graph, reset the appropriate system
            //m_vfx.SetInt(m_beats, 0);
            //m_vfx.SetInt(m_downbeat, 0);
        }

        void LightFlash(float _intensity, Color _color)
        {
            if (m_light == null)
                return;

            m_light.intensity = _intensity;
            m_light.color = _color;
        }

        public void UpdatePattern(BeatPattern bp)
        {
            if (bp.m_beatID == m_patternID)
            {
                m_pattern = bp.m_beatPattern;
            }
        }

        public bool ShallWePlay()
        {
            if (m_beatMachine != null && m_beatMachine.m_currentBeat > m_pattern.Count - 1)
            {
                Debug.Log("beat pattern is smaller than current beat count!");
                Debug.Log("Current beat is " + m_beatMachine.m_currentBeat + " " + this.name + ": pattern count is " + m_pattern.Count + " for " + m_beatMachine.m_saveAs);
                return false;
            }



            bool shouldPlayOnThisBeat = m_pattern[m_beatMachine.m_currentBeat];
            return shouldPlayOnThisBeat;
        }

    }

}