using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BeatEmitter : MonoBehaviour
{
    public ParticleSystem m_vfx;
    public string m_beats = "BeatParticles";
    public string m_downbeat = "DownBeatParticles";
    public bool m_playOnDownBeat = true;
    public AudioClip m_clip;
    public bool m_useRandom = true;
    [Range(0.01f, .99f)]
    public float m_randomChance = .5f;

    Light m_light;

    private void Awake()
    {
        m_vfx = this.GetComponentInChildren<ParticleSystem>();
        m_light = this.GetComponentInChildren<Light>();
    }

    private void OnEnable()
    {
        Metronome.OnBeat += Beat;
        Metronome.OnDownBeat += DownBeat;

        StartCoroutine(ResetBurst());
    }

    private void OnDisable()
    {
        Metronome.OnBeat -= Beat;
        Metronome.OnDownBeat -= DownBeat;
    }

    private void Update()
    {
        if (m_light != null && m_light.intensity > 0.01f)
            m_light.intensity = Mathf.Lerp(m_light.intensity, 0, Time.deltaTime * 20f);
    }

    void Beat()
    {
        if (m_useRandom && Random.Range(0.0f, 1.0f) > m_randomChance)
            return;

        AudioSource.PlayClipAtPoint(m_clip, this.transform.position, .5f);

        m_vfx.Emit(100);
        StartCoroutine(ResetBurst());

        LightFlash(.5f, new Color(.1f, .1f, 1f));

    }

    void DownBeat()
    {
        if (!m_playOnDownBeat)
            return;

        if (m_useRandom && Random.Range(0.0f, 1.0f) > m_randomChance)
            return;

        AudioSource.PlayClipAtPoint(m_clip, this.transform.position, 1f);
        m_vfx.Emit(1000);
        StartCoroutine(ResetBurst());

        LightFlash(1f, Color.white);
    }


    IEnumerator ResetBurst()
    {
        yield return new WaitForEndOfFrame();
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
}
