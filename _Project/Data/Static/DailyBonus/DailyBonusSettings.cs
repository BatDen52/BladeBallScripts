using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "DailyBonusSettings", menuName = "_Project/DailyBonusSettings", order = 0)]
    public class DailyBonusSettings : ScriptableObject
    {
        public DailyBonusData[] BonusSchedule;
    }

    [System.Serializable]
    public class DailyBonusData
    {
        public int DayNumber;
        public RewardType Type;
        public int Amount;
        public Sprite Icon;
        public LocalizedString ReceivedLocale;
        public LocalizedString DayLocale;
        public LocalizedString RewardLocale;
    }

    public enum RewardType
    {
        Coin,
        Sword
    }
}