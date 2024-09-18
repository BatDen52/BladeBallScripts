using _Project.Data.Persistent;
using UnityEngine;
using VContainer;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "PlaytimeRewardCrystals", menuName = "_Project/PlaytimeRewards/PlaytimeRewardCrystals", order = 0)]
    public class PlaytimeRewardCrystalsSO : PlaytimeRewardSO
    {
        public int Amount;

        private IAudio _audio;
        private IAnalytics _analytics;

        [Inject]
        private void Construct(IAudio audio, IAnalytics analytics)
        {
            _audio = audio;
            _analytics = analytics;
        }
        
        public override void Claim(PlayerData playerData)
        {
            playerData.Crystals.Value += Amount;
            _audio.PlaySound(_audio.Sounds.Coins);
            playerData.CurrencyGainedCount++;
            _analytics.LogEvent(AnalyticsEvents.currency_gained,
                (AnalyticsParameters.currency, "crystals"),
                (AnalyticsParameters.source, "playtime_reward"),
                (AnalyticsParameters.amount, Amount),
                (AnalyticsParameters.count, playerData.CurrencyGainedCount)
            );
        }
    }
}