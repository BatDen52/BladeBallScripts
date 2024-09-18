using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "TimedRewardSettings", menuName = "_Project/TimedRewardSettings", order = 0)]
    public class TimedRewardSettings : ScriptableObject
    {
        public int TimedSkillDuration = 10 * 60;
        public int TimedSkillFailReward = 50;
        public int FirstShowTime = 5;
        public int TimedIconInterval = 60;
        public int TimedIconDuration = 10;
        public Sprite TimedFailIcon;
        //public LocalizedString RewardLocale;
        public LocalizedString GetCoinsLocale;
        public LocalizedString NoSkillsLocale;
    }
}