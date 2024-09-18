using System;
using _Project.Data.Persistent;
using UnityEngine;
using VContainer;

namespace _Project.UI
{
    public class ShopPageGold : MonoBehaviour
    {
        [SerializeField] private ButtonWithCooldown _rewardedButton;

        private IAds _ads;
        private IPersistentDataService _persistentDataService;
        private PlayerData _playerData;
        private IAnalytics _analytics;

        [Inject]
        private void Construct(IObjectResolver container)
        {
            _ads = container.Resolve<IAds>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _playerData = _persistentDataService.PersistentData.PlayerData;
            _analytics = container.Resolve<IAnalytics>();
        }
        
        private void Start()
        {
            _rewardedButton.Cooldown(_ads.RewardedTimer);
        }
        
        private void OnEnable()
        {
            _analytics.LogEvent(AnalyticsEvents.open_window, 
                (AnalyticsParameters.name, "IAPS"));
        }
    }
}