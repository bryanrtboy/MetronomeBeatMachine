//Bryan Leister Feb. 2020
//This script is to set up a beat machine/step sequencer in the Editor
//It requires the scripts in the Editor folder to overwrite the Unity Editor
//with custom UIElements to generate the interface
//
//Instructions: Drag this onto a Gameobject in the scene that has a Metronome
//script attached to it.
//
//When you select the fields, an editor script runs and updates everything
//most importantly, the signatureHi value is changed in the Metronome when
//It is changed here

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Metronome))]
public class BeatMachine : MonoBehaviour
{

    public int m_beatsPerMeasure = 4;
    public int m_measureCount = 4;

    public Metronome m_metronome;
    public PatternSet m_currentPatternSet;
    public GameObject[] m_soundBankPrefabs;

    public int m_currentBeat = 0;
    public string m_saveAs = "settings";
    public string m_load = "default";
    public string m_currentSoundLabel = "Chimes";

    public delegate void PatternChange(BeatPattern beatPattern);
    public static event PatternChange OnPatternChange;

    #region Initialization Setup the singleton and subscribe to events


    private void OnEnable()
    {
        //ClearAllNotesFromScene();
        Metronome.OnBeat += BeatCount;
        Metronome.OnDownBeat += BeatCount;
    }

    private void OnDisable()
    {
        Metronome.OnBeat -= BeatCount;
        Metronome.OnDownBeat -= BeatCount;
    }
    #endregion

    //All of the BeatEmitters listen to this beatcount and update based on where we are in the loop
    void BeatCount()
    {
        m_currentBeat++;

        if (m_currentBeat >= m_metronome.signatureHi * m_measureCount)
            m_currentBeat = 0;
    }

    //When we change the loop pattern, we notify all of the loops about the new
    //patterns. If they have an ID that matches the pattern, they will add the pattern
    //to their BeatEmitter and use it to play notes as directed, i.e. true = play the note at this point
    //in the pattern
    public void DispatchPatterns(BeatPattern bp)
    {
        if (OnPatternChange != null)
            OnPatternChange(bp);
    }

    public void MakeNoteGameObjects()
    {
        if (m_currentPatternSet == null || m_currentPatternSet.patterns.Count < 1)
        {
            Debug.LogWarning("No pattern set or patterns!");
            return;
        }

        for (int i = 0; i < m_soundBankPrefabs.Length; i++)
        {

            if (m_currentPatternSet.patterns.Count > i)
            {

                GameObject note = Instantiate(m_soundBankPrefabs[i], Random.insideUnitSphere + this.transform.position, Quaternion.identity);
                note.tag = "Player";

                BeatEmitter b = note.GetComponent<BeatEmitter>();
                if (b)
                {
                    b.m_patternID = m_currentPatternSet.patterns[i].m_beatID;
                    b.m_playOnDownBeat = true;
                    b.m_useRandom = false;
                }

                DispatchPatterns(m_currentPatternSet.patterns[i]);
            }

        }
    }


    public void ClearAllNotesFromScene()
    {

        GameObject[] notes = GameObject.FindGameObjectsWithTag("Player");

#if UNITY_EDITOR
        foreach (GameObject g in notes)
            DestroyImmediate(g);
#else
        foreach (GameObject g in notes)
            Destroy(g);
#endif
        m_soundBankPrefabs = null;

    }

}

