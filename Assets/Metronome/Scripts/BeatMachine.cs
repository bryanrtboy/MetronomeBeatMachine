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
using System.Linq;


[RequireComponent(typeof(Metronome))]
public class BeatMachine : MonoBehaviour
{

    public int m_beatsPerMeasure = 4;
    public int m_measureCount = 4;

    public Metronome m_metronome;
    public PatternSet m_currentPatternSet;
    public GameObject[] m_soundBankPrefabs;

    public string[] m_savedFilenames;
    public string[] m_defaultFilenames;

    public int m_currentBeat = 0;
    public string m_saveAs = "settings";
    public string m_load = "default";
    public string m_currentSoundLabel = "Chimes";

    public delegate void PatternChange(BeatPattern beatPattern);
    public static event PatternChange OnPatternChange;

    private void OnEnable()
    {
        ClearAllNotesFromScene();
        LoadPatternSetFromDisc();
        UpdateMenus();
        GetSoundBankPrefabsFromName(m_currentSoundLabel);
        MakeNoteGameObjects();

        Metronome.OnBeat += BeatCount;
        Metronome.OnDownBeat += BeatCount;
    }

    private void OnDisable()
    {
        Metronome.OnBeat -= BeatCount;
        Metronome.OnDownBeat -= BeatCount;
    }

    public void UpdateMenus()
    {
        m_savedFilenames = GetFileNames(Application.persistentDataPath + "/");
        m_defaultFilenames = GetFileNames(Application.streamingAssetsPath + "/DefaultPatternSets/");
    }
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

    public void GetSoundBankPrefabsFromName(string soundBankName)
    {
        ClearAllNotesFromScene();

        m_currentSoundLabel = soundBankName;
        string path = soundBankName + "/";

        m_soundBankPrefabs = Resources.LoadAll("SoundBanks/" + path + "/", typeof(GameObject)).Cast<GameObject>().ToArray();
    }


    public void SavePatternSetToDisc(List<BeatPattern> patterns)
    {
        //List<BeatPattern> patterns = GetCurrentPatterns(_PatternSetElement, "pattern-list-container");

        PatternSet toSave = new PatternSet();
        toSave.patterns = new List<BeatPattern>();
        toSave.tempo = m_metronome.bpm;
        toSave.signatureHi = m_metronome.signatureHi;
        toSave.measures = m_measureCount;
        toSave.soundBank = m_currentPatternSet.soundBank;

        foreach (BeatPattern bp in patterns)
            toSave.patterns.Add(bp);

        string savedPatterns = JsonUtility.ToJson(toSave);

        File.WriteAllText(Application.persistentDataPath + "/" + m_saveAs + ".json", savedPatterns);

        //Refresh the files directory
        m_savedFilenames = GetFileNames(Application.persistentDataPath + "/");
    }


    public void LoadPatternSetFromDisc()
    {
        string path = Application.persistentDataPath + "/" + m_load + ".json";

        //Try to Use the default patterns if there are no saved patterns
        if (!File.Exists(path))
        {
            string newPath = Application.streamingAssetsPath + "/DefaultPatternSets/default.json";

            if (!File.Exists(newPath))
            {
                Debug.LogError("There are no saved patternsets at " + path + " and " + newPath + " and I can't find any default patterns to use!");
                return;
            }
            else
            {
                path = newPath;
            }
        }

        string settings = File.ReadAllText(path);
        m_currentPatternSet = JsonUtility.FromJson<PatternSet>(settings);
    }

    public string[] GetFileNames(string path)
    {
        string[] array = Directory.GetFiles(path);

        List<string> names = new List<string>();

        for (int i = 0; i < array.Length; i++)
        {
            string name = Path.GetFileName(array[i]);

            if (!name.EndsWith(".meta"))
            {
                name = name.Substring(0, name.Length - 5);
                if (name != ".DS_")
                    names.Add(name);
            }
        }

        return names.ToArray();
    }

    public string[] GetFolderNames(string path)
    {
        string[] parentDirectory = Directory.GetDirectories(path);
        List<string> directories = new List<string>();

        foreach (var directory in parentDirectory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);

            string name = dirInfo.Name;
            directories.Add(name);
        }
        return directories.ToArray();
    }

    // copies audiosource properties to temp audiosource for playing at a position
    public static AudioSource PlayClipAtPoint(AudioSource audioSource, AudioClip clip, Vector3 pos, float volume)
    {
        GameObject tempGO = new GameObject(clip.name); // create the temp object
        tempGO.transform.position = pos; // set its position
        AudioSource tempASource = tempGO.AddComponent<AudioSource>(); // add an audio source
        tempASource.clip = clip;
        tempASource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
        tempASource.mute = audioSource.mute;
        tempASource.bypassEffects = audioSource.bypassEffects;
        tempASource.bypassListenerEffects = audioSource.bypassListenerEffects;
        tempASource.bypassReverbZones = audioSource.bypassReverbZones;
        tempASource.playOnAwake = audioSource.playOnAwake;
        tempASource.loop = audioSource.loop;
        tempASource.priority = audioSource.priority;
        tempASource.volume = volume;
        tempASource.pitch = audioSource.pitch;
        tempASource.panStereo = audioSource.panStereo;
        tempASource.spatialBlend = audioSource.spatialBlend;
        tempASource.reverbZoneMix = audioSource.reverbZoneMix;
        tempASource.dopplerLevel = audioSource.dopplerLevel;
        tempASource.rolloffMode = audioSource.rolloffMode;
        tempASource.minDistance = audioSource.minDistance;
        tempASource.spread = audioSource.spread;
        tempASource.maxDistance = audioSource.maxDistance;
        // set other aSource properties here, if desired
        tempASource.Play(); // start the sound
        MonoBehaviour.Destroy(tempGO, tempASource.clip.length); // destroy object after clip duration (this will not account for whether it is set to loop)
        return tempASource; // return the AudioSource reference
    }


}

