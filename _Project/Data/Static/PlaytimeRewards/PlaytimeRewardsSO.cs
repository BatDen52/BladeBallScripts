using System.Collections.Generic;
using UnityEngine;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "PlaytimeRewards", menuName = "_Project/PlaytimeRewards/PlaytimeRewards", order = 0)]
    public class PlaytimeRewardsSO : ScriptableObject
    {
        public List<PlaytimeRewardSO> Rewards = new List<PlaytimeRewardSO>();
        public List<int> Timings = new List<int>();
    }
}