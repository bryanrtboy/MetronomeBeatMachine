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

namespace Beats
{
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

        #region Starting up the interface, getting the metronome and hooking up the callbacks

        public void OnEnable()
        {
            //Bindings on the UI elements will target the attached BeatMachine.cs script
            _beatMachine = (BeatMachine)target;
            _beatMachine.m_metronome = _beatMachine.transform.GetComponent<Metronome>();

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

            _beatMachine.UpdateMenus();

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
            // LoadSoundBankAndSetPatternRows(_beatMachine.m_currentPatternSet.soundBank, _PatternSetElement);
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


        void LoadSoundBankAndSetPatternRows(string soundBankName)
        {
            _PatternSetElement.Clear();

            //Rename the soundbank so that a new set of notes are created, but use the old pattern
            //_beatMachine.m_currentPatternSet.soundBank = soundBankName;
            // _beatMachine.m_currentSoundLabel = soundBankName;
            _beatMachine.GetSoundBankPrefabsFromName(soundBankName);

            PatternSet newPatternSet = new PatternSet();
            newPatternSet.soundBank = soundBankName;
            newPatternSet.patterns = new List<BeatPattern>();
            newPatternSet.tempo = _beatMachine.m_currentPatternSet.tempo;
            newPatternSet.signatureHi = _beatMachine.m_currentPatternSet.signatureHi;
            newPatternSet.measures = _beatMachine.m_currentPatternSet.measures;

            //Make a pattern line for each sound
            for (int patternLine = 0; patternLine < _beatMachine.m_soundBankPrefabs.Length; patternLine++)
            {
                BeatPattern bp = new BeatPattern();
                bp.m_beatID = patternLine;
                List<bool> booleans = new List<bool>();

                bool doesOldPatternHaveAPatternAtThisLineNumber = _beatMachine.m_currentPatternSet.patterns.Count > patternLine;
                int oldPatternLength = _beatMachine.m_currentPatternSet.patterns[0].m_beatPattern.Count;

                VisualElement patternLineVisualElement = new VisualElement();

                _BeatPatternTemplate.CloneTree(patternLineVisualElement);

                //Create the label for this beat pattern
                var label = patternLineVisualElement.Query<Label>().Class("pattern-id").First();
                label.text = patternLine.ToString();

                patternLineVisualElement.AddToClassList("pattern-list-container");

                //Name the pattern with the integer - !important
                patternLineVisualElement.name = patternLine.ToString();

                //Create all of the beat toggles
                for (int b = 0; b < _beatMachine.m_currentPatternSet.measures * _beatMachine.m_currentPatternSet.signatureHi; b++)
                {
                    Toggle toggle = new Toggle();

                    //resuse the current pattern if it is in range of the newly created pattern (number of measures *  signature)
                    if (doesOldPatternHaveAPatternAtThisLineNumber)
                    {
                        if (b < oldPatternLength)
                            toggle.value = _beatMachine.m_currentPatternSet.patterns[patternLine].m_beatPattern[b];

                    }

                    toggle.name = bp.m_beatID.ToString() + "beat" + b.ToString();
                    toggle.viewDataKey = toggle.name;
                    toggle.RegisterCallback<MouseUpEvent>(HandlePatternChanges);

                    if (b > 0 && ((b + 1) % _beatMachine.m_metronome.signatureHi) == 0 && b != bp.m_beatPattern.Count - 1)
                        toggle.AddToClassList("lastbeat");
                    else
                        toggle.AddToClassList("beat");

                    patternLineVisualElement.Add(toggle);
                    booleans.Add(toggle.value);
                }

                bp.m_beatPattern = booleans;
                newPatternSet.patterns.Add(bp);

                Label l = new Label();
                l.text = _beatMachine.m_soundBankPrefabs[patternLine].name;
                patternLineVisualElement.Add(l);

                _PatternSetElement.Add(patternLineVisualElement);

            }

            //OnPatternChangeUpdateNotesWithNewPatterns();

            _beatMachine.m_currentPatternSet = newPatternSet;
            _beatMachine.MakeNoteGameObjects();
            _beatMachine.UpdateMenus();



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

            SetupLoadSavedMenu();
            SetupSoundBankMenu();
        }

        void SetupLoadSavedMenu()
        {
            //Setup the dropdown menu
            ContextualMenuManipulator m = new ContextualMenuManipulator(BuildSavedPatternMenu);
            m.target = _RootElement;

            Label label = _ControlElement.Query<Label>("Load");
            if (label != null)
                label.AddManipulator(m);
        }

        void SetupSoundBankMenu()
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

            LoadSoundBankAndSetPatternRows(_beatMachine.m_currentPatternSet.soundBank);
            OnPatternChangeUpdateNotesWithNewPatterns();
        }

        void HandlePatternChanges(MouseUpEvent mouseUp)
        {
            _beatMachine.m_beatsPerMeasure = _beatMachine.m_metronome.signatureHi;
            OnPatternChangeUpdateNotesWithNewPatterns();
        }

        void BuildSavedPatternMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (string s in _beatMachine.m_savedFilenames)
                evt.menu.AppendAction(s, OnLoadPresetFromMenuSelection, DropdownMenuAction.AlwaysEnabled);

            foreach (string s in _beatMachine.m_defaultFilenames)
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

            string[] soundBanks = _beatMachine.GetFolderNames(path);

            foreach (string s in soundBanks)
                evt.menu.AppendAction(s, OnLoadSoundBankFromMenuSelection, DropdownMenuAction.AlwaysEnabled);

        }

        void OnLoadSoundBankFromMenuSelection(DropdownMenuAction action)
        {
            LoadSoundBankAndSetPatternRows(action.name);
        }


        void OnLoadPresetFromMenuSelection(DropdownMenuAction action)
        {
            _beatMachine.m_load = action.name;
            _beatMachine.m_saveAs = action.name;

            LoadPatternSetFromDisc();
        }

        #endregion

        #region Saving and Loading the patterns as Json files and gather the filenames of the saved patternsets

        void SavePatternSetToDisc()
        {
            List<BeatPattern> patterns = GetCurrentPatterns(_PatternSetElement, "pattern-list-container");

            _beatMachine.SavePatternSetToDisc(patterns);

        }

        void LoadPatternSetFromDisc()
        {
            _beatMachine.LoadPatternSetFromDisc();

            LoadSoundBankAndSetPatternRows(_beatMachine.m_currentPatternSet.soundBank);

        }
        #endregion

        #region Utility Methods for getting patterns and loading filenames for Soundbanks

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


}