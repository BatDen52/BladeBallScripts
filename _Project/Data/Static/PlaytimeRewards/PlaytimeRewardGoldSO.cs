using _Project.Data.Persistent;
using UnityEngine;
using VContainer;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "PlaytimeRewardGold", menuName = "_Project/PlaytimeRewards/PlaytimeRewardGold", order = 0)]
    public class PlaytimeRewardGoldSO : PlaytimeRewardSO
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
            playerData.Coins.Value += Amount;
            _audio.PlaySound(_audio.Sounds.Coins);
            playerData.CurrencyGainedCount++;
            _analytics.LogEvent(AnalyticsEvents.currency_gained,
                (AnalyticsParameters.currency, "coins"),
                (AnalyticsParameters.source, "playtime_reward"),
                (AnalyticsParameters.amount, Amount),
                (AnalyticsParameters.count, playerData.CurrencyGainedCount)
            );
        }
    }
}