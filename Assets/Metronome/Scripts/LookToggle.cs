//Bryan Leister March 2020
//
//This script is meant for a specific hierarchy, where a parent sphere collider moves the group around
//At start, the parent collider is large, hiding all the objects inside. On click, the parent indicator
//disappears, but the collider is still there and shrinks so the new Node indicator (which is smaller)
//Can be used to move the object around.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookToggle : MonoBehaviour
{
    [Tooltip("This should be model that indicates a complete, unopened Node")]
    public GameObject m_nodeIndicator;
    [Tooltip("Parent that holds all the notes")]
    public GameObject m_noteContainer;
    [Tooltip("Anything else that you want to turn off at the start")]
    public GameObject[] m_others;
    [Tooltip("A single layer that the notes are using")]
    public LayerMask m_noteLayer;
    [Tooltip("This should be a ollider on the parent of the node indicator")]
    public SphereCollider m_parentSphereCollider;
    [Tooltip("The collider will be shrunk when clicked to this size")]
    public float m_hitRadius = .04f;

    List<GameObject> m_objectsToToggle;

    float m_startRadius = 1f;

    public void Awake()
    {
        m_startRadius = m_parentSphereCollider.radius;
        m_objectsToToggle = new List<GameObject>();

        Transform[] notes = m_noteContainer.GetComponentsInChildren<Transform>();

        foreach (Transform t in notes)
        {
            if (((1 << t.gameObject.layer) & m_noteLayer) != 0)
            {
                m_objectsToToggle.Add(t.gameObject);
            }
        }

        foreach (GameObject g in m_others)
            m_objectsToToggle.Add(g);

    }

    private void OnEnable()
    {
        Invoke("ToggleToStartState", .5f);
    }

    public void ToggleToStartState()
    {

        m_nodeIndicator.SetActive(true);

        foreach (GameObject g in m_objectsToToggle)
            g.SetActive(false);

        m_parentSphereCollider.radius = m_startRadius;
    }

    public void ToggleToClickedState()
    {
        m_nodeIndicator.SetActive(false);

        foreach (GameObject g in m_objectsToToggle)
            g.SetActive(true);

        m_parentSphereCollider.radius = m_hitRadius;

    }
}
