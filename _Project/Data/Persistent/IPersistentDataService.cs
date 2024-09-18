using System;
using _Project.Data.Persistent;

namespace _Project
{
    public interface IPersistentDataService
    {
        PersistentData PersistentData { get; set; }
        
        void Save();

        void Load(Action<PersistentData> onLoaded);
        
        void LoadFromJson(string json);
        
        void Reset();
    }
}