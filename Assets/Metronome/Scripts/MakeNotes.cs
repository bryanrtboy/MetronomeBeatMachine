using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    public class MakeNotes : MonoBehaviour
    {
        //How far in front of the camera spawn
        public float m_distanceInFrontToSpawn = 1f;

        public void MakeANote(Transform t)
        {
            Vector3 pos = Random.insideUnitSphere * .2f;

            Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, m_distanceInFrontToSpawn));

            Transform obj = Instantiate(t, spawnPosition + pos, Quaternion.identity);

            obj.Rotate(new Vector3(-90, 0, 0));
        }

        public void DeleteAllBeatEmitters()
        {
            BeatEmitter[] beats = FindObjectsOfType<BeatEmitter>();

            foreach (BeatEmitter b in beats)
                Destroy(b.gameObject);
        }

        public void ToggleRandomness()
        {
            BeatEmitter[] beats = FindObjectsOfType<BeatEmitter>();

            foreach (BeatEmitter b in beats)
                b.m_useRandom = !b.m_useRandom;
        }

        public void ResetRandomValues()
        {
            BeatEmitter[] beats = FindObjectsOfType<BeatEmitter>();

            foreach (BeatEmitter b in beats)
                b.m_randomChance = Random.Range(.25f, .75f);
        }

        public void RandomizeAllDownBeats()

        {
            BeatEmitter[] beats = FindObjectsOfType<BeatEmitter>();
            foreach (BeatEmitter b in beats)
            {
                float rand = Random.Range(0.0f, 1.0f);
                if (rand > .5f)
                    b.m_playOnDownBeat = true;
                else
                    b.m_playOnDownBeat = false;
            }
        }

        public void UseAllDownBeats()

        {
            BeatEmitter[] beats = FindObjectsOfType<BeatEmitter>();

            foreach (BeatEmitter b in beats)
                b.m_playOnDownBeat = true;


        }
    }
}