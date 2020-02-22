using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(BeatMachine))]
public class BeatMachineEditor : Editor
{

    private BeatMachine _spawner;
    private VisualElement _RootElement;
    private VisualTreeAsset _VisualTree;
    private VisualElement _BeatPattern;

    private List<Editor> objectPreviewEditors;
    private Button refresh;
    private Button sendPatterns;
    private Metronome m_metronome;

    public void OnEnable()
    {

        _spawner = (BeatMachine)target;
        m_metronome = _spawner.transform.GetComponent<Metronome>();

        _RootElement = new VisualElement();

        _VisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Metronome/Scripts/Editor/BeatMachineTemplate.uxml");

        //Load the style
        StyleSheet stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Metronome/Scripts/Editor/BeatMachineStyles.uss");
        _RootElement.styleSheets.Add(stylesheet);

    }

    //runs when the item is selected in the inspector...I think.
    public override VisualElement CreateInspectorGUI()
    {
        //Clear the visual element
        _RootElement.Clear();

        //Clone the visual tree into our Visual Element so it can be drawn
        _VisualTree.CloneTree(_RootElement);

        refresh = new Button() { text = "Update Beat Pattern Count" };
        refresh.tooltip = "This will Erase current patterns!";
        refresh.clickable.clicked += () => MakeInspectorTree();
        refresh.AddToClassList("button");

        _BeatPattern = new VisualElement();
        _BeatPattern.name = "BeatPatterns";

        for (int i = 0; i < _spawner.m_patternCount; i++)
            MakeBeatPattern(i);


        _RootElement.Add(refresh);

        _RootElement.Add(_BeatPattern);

        sendPatterns = new Button() { text = "Send Patterns to Audio" };
        sendPatterns.tooltip = "Send this pattern to all the sounds";
        sendPatterns.clickable.clicked += () => GetAndSendPatterns();
        sendPatterns.AddToClassList("button");

        _RootElement.Add(sendPatterns);

        return _RootElement;
    }

    void GetAndSendPatterns()
    {
        //Poll the UI for pattern containers
        var toggleContainers = _BeatPattern.Query<VisualElement>().Class("toggle-container").ToList();

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

            _spawner.DispatchPatterns(beatPattern);
        }


    }

    void MakeBeatPattern(int num)
    {


        var name = new Label(text: num.ToString());
        _BeatPattern.Add(name);

        VisualElement visualElement = new VisualElement();
        visualElement.AddToClassList("toggle-container");
        visualElement.name = num.ToString();


        for (int j = 0; j < _spawner.m_measureCount; j++)
        {
            for (int i = 0; i < m_metronome.signatureHi; i++)
            {
                var toggle = new Toggle();
                if (i == m_metronome.signatureHi - 1 && j < _spawner.m_measureCount - 1)
                    toggle.AddToClassList("lastbeat");
                else
                    toggle.AddToClassList("beat");

                string id = num.ToString() + j.ToString() + i.ToString();

                toggle.name = "toggle" + id;

                //data keys allow the values to persist?
                toggle.viewDataKey = id;
                visualElement.Add(toggle);

            }


        }
        _BeatPattern.Add(visualElement);



    }

    public void MakeInspectorTree()
    {

        _BeatPattern.Clear();

        for (int i = 0; i < _spawner.m_patternCount; i++)
        {
            MakeBeatPattern(i);
        }
    }

    void SetupInts(IntegerField textField)
    {
        //Set up events here to set the Metronome time signature...
    }



}
