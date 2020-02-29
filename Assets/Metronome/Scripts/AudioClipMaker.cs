using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    public static class AudioClipMaker
    {
        // copies audiosource properties to temp audiosource for playing at a position
        public static AudioSource PlayClipAtPoint(AudioSource audioSource, AudioClip clip, Vector3 pos, float volume)
        {
            GameObject tempGO = new GameObject(clip.name); // create the temp object
            tempGO.transform.position = pos; // set its position
            AudioSource tempASource = tempGO.AddComponent<AudioSource>(); // add an audio source
            tempASource.clip = clip;
            tempASource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            tempASource.mute = audioSource.mute;
            tempASource.bypassEffects = audioSource.bypassEffects;
            tempASource.bypassListenerEffects = audioSource.bypassListenerEffects;
            tempASource.bypassReverbZones = audioSource.bypassReverbZones;
            tempASource.playOnAwake = audioSource.playOnAwake;
            tempASource.loop = audioSource.loop;
            tempASource.priority = audioSource.priority;
            tempASource.volume = volume;
            tempASource.pitch = audioSource.pitch;
            tempASource.panStereo = audioSource.panStereo;
            tempASource.spatialBlend = audioSource.spatialBlend;
            tempASource.reverbZoneMix = audioSource.reverbZoneMix;
            tempASource.dopplerLevel = audioSource.dopplerLevel;
            tempASource.rolloffMode = audioSource.rolloffMode;
            tempASource.minDistance = audioSource.minDistance;
            tempASource.spread = audioSource.spread;
            tempASource.maxDistance = audioSource.maxDistance;
            // set other aSource properties here, if desired
            tempASource.Play(); // start the sound
            MonoBehaviour.Destroy(tempGO, tempASource.clip.length); // destroy object after clip duration (this will not account for whether it is set to loop)
            return tempASource; // return the AudioSource reference
        }

    }
}