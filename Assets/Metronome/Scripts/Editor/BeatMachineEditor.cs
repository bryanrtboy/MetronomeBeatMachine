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

    //Other
    private Metronome m_metronome;
    private PatternSet m_currentPatternSet;

    private string m_savedDataPath = "";
    private string[] savedFilenames;

    #region Starting up the interface, getting the metronome and hooking up the callbacks
    public void OnEnable()
    {
        _beatMachine = (BeatMachine)target;
        m_metronome = _beatMachine.transform.GetComponent<Metronome>();

        //m_savedDataPath = Application.dataPath + "/Metronome/Resources/Data/";
        m_savedDataPath = Application.persistentDataPath + "/";
        savedFilenames = Directory.GetFiles(m_savedDataPath);

        _RootElement = new VisualElement();
        _RootElement.name = "RootElement";
        _ControlElement = new VisualElement();

        _BeatMachineTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Metronome/Scripts/Editor/BeatMachineTemplate.uxml");
        _BeatPatternTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Metronome/Scripts/Editor/patternTemplate.uxml");
        _ControlTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Metronome/Scripts/Editor/ControlAreaTemplate.uxml");

        //Load the style
        StyleSheet stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Metronome/Scripts/Editor/BeatMachineStyles.uss");
        _RootElement.styleSheets.Add(stylesheet);

        LoadPatternSetFromDisc();
    }

    //Overwrite and build the Editor interface
    public override VisualElement CreateInspectorGUI()
    {
        //Clear the visual element
        _RootElement.Clear();
        _ControlElement.Clear();

        //Clone the visual tree into our Visual Element so it can be drawn
        _BeatMachineTemplate.CloneTree(_RootElement);


        //Find the Integer entry fields for number of beat patterns and set them to update the metronome and interface
        var integerFields = _RootElement.Query<IntegerField>().ToList();
        foreach (IntegerField i in integerFields)
            i.RegisterCallback<KeyDownEvent>(HandlePatternCountChanges);

        //Create a new container to hold the pattern set
        _PatternSetElement = new VisualElement();
        _PatternSetElement.name = "PatternSetContainer";

        if (m_currentPatternSet != null && m_currentPatternSet.patterns.Count > 0)
            ApplyPatternSet(m_currentPatternSet, _PatternSetElement);

        _RootElement.Add(_PatternSetElement);

        _ControlTemplate.CloneTree(_ControlElement);

        Button reset = _ControlElement.Query<Button>("Reset");
        if (reset != null)
            reset.clickable.clicked += () => ResetValues();

        Button save = _ControlElement.Query<Button>("Save");
        if (save != null)
            save.clickable.clicked += () => SavePatternToDisc();

        Button recreate = _ControlElement.Query<Button>("ReCreate");
        if (recreate != null)
            recreate.clickable.clicked += () => RecreateNotes();

        _RootElement.Add(_ControlElement);

        GetAndSendPatterns();

        ContextualMenuManipulator m = new ContextualMenuManipulator(BuildContextualMenu);
        m.target = _RootElement;

        Label label = _ControlElement.Query<Label>("Load");
        if (label != null)
            label.AddManipulator(m);


        return _RootElement;
    }

    #endregion

    #region Building, Updating and Sending out the patterns as lines of toggles in the UI (Please help me clean this up!)
    void GetAndSendPatterns()
    {
        //Poll the UI for pattern containers
        var toggleContainers = _PatternSetElement.Query<VisualElement>().Class("pattern-list-container").ToList();

        //each toggle container is a list of booleans, gather and send them to the audio files
        foreach (VisualElement ve in toggleContainers)
        {
            var bools = ve.Query<Toggle>().ToList();

            List<bool> list = new List<bool>();
            foreach (Toggle t in bools)
            {
                list.Add(t.value);
            }

            BeatPattern beatPattern = new BeatPattern();
            beatPattern.m_beatID = System.Convert.ToInt32(ve.name);
            beatPattern.m_beatPattern = list;

            _beatMachine.DispatchPatterns(beatPattern);
        }


    }

    void ClearAndUpdateToNewPatternCountChanges(int number, VisualElement patternSetContainer)
    {
        VisualElement visualElement = new VisualElement();
        _BeatPatternTemplate.CloneTree(visualElement);

        var label = visualElement.Query<Label>().Class("pattern-id").First();
        label.text = number.ToString();

        visualElement.AddToClassList("pattern-list-container");
        visualElement.name = number.ToString();

        //Add measure indicator
        for (int measure = 0; measure < _beatMachine.m_measureCount; measure++)
        {
            for (int beat = 0; beat < m_metronome.signatureHi; beat++)
            {
                var toggle = new Toggle();

                //Add styles to indicate each measure
                if (beat == m_metronome.signatureHi - 1 && measure < _beatMachine.m_measureCount - 1)
                    toggle.AddToClassList("lastbeat");
                else
                    toggle.AddToClassList("beat");

                string id = number.ToString() + measure.ToString() + beat.ToString();

                toggle.name = "beat" + id;
                toggle.viewDataKey = id;
                toggle.RegisterCallback<MouseUpEvent>(HandlePatternChanges);
                visualElement.Add(toggle);

            }
        }

        ObjectField note = new ObjectField();
        note.objectType = typeof(GameObject);
        visualElement.Add(note);

        patternSetContainer.Add(visualElement);
    }

    void LoadBeatPatternSet(BeatPattern bp, VisualElement patternSetContainer)
    {
        if (m_currentPatternSet != null)
            _beatMachine.m_currentPatternSet = m_currentPatternSet;

        VisualElement visualElement = new VisualElement();
        _BeatPatternTemplate.CloneTree(visualElement);

        //Create the label for this beat pattern
        var label = visualElement.Query<Label>().Class("pattern-id").First();
        label.text = bp.m_beatID.ToString();

        visualElement.AddToClassList("pattern-list-container");
        visualElement.name = bp.m_beatID.ToString();

        //Create all of the beat toggles
        for (int i = 0; i < bp.m_beatPattern.Count; i++)
        {
            Toggle toggle = new Toggle();

            toggle.value = bp.m_beatPattern[i];
            toggle.name = bp.m_beatID.ToString() + "beat" + i.ToString();
            toggle.viewDataKey = toggle.name;
            toggle.RegisterCallback<MouseUpEvent>(HandlePatternChanges);

            if (((i + 1) % m_metronome.signatureHi) == 0 && i != bp.m_beatPattern.Count - 1)
                toggle.AddToClassList("lastbeat");
            else
                toggle.AddToClassList("beat");

            visualElement.Add(toggle);

        }

        ObjectField note = new ObjectField();
        note.objectType = typeof(GameObject);
        note.value = bp.note;

        visualElement.Add(note);
        patternSetContainer.Add(visualElement);

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
        GetAndSendPatterns();
    }

    void ApplyPatternSet(PatternSet settingsToLoad, VisualElement visualElement)
    {
        visualElement.Clear();
        _beatMachine.m_currentPatternSet = settingsToLoad;
        _beatMachine.m_measureCount = settingsToLoad.measures;
        _beatMachine.m_patternCount = settingsToLoad.patterns.Count;
        m_metronome.bpm = settingsToLoad.tempo;
        m_metronome.signatureHi = settingsToLoad.signatureHi;

        if (settingsToLoad.patterns.Count > 0)
        {
            for (int i = 0; i < settingsToLoad.patterns.Count; i++)
                LoadBeatPatternSet(settingsToLoad.patterns[i], visualElement);
        }

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

            List<bool> list = new List<bool>();
            foreach (Toggle t in bools)
            {
                list.Add(t.value);
            }

            BeatPattern beatPattern = new BeatPattern();
            beatPattern.m_beatID = System.Convert.ToInt32(ve.name);
            beatPattern.m_beatPattern = list;
            beatPattern.note = note.value as GameObject;

            patterns.Add(beatPattern);
        }

        return patterns;
    }

    #endregion

    #region Editor callbacks and events for updating everything when changes happen in the UI

    void HandlePatternCountChanges(KeyDownEvent keyDownEvent)
    {
        m_metronome.signatureHi = _beatMachine.m_beatsPerMeasure;

        _PatternSetElement.Clear();

        for (int i = 0; i < _beatMachine.m_patternCount; i++)
        {
            ClearAndUpdateToNewPatternCountChanges(i, _PatternSetElement);
        }

        _beatMachine.ClearAllNotesFromScene();
    }

    void HandlePatternChanges(MouseUpEvent mouseUp)
    {
        GetAndSendPatterns();
    }

    void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {

        foreach (string s in savedFilenames)
        {
            string name = Path.GetFileName(s);
            name = name.Substring(0, name.Length - 5);
            if (name != ".DS_")
                evt.menu.AppendAction(name, OnMenuAction, DropdownMenuAction.AlwaysEnabled);
        }

    }

    void OnMenuAction(DropdownMenuAction action)
    {
        _beatMachine.m_load = action.name;
        _beatMachine.m_saveAs = action.name;

        LoadPatternSetFromDisc();
        RecreateNotes();
    }

    #endregion

    #region Saving and Loading the patterns as Json files

    void SavePatternToDisc()
    {
        List<BeatPattern> patterns = GetCurrentPatterns(_PatternSetElement, "pattern-list-container");

        PatternSet toSave = new PatternSet();
        toSave.patterns = new List<BeatPattern>();
        toSave.tempo = m_metronome.bpm;
        toSave.signatureHi = m_metronome.signatureHi;
        toSave.measures = _beatMachine.m_measureCount;

        foreach (BeatPattern bp in patterns)
            toSave.patterns.Add(bp);

        string savedPatterns = EditorJsonUtility.ToJson(toSave);

        File.WriteAllText(m_savedDataPath + _beatMachine.m_saveAs + ".json", savedPatterns);

        //Refresh the files directory
        savedFilenames = Directory.GetFiles(m_savedDataPath);
    }

    void LoadPatternSetFromDisc()
    {
        string path = m_savedDataPath + _beatMachine.m_load + ".json";

        if (!File.Exists(path))
            return;

        string settings = File.ReadAllText(path);

        PatternSet myStruct = new PatternSet();
        object boxedStruct = myStruct;
        EditorJsonUtility.FromJsonOverwrite(settings, myStruct);
        m_currentPatternSet = (PatternSet)boxedStruct;

        if (m_currentPatternSet != null && m_currentPatternSet.patterns.Count > 0 && _PatternSetElement != null)
        {
            ApplyPatternSet(m_currentPatternSet, _PatternSetElement);
        }

    }

    #endregion



    void RecreateNotes()
    {
        if (_beatMachine.m_currentPatternSet == null)
            _beatMachine.m_currentPatternSet = m_currentPatternSet;

        _beatMachine.ClearAllNotesFromScene();
        _beatMachine.MakeNoteGameObjects();

    }




}


