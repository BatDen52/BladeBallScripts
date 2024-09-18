using _Project.Data.Persistent;
using _Project.Data.Static;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.UI
{
    public class RewardedForCoinsButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        
        private IPersistentDataService _persistentDataService;
        private PlayerData _playerData;
        private IAds _ads;
        private StaticData _staticData;
        private IAnalytics _analytics;

        [Inject]
        public void Construct(IObjectResolver container)
        {
            _ads = container.Resolve<IAds>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _playerData = _persistentDataService.PersistentData.PlayerData;
            _staticData = container.Resolve<StaticData>();
            _analytics = container.Resolve<IAnalytics>();
        }

        private void Awake()
        {
            _button.onClick.AddListener(TryAddCoins);
        }

        public void TryAddCoins()
        {
            _ads.ShowRewarded(AddCoins, placement: "iaps");
        }

        private void AddCoins()
        {
            _playerData.Coins.Value += _staticData.Settings.CoinsForRewarded * (_playerData.HasCoinsX2 ? 2 : 1);
            _persistentDataService.Save();
            _playerData.CurrencyGainedCount++;
            _analytics.LogEvent(AnalyticsEvents.currency_gained,
                (AnalyticsParameters.currency, "coins"),
                (AnalyticsParameters.source, "rewarded_shop"),
                (AnalyticsParameters.amount, _staticData.Settings.CoinsForRewarded),
                (AnalyticsParameters.count, _playerData.CurrencyGainedCount)
            );
            _ads.RewardedTimer.Restart(_staticData.Settings.RewardedCooldown);
        }
    }
}
