using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void ClickAction();
    public static event ClickAction OnClicked;

    public delegate void SliderAction(float value);
    public static event SliderAction OnChangedSliderValue;


    public delegate void CreateLoops(Vector3 origin);
    public static event CreateLoops OnCreateLoops;


    public void RestObjects()
    {
        if (OnClicked != null)
            OnClicked();
    }

    public void SetVelocity(float f)
    {
        if (OnChangedSliderValue != null)
            OnChangedSliderValue(f);
    }


    //KD Point Cloud subscribes to this and creates the objects
    //This function is triggered by the ARObjectManager when an
    //image is found
    public static void CreateLoopObjects(Vector3 origin)
    {
        if (OnCreateLoops != null)
            OnCreateLoops(origin);
    }

    //Use for a button to manually create objects
    public void Initialize()
    {
        if (OnCreateLoops != null)
            OnCreateLoops(Vector3.zero);
    }
}
