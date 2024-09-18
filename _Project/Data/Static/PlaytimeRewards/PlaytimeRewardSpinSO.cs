using _Project.Data.Persistent;
using UnityEngine;
using VContainer;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "PlaytimeRewardSpin", menuName = "_Project/PlaytimeRewards/PlaytimeRewardSpin", order = 0)]
    public class PlaytimeRewardSpinSO : PlaytimeRewardSO
    {
        private IAudio _audio;

        [Inject]
        private void Construct(IAudio audio)
        {
            _audio = audio;
        }
        
        public override void Claim(PlayerData playerData)
        {
            playerData.FreeSpinsCount++;
            _audio.PlaySound(_audio.Sounds.ChestOpenSound);
        }
    }
}