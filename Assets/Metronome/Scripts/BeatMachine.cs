using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Metronome))]
public class BeatMachine : MonoBehaviour
{
    private static BeatMachine _instance;

    public static BeatMachine Instance { get { return _instance; } }

    public int m_patternCount = 3;
    public int m_measureCount = 4;

    [HideInInspector]
    public int m_currentBeat = 0;

    public delegate void PatternChange(BeatPattern beatPattern);
    public static event PatternChange OnPatternChange;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void OnEnable()
    {
        Metronome.OnBeat += BeatCount;
        Metronome.OnDownBeat += BeatCount;
    }

    private void OnDisable()
    {
        Metronome.OnBeat -= BeatCount;
        Metronome.OnDownBeat -= BeatCount;
    }

    void BeatCount()
    {
        m_currentBeat++;

        if (m_currentBeat >= Metronome.Instance.signatureHi * m_measureCount)
            m_currentBeat = 0;
    }

    public void DispatchPatterns(BeatPattern bp)
    {
        if (OnPatternChange != null)
            OnPatternChange(bp);
    }


}
