using System;
using _Project.Data.Persistent;
using UnityEngine;

namespace _Project
{
    public class PersistentDataPlayerPrefs : IPersistentDataService
    {
        private const string PersistentDataKey = "PersistentData";

        public PersistentData PersistentData { get; set; } = new PersistentData();
        
        public void Save()
        {
            PersistentData.Save();
            PlayerPrefs.SetString(PersistentDataKey, PersistentData.ToJson());
            PlayerPrefs.Save();
        }

        public void Load(Action<PersistentData> onLoaded)
        {
            PersistentData = PlayerPrefs.GetString(PersistentDataKey)?
                .FromJson<PersistentData>();
            PersistentData?.Load();
            onLoaded?.Invoke(PersistentData);
        }

        public void LoadFromJson(string json)
        {
            
        }

        public void Reset()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}