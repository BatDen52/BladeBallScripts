using _Project.Data.Persistent;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Data.Static
{
    public abstract class PlaytimeRewardSO : ScriptableObject
    {
        public Sprite Icon;
        public LocalizedString Name;
        
        public abstract void Claim(PlayerData playerData);
    }
}