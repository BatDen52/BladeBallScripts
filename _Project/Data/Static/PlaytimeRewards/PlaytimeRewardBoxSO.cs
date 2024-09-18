using _Project.Data.Persistent;
using _Project.Data.Static.Boxes;
using UnityEngine;
using VContainer;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "PlaytimeRewardBox", menuName = "_Project/PlaytimeRewards/PlaytimeRewardBox", order = 0)]
    public class PlaytimeRewardBoxSO : PlaytimeRewardSO
    {
        public BoxContentType Content;
        public TypeBox Type;
        public int Amount = 1;
        
        private IAudio _audio;

        [Inject]
        private void Construct(IAudio audio)
        {
            _audio = audio;
        }
        
        public override void Claim(PlayerData playerData)
        {
            if (playerData.FreeBoxOpenings.ContainsKey(Content + "_" + Type))
            {
                playerData.FreeBoxOpenings[Content + "_" + Type] += Amount;
            }
            else
            {
                playerData.FreeBoxOpenings[Content + "_" + Type] = Amount;
            }
            _audio.PlaySound(_audio.Sounds.ChestOpenSound);
        }
    }
}