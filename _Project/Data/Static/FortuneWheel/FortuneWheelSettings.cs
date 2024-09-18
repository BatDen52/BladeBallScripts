using _Project.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Data.Static.FortuneWheel
{
    [CreateAssetMenu(menuName = "FortuneWheelSettings")]
    public class FortuneWheelSettings : ScriptableObject
    {
        public WindowId Window = WindowId.FortuneWheel;
        public float NeedPressTime = 1f;

        public int TurnCost = 10;
        public int FullTurnovers = 5;
        public float RotationTime = 4f;

        public FortuneWheelSector[] Sectors;
        public FortuneWheelSector[] AlternativeSectors;

        [Header("Time Between Two Free Turns")]
        public int TimerMaxHours;
        [Range(0, 59)]
        public int TimerMaxMinutes;
        [Range(0, 59)]
        public int TimerMaxSeconds = 10;

        [Header("Can players turn the wheel for FREE from time to time?")]
        public bool IsFreeTurnEnabled = true;

        public float WheelSize = 3f;
        public float LabelRotation = 0f;
        public float IconOffset = 2f;
        public float LabelOffset = 1f;
        public TMP_FontAsset Font;
        public Slice CirclePrefab;

        public LocalizedString BtnOpenLocale;
    }

    public enum WheelRewardType
    {
        coins_x2,//x2 монет на 15 минут
        sword,//Красный меч(Легендарный)
        crystals,//5 кристаллов
        coins,//500 монет
        skill,//Способность, которую не купить за монеты.
        fast_spin//Мгновенное вращение на 30 минут.
    }

    [Serializable]
    public class FortuneWheelSector : System.Object
    {
        public WheelRewardType RewardType;
        public int RewardValue = 100;
        public List<int> RewardIds = new List<int>();
        public LocalizedString RewardText;
        [Tooltip("Chance that this sector will be randomly selected")]
        [Range(0, 100)]
        public int Probability = 100;
        public Color FillColor;
        public Sprite Icon;
        public bool ShowLabelInWheel = true;
        public LocalizedString Label;
        public Color LabelColor;
        [Range(4, 10)]
        public float FontSize;
    }
}
