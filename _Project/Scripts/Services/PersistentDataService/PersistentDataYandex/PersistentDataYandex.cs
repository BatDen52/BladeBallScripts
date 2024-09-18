using System;
using _Project.Data.Persistent;

namespace _Project
{
    public class PersistentDataYandex : IPersistentDataService
    {
        private const string PersistentDataKey = "PersistentData";

        private Action<PersistentData> _onLoaded;
        
        public PersistentData PersistentData { get; set; }
        
        public void Save()
        {
            PersistentData.Save();
            string persistentDataJson = PersistentData.ToJson();
            Yandex.SDK.SaveExtern(persistentDataJson);
        }

        public void Load(Action<PersistentData> onLoaded)
        {
            _onLoaded = onLoaded;
            Yandex.SDK.LoadExtern();
        }

        public void LoadFromJson(string json)
        {
            PersistentData persistentDataFromYandex = null;
            if (json != "{}") persistentDataFromYandex = json.FromJson<PersistentData>();
            if (persistentDataFromYandex != null) PersistentData = persistentDataFromYandex;
            PersistentData.Load();
            _onLoaded?.Invoke(PersistentData);
        }

        public void Reset()
        {
            Yandex.SDK.SaveExtern("{}");
        }
    }
}