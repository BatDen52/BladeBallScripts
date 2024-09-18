using System;
using UnityEngine;
using UnityEngine.Audio;

namespace _Project
{
    [Serializable]
    public class AudioDependencies
    {
        public Sounds Sounds;
        public MusicAudioSource MusicAudioSource;
        public SoundsAudioSource SoundsAudioSource;
        public AudioMixer AudioMixer;
    }
}