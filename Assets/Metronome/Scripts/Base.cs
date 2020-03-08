using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    public class Base : MonoBehaviour
    {
        public Vector3 m_rootBuildOffsetPosition = Vector3.up;

        BeatMachine m_beatMachine;

        private void Awake()
        {
            m_beatMachine = FindObjectOfType<BeatMachine>();
        }

        private void OnEnable()
        {
            this.transform.position = Vector3.zero;


            if (m_beatMachine)
                m_beatMachine.MakeNoteGameObjects(this.transform.position + m_rootBuildOffsetPosition);

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject g in gameObjects)
                g.transform.parent = this.transform;
        }
    }
}
