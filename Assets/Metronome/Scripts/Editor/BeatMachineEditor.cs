//Bryan Leister Feb. 2020
//This is the Editor script to manage and create Beat Patterns
//It is not attached to any game object, but will show up when
//the BeatMachine.cs script is attached to a gameobject in the
//scene

//using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


[CustomEditor(typeof(BeatMachine))]
public class BeatMachineEditor : Editor
{
    //The target of our interface
    private BeatMachine _beatMachine;

    //Containers
    private VisualElement _RootElement;
    private VisualElement _PatternSetElement;
    private VisualElement _ControlElement;

    //Templates to make the containers from
    private VisualTreeAsset _BeatMachineTemplate;
    private VisualTreeAsset _BeatPatternTemplate;
    private VisualTreeAsset _ControlTemplate;

    //private PatternSet m_currentPatternSet;

    private string m_savedDataPath = "";
    private string[] m_savedFilenames;
    private string[] m_defaultFilenames;

    #region Starting up the interface, getting the metronome and hooking up the callbacks
    public void OnEnable()
    {
        //Bindings on the UI elements will target the attached BeatMachine.cs script
        _beatMachine = (BeatMachine)target;
        _beatMachine.m_metronome = _beatMachine.transform.GetComponent<Metronome>();


        //Load the presets, if they exist and cache the path to savedData
        m_savedDataPath = Application.persistentDataPath + "/";
        m_savedFilenames = GetFileNames(m_savedDataPath);
        m_defaultFilenames = GetFileNames(Application.streamingAssetsPath + "/DefaultPatternSets/");


        //Initialize the UI Elements
        _RootElement = new VisualElement();
        _ControlElement = new VisualElement();
        _PatternSetElement = new VisualElement();

        _RootElement.name = "RootElement";
        _ControlElement.name = "ControlsAndButtons";
        _PatternSetElement.name = "PatternSets";


        //Load the templates
        _BeatMachineTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Metronome/Scripts/Editor/BeatMachineTemplate.uxml");
        _BeatPatternTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Metronome/Scripts/Editor/patternTemplate.uxml");
        _ControlTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Metronome/Scripts/Editor/ControlAreaTemplate.uxml");

        //Load the styles
        StyleSheet stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Metronome/Scripts/Editor/BeatMachineTemplate.uss");
        _RootElement.styleSheets.Add(stylesheet);

    }

    //Overwrite and build the Editor interface
    public override VisualElement CreateInspectorGUI()
    {
        //Clear the visual element
        _RootElement.Clear();
        _ControlElement.Clear();
        _PatternSetElement.Clear();

        //Clone the visual tree into our Visual Elements using the templates
        _BeatMachineTemplate.CloneTree(_RootElement);
        _ControlTemplate.CloneTree(_ControlElement);

        LoadPatternSetFromDisc();
        LoadSoundBankAndSetPatternRows(_beatMachine.m_currentPatternSet.soundBank, _PatternSetElement);
        RegisterUICallbacks();

        _RootElement.Add(_PatternSetElement);
        _RootElement.Add(_ControlElement);

        return _RootElement;
    }

    #endregion

    #region Update the UI when making changes in the Editor to the pattern itself, or the measures and signature

    void OnPatternChangeUpdateNotesWithNewPatterns()
    {
        //Poll the UI for pattern containers
        var patternToggleContainers = _PatternSetElement.Query<VisualElement>().Class("pattern-list-container").ToList();

        //Each Container is a Visual Element containing a beat pattern with a beat ID
        foreach (VisualElement ve in patternToggleContainers)
        {
            var beatToggles = ve.Query<Toggle>().ToList();

            List<bool> beatToggleValues = new List<bool>();

            //Create the beat list for each note
            foreach (Toggle t in beatToggles)
                beatToggleValues.Add(t.value);

            BeatPattern beatPattern = new BeatPattern();

            //Any note with this beat ID will play the corresponding pattern
            beatPattern.m_beatID = System.Convert.ToInt32(ve.name);
            beatPattern.m_beatPattern = beatToggleValues;

            _beatMachine.DispatchPatterns(beatPattern);
        }


    }


    void LoadSoundBankAndSetPatternRows(string soundBankName, VisualElement patternSetContainer)
    {
        patternSetContainer.Clear();
        _beatMachine.ClearAllNotesFromScene();
        _beatMachine.m_currentSoundLabel = soundBankName;

        string path = soundBankName + "/";

        GameObject[] prefabs = Resources.LoadAll("SoundBanks/" + soundBankName + "/", typeof(GameObject)).Cast<GameObject>().ToArray();

        _beatMachine.m_currentPatternSet.soundBank = soundBankName;
        _beatMachine.m_soundBankPrefabs = prefabs;

        //Make a pattern line for each sound
        for (int i = 0; i < prefabs.Length; i++)
        {
            BeatPattern bp = new BeatPattern();

            bool doesCurrentPatternSetHaveBeatPatternAtThisIndexPosition = _beatMachine.m_currentPatternSet.patterns.Count > i;

            VisualElement visualElement = new VisualElement();

            _BeatPatternTemplate.CloneTree(visualElement);

            //Create the label for this beat pattern
            var label = visualElement.Query<Label>().Class("pattern-id").First();
            label.text = i.ToString();

            visualElement.AddToClassList("pattern-list-container");

            //Name the pattern with the integer - !important
            visualElement.name = i.ToString();

            //Create all of the beat toggles
            for (int b = 0; b < _beatMachine.m_currentPatternSet.measures * _beatMachine.m_currentPatternSet.signatureHi; b++)
            {
                Toggle toggle = new Toggle();

                //resuse the current pattern if it is in range of the newly created pattern (number of measures *  signature)
                if (doesCurrentPatternSetHaveBeatPatternAtThisIndexPosition && b < _beatMachine.m_currentPatternSet.patterns[i].m_beatPattern.Count)
                    toggle.value = _beatMachine.m_currentPatternSet.patterns[i].m_beatPattern[b];

                toggle.name = bp.m_beatID.ToString() + "beat" + b.ToString();
                toggle.viewDataKey = toggle.name;
                toggle.RegisterCallback<MouseUpEvent>(HandlePatternChanges);

                if (b > 0 && ((b + 1) % _beatMachine.m_metronome.signatureHi) == 0 && b != bp.m_beatPattern.Count - 1)
                    toggle.AddToClassList("lastbeat");
                else
                    toggle.AddToClassList("beat");

                visualElement.Add(toggle);
            }

            Label l = new Label();
            l.text = prefabs[i].name;
            visualElement.Add(l);

            patternSetContainer.Add(visualElement);
        }


        _beatMachine.MakeNoteGameObjects();
        //OnPatternChangeUpdateNotesWithNewPatterns();

    }

    void ResetValues()
    {
        var toggleContainers = _PatternSetElement.Query<VisualElement>().Class("pattern-list-container").ToList();

        foreach (VisualElement ve in toggleContainers)
        {
            var bools = ve.Query<Toggle>().ToList();

            List<bool> list = new List<bool>();
            foreach (Toggle t in bools)
                t.value = false;

        }
        OnPatternChangeUpdateNotesWithNewPatterns();
    }

    #endregion

    #region Editor callbacks and events for updating everything when changes happen in the UI

    void RegisterUICallbacks()
    {
        //Find the Integer entry fields for number of beat patterns and set them to update the metronome and interface
        var integerFields = _RootElement.Query<IntegerField>().ToList();

        foreach (IntegerField i in integerFields)
            i.RegisterCallback<KeyUpEvent>(HandlePatternCountChanges);

        Button reset = _ControlElement.Query<Button>("Reset");
        if (reset != null)
            reset.clickable.clicked += () => ResetValues();

        Button save = _ControlElement.Query<Button>("Save");
        if (save != null)
            save.clickable.clicked += () => SavePatternSetToDisc();

        UpdateLoadSavedMenu();
        UpdateLoadSoundBankMenu();
    }

    void UpdateLoadSavedMenu()
    {
        //Setup the dropdown menu
        ContextualMenuManipulator m = new ContextualMenuManipulator(BuildSavedPatternMenu);
        m.target = _RootElement;

        Label label = _ControlElement.Query<Label>("Load");
        if (label != null)
            label.AddManipulator(m);
    }

    void UpdateLoadSoundBankMenu()
    {
        //Setup the dropdown menu
        ContextualMenuManipulator loadSoundBanksMenu = new ContextualMenuManipulator(BuildSoundBankMenu);
        loadSoundBanksMenu.target = _RootElement;

        Label soundBankLabel = _ControlElement.Query<Label>("LoadSoundBank");
        if (soundBankLabel != null)
            soundBankLabel.AddManipulator(loadSoundBanksMenu);
    }


    void HandlePatternCountChanges(KeyUpEvent keyUpEvent)
    {

        _beatMachine.m_metronome.signatureHi = _beatMachine.m_beatsPerMeasure;
        _beatMachine.m_currentPatternSet.measures = _beatMachine.m_measureCount;

        LoadSoundBankAndSetPatternRows(_beatMachine.m_currentPatternSet.soundBank, _PatternSetElement);
        OnPatternChangeUpdateNotesWithNewPatterns();
    }

    void HandlePatternChanges(MouseUpEvent mouseUp)
    {
        _beatMachine.m_beatsPerMeasure = _beatMachine.m_metronome.signatureHi;
        //LoadSoundBankAndSetPatternRows(_beatMachine.m_currentSoundBank, _PatternSetElement);
        OnPatternChangeUpdateNotesWithNewPatterns();
    }

    void BuildSavedPatternMenu(ContextualMenuPopulateEvent evt)
    {
        foreach (string s in m_savedFilenames)
            evt.menu.AppendAction(s, OnLoadPresetFromMenuSelection, DropdownMenuAction.AlwaysEnabled);

        foreach (string s in m_defaultFilenames)
            evt.menu.AppendAction(s, OnLoadPresetFromMenuSelection, DropdownMenuAction.AlwaysEnabled);
    }

    void BuildSoundBankMenu(ContextualMenuPopulateEvent evt)
    {
        string path = Application.dataPath + "/Metronome/Resources/SoundBanks/";

        if (!Directory.Exists(path))
        {
            Debug.LogError("Wrong path for SoundBanks! Fix it before this will work");
            return;
        }

        string[] soundBanks = GetFolderNames(path);

        foreach (string s in soundBanks)
            evt.menu.AppendAction(s, OnLoadSoundBankFromMenuSelection, DropdownMenuAction.AlwaysEnabled);

    }

    void OnLoadSoundBankFromMenuSelection(DropdownMenuAction action)
    {
        _beatMachine.m_currentPatternSet.soundBank = action.name;

        LoadSoundBankAndSetPatternRows(_beatMachine.m_currentPatternSet.soundBank, _PatternSetElement);
    }


    void OnLoadPresetFromMenuSelection(DropdownMenuAction action)
    {
        _beatMachine.m_load = action.name;
        _beatMachine.m_saveAs = action.name;

        LoadPatternSetFromDisc();
        //LoadSoundBankAndSetPatternRows(_beatMachine.m_currentPatternSet.soundBank, _PatternSetElement);
    }

    #endregion

    #region Saving and Loading the patterns as Json files and gather the filenames of the saved patternsets

    void SavePatternSetToDisc()
    {
        List<BeatPattern> patterns = GetCurrentPatterns(_PatternSetElement, "pattern-list-container");

        PatternSet toSave = new PatternSet();
        toSave.patterns = new List<BeatPattern>();
        toSave.tempo = _beatMachine.m_metronome.bpm;
        toSave.signatureHi = _beatMachine.m_metronome.signatureHi;
        toSave.measures = _beatMachine.m_measureCount;
        toSave.soundBank = _beatMachine.m_currentPatternSet.soundBank;

        foreach (BeatPattern bp in patterns)
            toSave.patterns.Add(bp);

        //string savedPatterns = EditorJsonUtility.ToJson(toSave);
        string savedPatterns = JsonUtility.ToJson(toSave);

        File.WriteAllText(m_savedDataPath + _beatMachine.m_saveAs + ".json", savedPatterns);

        //Refresh the files directory
        m_savedFilenames = GetFileNames(m_savedDataPath);
    }

    void LoadPatternSetFromDisc()
    {
        string path = m_savedDataPath + _beatMachine.m_load + ".json";

        //Try to Use the default patterns if there are no saved patterns
        if (!File.Exists(path))
        {
            string newPath = Application.streamingAssetsPath + "/DefaultPatternSets/" + _beatMachine.m_load + ".json";


            if (!File.Exists(newPath))
            {
                Debug.LogError("There are no saved patternsets and I can't find any default patterns to use!");
                return;
            }
            else
            {
                path = newPath;
            }
        }

        string settings = File.ReadAllText(path);
        _beatMachine.m_currentPatternSet = JsonUtility.FromJson<PatternSet>(settings);

        //_beatMachine.ClearAllNotesFromScene();
        LoadSoundBankAndSetPatternRows(_beatMachine.m_currentPatternSet.soundBank, _PatternSetElement);
        //_beatMachine.MakeNoteGameObjects();

    }
    #endregion

    #region Utility Methods for getting patterns and loading filenames for Soundbanks

    string[] GetFileNames(string path)
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

    string[] GetFolderNames(string path)
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

    List<BeatPattern> GetCurrentPatterns(VisualElement elementToQuery, string className)
    {
        List<BeatPattern> patterns = new List<BeatPattern>();
        //Poll the UI for pattern containers
        var toggleContainers = elementToQuery.Query<VisualElement>().Class(className).ToList();

        //each toggle container is a list of booleans, gather and send them to the audio files
        foreach (VisualElement ve in toggleContainers)
        {
            var bools = ve.Query<Toggle>().ToList();
            var note = ve.Query<ObjectField>().First();

            List<bool> beatValueList = new List<bool>();
            foreach (Toggle t in bools)
            {
                beatValueList.Add(t.value);
            }

            BeatPattern beatPattern = new BeatPattern();
            beatPattern.m_beatID = System.Convert.ToInt32(ve.name);
            beatPattern.m_beatPattern = beatValueList;

            patterns.Add(beatPattern);
        }

        return patterns;
    }


    #endregion



}


