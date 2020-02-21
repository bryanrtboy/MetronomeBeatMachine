using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

//using UnityEngine.UI;

[CustomEditor(typeof(BeatMachine))]
public class BeatMachineEditor : Editor
{

    private BeatMachine _spawner;
    private VisualElement _RootElement;
    private VisualTreeAsset _VisualTree;

    private List<Editor> objectPreviewEditors;
    Button refresh;

    public void OnEnable()
    {
        _spawner = (BeatMachine)target;

        _RootElement = new VisualElement();


        _VisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Metronome/Scripts/Editor/BeatMachineTemplate.uxml");

        //Load the style
        StyleSheet stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Metronome/Scripts/Editor/BeatMachineStyles.uss");
        _RootElement.styleSheets.Add(stylesheet);



    }

    void SetupInts(IntegerField textField)
    {
        //Set up events here to set the Metronome time signature...
        //        Debug.Log("Found " + textField.label + textField.value);
    }

    public void MakeInspectorTree()
    {

        //Clear the visual element
        _RootElement.Clear();

        //Clone the visual tree into our Visual Element so it can be drawn
        _VisualTree.CloneTree(_RootElement);


        for (int i = 0; i < _spawner.m_patternCount; i++)
        {
            MakeBeatPattern(i);
        }

        if (refresh != null)
            _RootElement.Add(refresh);
    }

    public override VisualElement CreateInspectorGUI()
    {


        if (refresh == null)
        {
            refresh = new Button() { text = "Update Beat Pattern Count" };
            refresh.tooltip = "This will Erase current patterns!";
            refresh.clickable.clicked += () => MakeInspectorTree();
            refresh.AddToClassList("button");

            _RootElement.Add(refresh);
        }

        MakeInspectorTree();


        return _RootElement;
    }

    void MakeBeatPattern(int num)
    {
        num++;
        var name = new Label(text: "Beat Pattern " + num.ToString());
        _RootElement.Add(name);

        VisualElement visualElement = new VisualElement();
        visualElement.AddToClassList("toggle-container");

        for (int j = 0; j < _spawner.m_measureCount; j++)
        {
            for (int i = 0; i < _spawner.m_beatsPerMeasure; i++)
            {
                var toggle = new Toggle();
                if (i == _spawner.m_beatsPerMeasure - 1 && j < _spawner.m_measureCount - 1)
                    toggle.AddToClassList("lastbeat");
                else
                    toggle.AddToClassList("beat");

                string id = num.ToString() + j.ToString() + i.ToString();
                toggle.name = "toggle" + id;
                //data keys allow the values to persist
                toggle.viewDataKey = id;

                visualElement.Add(toggle);

            }
        }
        _RootElement.Add(visualElement);

    }

}
