using System;
using UniRx;
using UnityEngine;
using VContainer;

namespace _Project.Data.Persistent
{
    [Serializable]
    public class PersistentData
    {
        public PlayerData PlayerData = new PlayerData();
        public AudioSettings AudioSettings = new AudioSettings();
        public BoolReactiveProperty VibrationsEnabled = new BoolReactiveProperty(true);
        
        public void Save()
        {
        }

        public void Load()
        {
        }
    }
    
}