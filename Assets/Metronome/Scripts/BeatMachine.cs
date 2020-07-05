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

namespace Beats
{
    [RequireComponent(typeof(Metronome))]
    public class BeatMachine : MonoBehaviour
    {
        public float m_spawnSize = .3f;

        public int m_beatsPerMeasure = 4;
        public int m_measureCount = 4;

        public Metronome m_metronome;
        public PatternSet m_currentPatternSet;
        public GameObject[] m_soundBankPrefabs;

        public string m_resourceFolderLocation = "Metronome";
        public string m_soundbankFolderName = "SoundBanks";
        public string[] m_savedFilenames;
        public string[] m_defaultFilenames;

        public int m_currentBeat = 0;
        public string m_saveAs = "settings";
        public string m_load = "default";
        public string m_currentSoundLabel = "Chimes";

        public delegate void PatternChange(BeatPattern beatPattern);
        public static event PatternChange OnPatternChange;

        private void Awake()
        {
            UpdateMenus();
            LoadPatternSetFromDisc();
        }

        private void OnEnable()
        {
            ClearAllNotesFromScene();
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

        public void MakeNotes()
        {

            //LoadPatternSetFromDisc();
            MakeNoteGameObjects(this.transform.position);
        }

        public void MakeNoteGameObjects(Vector3 origin)
        {
            if (m_currentPatternSet == null || m_currentPatternSet.patterns.Count < 1)
            {
                Debug.LogWarning("No pattern set or patterns!");
                return;
            }

            if (m_soundBankPrefabs == null)
                return;

            for (int i = 0; i < m_soundBankPrefabs.Length; i++)
            {
                GameObject note = Instantiate(m_soundBankPrefabs[i], (Random.insideUnitSphere * m_spawnSize) + origin, Quaternion.identity);
                note.tag = "Player";
                note.name = m_currentPatternSet.soundBank + " " + i.ToString() + ": " + m_soundBankPrefabs[i].name;
                note.transform.localScale = Vector3.one;

                BeatEmitter b = note.GetComponentInChildren<BeatEmitter>();
                if (b != null && m_currentPatternSet.patterns.Count > i)
                {
                    b.m_patternID = m_currentPatternSet.patterns[i].m_beatID;
                    b.m_useRandom = false;
                }

                if (m_currentPatternSet.patterns.Count > i)
                    DispatchPatterns(m_currentPatternSet.patterns[i]);

                //m_soundBankPrefabs[i].SetActive(false);
            }
        }

        public void ClearAllNotesFromScene()
        {

            GameObject[] beatOrNodes = GameObject.FindGameObjectsWithTag("Player");

#if UNITY_EDITOR
            foreach (GameObject g in beatOrNodes)
                DestroyImmediate(g);
#else
            foreach (GameObject g in beatOrNodes)
                Destroy(g);
#endif
            m_soundBankPrefabs = null;

        }

        public void GetSoundBankPrefabsFromName(string soundBankName)
        {
            ClearAllNotesFromScene();

            m_currentSoundLabel = soundBankName;
            string path = m_soundbankFolderName + "/" + m_currentSoundLabel + "/";
            //Debug.Log("loading " + path);

            m_soundBankPrefabs = Resources.LoadAll(path, typeof(GameObject)).Cast<GameObject>().ToArray();

            //Debug.Log("loading from " + path + ", found " + m_soundBankPrefabs.Length + " prefabs");
        }


        public void SavePatternSetToDisc(List<BeatPattern> patterns)
        {
            //List<BeatPattern> patterns = GetCurrentPatterns(_PatternSetElement, "pattern-list-container");

            PatternSet toSave = new PatternSet();
            toSave.patterns = new List<BeatPattern>();
            toSave.tempo = m_metronome.bpm;
            toSave.signatureHi = m_beatsPerMeasure;
            toSave.measures = m_measureCount;
            toSave.soundBank = m_currentPatternSet.soundBank;
            toSave.rootResourceFolder = m_resourceFolderLocation;
            toSave.soundBankFolderName = m_soundbankFolderName;

            foreach (BeatPattern bp in patterns)
                toSave.patterns.Add(bp);

            string savedPatterns = JsonUtility.ToJson(toSave);

            if (m_saveAs == "default" || m_saveAs == "defaultArp" || m_saveAs == "ChillyChill" || m_saveAs == "defaultNotes")
                m_saveAs += "_new";

            File.WriteAllText(Application.persistentDataPath + "/" + m_saveAs + ".json", savedPatterns);

            //Refresh the files directory
            m_savedFilenames = GetFileNames(Application.persistentDataPath + "/");
        }


        public void LoadPatternSetFromDisc()
        {

            string path = Application.persistentDataPath + "/" + m_load + ".json";
            //Debug.Log("Trying to load from " + path);

            //Try to Use the default patterns if there are no saved patterns
            if (!File.Exists(path))
            {
                string newPath = Application.streamingAssetsPath + "/DefaultPatternSets/" + m_load + ".json";

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

            //Debug.Log("Reading Json from " + path);

            string settings = File.ReadAllText(path);
            m_currentPatternSet = JsonUtility.FromJson<PatternSet>(settings);
            m_saveAs = m_load;
            m_soundbankFolderName = m_currentPatternSet.soundBankFolderName;
            m_resourceFolderLocation = m_currentPatternSet.rootResourceFolder;
            m_beatsPerMeasure = m_currentPatternSet.signatureHi;
            m_measureCount = m_currentPatternSet.measures;
            m_metronome.UpdateBPM((float)m_currentPatternSet.tempo);
            m_metronome.UpdateHi(m_currentPatternSet.signatureHi);


            //            Debug.Log(m_currentPatternSet.soundBank + " is the current soundbank");

            GetSoundBankPrefabsFromName(m_currentPatternSet.soundBank);
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

    }

}

