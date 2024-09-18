using System;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.Timers;
using UnityEngine;
using VContainer;

namespace _Project
{
    public class AdsIronSource : IAds
    {
        // private const string AppKey = "ca-app-pub-1933591656142280~5687159149";
        private const string AppKey = "1c4e1612d";
            
        private const string AdNetworkName = "ironsource";

        private readonly IAudio _audio;
        private readonly StaticData _staticData;
        private readonly TimeInvoker _timeInvoker;
        private readonly IAnalytics _analytics;
        private readonly IPersistentDataService _persistentDataService;
        private readonly IInput _input;

        private PlayerData _playerData;
#pragma warning disable 0414
        private bool _isRewardedCompleted;
#pragma warning restore 0414
        private string _interstitialPlacement;
        private string _interstitialResult;
        private string _rewardedPlacement;
        private string _rewardedResult;
        
        private Action _onAdFinished;
        private Action _onAdSuccess;
        private Action _onAdFailure;

        [Inject]
        public AdsIronSource(IObjectResolver container)
        {
            _audio = container.Resolve<IAudio>();
            _staticData = container.Resolve<StaticData>();
            _timeInvoker = container.Resolve<TimeInvoker>();
            _analytics = container.Resolve<IAnalytics>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _input = container.Resolve<IInput>();

            IronSource.Agent.validateIntegration();
            IronSource.Agent.init(AppKey, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.REWARDED_VIDEO);
            IronSource.Agent.init(IronSourceAdUnits.REWARDED_VIDEO);
            IronSource.Agent.init(IronSourceAdUnits.INTERSTITIAL);
            
            IronSourceEvents.onImpressionSuccessEvent += OnImpressionSuccessEvent;
            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;

            IronSourceRewardedVideoEvents.onAdOpenedEvent += OnRewardedShown;
            IronSourceRewardedVideoEvents.onAdClosedEvent += OnRewardedClose;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += OnRewardedError;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += OnRewardedFinished;
            IronSourceRewardedVideoEvents.onAdClickedEvent += OnRewardedClicked;
            
            IronSourceInterstitialEvents.onAdOpenedEvent += OnInterstitialShown;
            IronSourceInterstitialEvents.onAdShowFailedEvent += OnInterstitialShowFailed;
            IronSourceInterstitialEvents.onAdClosedEvent += OnInterstitialClosed;
        }

        public bool IsActive { get; private set; }

        public SyncedTimer RewardedTimer { get; private set; }

        public void Initialize()
        {
            _playerData = _persistentDataService.PersistentData.PlayerData;
            RewardedTimer = new SyncedTimer(TimerType.OneSecTickUnscaled, _staticData.Settings.RewardedCooldown, _timeInvoker);
            RewardedTimer.Start();
            LoadInterstitial();
            LoadRewarded();
        }

        private void LoadInterstitial()
        {
            if (!IronSource.Agent.isInterstitialReady())
                IronSource.Agent.loadInterstitial();
        }

        private void LoadRewarded()
        {
            if (!IronSource.Agent.isRewardedVideoAvailable())
                IronSource.Agent.loadRewardedVideo();
        }

        public void ShowInterstitial(Action onAdFinished, string placement = "")
        {
            if (IsActive)
            {
                return;
            }
            
            IsActive = true;
            _onAdFinished = onAdFinished;
            _audio.Mute(true);
            _interstitialPlacement = placement;
            _interstitialResult = "canceled";

            bool isLoaded = IronSource.Agent.isInterstitialReady();
            
            _analytics.LogEvent(AnalyticsEvents.video_ads_triggered, 
                (AnalyticsParameters.ad_type, "interstitial"),
                (AnalyticsParameters.placement, _interstitialPlacement),
                (AnalyticsParameters.ad_network, AdNetworkName),
                (AnalyticsParameters.result, isLoaded ? "success" : "fail"),
                (AnalyticsParameters.internet, Application.internetReachability == NetworkReachability.NotReachable ? 0: 1)
            );
            
            if (isLoaded == false)
            {
                IsActive = false;
                _audio.Mute(false);
                _input.Enable();
                LoadInterstitial();
                return;
            }
            
            _playerData.LastInterstitialDateTime = DateTime.Now;
            IronSource.Agent.showInterstitial();
            
            _analytics.LogEvent(AnalyticsEvents.video_ads_started, 
                (AnalyticsParameters.ad_type, "interstitial"),
                (AnalyticsParameters.placement, _interstitialPlacement),
                (AnalyticsParameters.ad_network, AdNetworkName),
                (AnalyticsParameters.result, "start"),
                (AnalyticsParameters.internet, Application.internetReachability == NetworkReachability.NotReachable ? 0: 1)
            );
        }

        private void OnInterstitialShowFailed(IronSourceError ironSourceError, IronSourceAdInfo ironSourceAdInfo)
        {
            _interstitialResult = "failed";
            IsActive = false;
            _audio.Mute(false);
            _input.Enable();
        }

        private void OnInterstitialShown(IronSourceAdInfo ironSourceAdInfo)
        {
            _interstitialResult = "watched"; 
        }

        private void OnInterstitialClosed(IronSourceAdInfo ironSourceAdInfo)
        {
            IsActive = false;
            _audio.Mute(false);
            _input.Enable();
            _onAdFinished?.Invoke();
            _onAdFinished = null;
            _analytics.LogEvent(AnalyticsEvents.video_ads_complete, 
                (AnalyticsParameters.ad_type, "interstitial"),
                (AnalyticsParameters.placement, _interstitialPlacement),
                (AnalyticsParameters.ad_network, AdNetworkName),
                (AnalyticsParameters.result, _interstitialResult),
                (AnalyticsParameters.internet, Application.internetReachability == NetworkReachability.NotReachable ? 0: 1)
            );
            LoadInterstitial();
        }

        public void ShowRewarded(Action onSuccess, Action onFailure = null, string placement = "")
        {
            if (IsActive)
            {
                return;
            }
            
            IsActive = true;        
            _audio.Mute(true);
            _isRewardedCompleted = false;
            _onAdSuccess = onSuccess;
            _onAdFailure = onFailure;
            _rewardedPlacement = placement;
            _rewardedResult = "canceled";

            bool isLoaded = IronSource.Agent.isRewardedVideoAvailable();
            
            _analytics.LogEvent(AnalyticsEvents.video_ads_triggered, 
                (AnalyticsParameters.ad_type, "rewarded"),
                    (AnalyticsParameters.placement, _rewardedPlacement),
                    (AnalyticsParameters.ad_network, AdNetworkName),
                    (AnalyticsParameters.result, isLoaded ? "success" : "fail"),
                    (AnalyticsParameters.internet, Application.internetReachability == NetworkReachability.NotReachable ? 0: 1)
                    );
            
            if (isLoaded == false)
            {
                IsActive = false;
                _audio.Mute(false);
                _input.Enable();
                LoadRewarded();
                return;
            }
            
            IronSource.Agent.showRewardedVideo();
            
            _analytics.LogEvent(AnalyticsEvents.video_ads_started, 
                (AnalyticsParameters.ad_type, "rewarded"),
                (AnalyticsParameters.placement, _rewardedPlacement),
                (AnalyticsParameters.ad_network, AdNetworkName),
                (AnalyticsParameters.result, "start"),
                (AnalyticsParameters.internet, Application.internetReachability == NetworkReachability.NotReachable ? 0: 1)
            );
        }

        public void OnRewardedFinished()
        {
        }

        private void OnRewardedClicked(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo ironSourceAdInfo)
        {
            _rewardedResult = "clicked";
        }

        public void OnRewardedFinished(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo ironSourceAdInfo)
        {
            _isRewardedCompleted = true;
            _rewardedResult = "watched";
            _input.Enable();
        }

        private void OnRewardedShown(IronSourceAdInfo ironSourceAdInfo)
        {
            _rewardedResult = "watched";
        }

        public void OnRewardedError(IronSourceError ironSourceError, IronSourceAdInfo ironSourceAdInfo)
        {
            _rewardedResult = "failed";
            IsActive = false;
            _input.Enable();
            _audio.Mute(false);
            _onAdFailure?.Invoke();
            _onAdFailure = null;
            LoadRewarded();
        }

        public void OnRewardedClose(IronSourceAdInfo ironSourceAdInfo)
        {
            IsActive = false;
            _input.Enable();
            _audio.Mute(false);
            _playerData.LastRewardedDateTime = DateTime.Now;
            _onAdSuccess?.Invoke();
            _onAdFailure = null;
            _onAdSuccess = null;
            
            _analytics.LogEvent(AnalyticsEvents.video_ads_complete, 
                (AnalyticsParameters.ad_type, "rewarded"),
                (AnalyticsParameters.placement, _rewardedPlacement),
                (AnalyticsParameters.ad_network, AdNetworkName),
                (AnalyticsParameters.result, _rewardedResult),
                (AnalyticsParameters.internet, Application.internetReachability == NetworkReachability.NotReachable ? 0: 1)
            );
            LoadRewarded();
        }

        private void ImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        {
            _analytics.LogEvent(AnalyticsEvents.ad_impression, 
                (AnalyticsParameters.ad_platform, "ironSource"),
                (AnalyticsParameters.ad_source, impressionData.adNetwork),
                (AnalyticsParameters.ad_unit_name, impressionData.adUnit),
                (AnalyticsParameters.ad_format, impressionData.instanceName),
                (AnalyticsParameters.currency, "USD"),
                (AnalyticsParameters.value, impressionData.revenue)
            );
        }

        public void OnRewardedError()
        {
        }

        public void OnRewardedClose()
        {
        }

        public void OnInterstitialFinished()
        {
        }

        private void OnImpressionSuccessEvent(IronSourceImpressionData data)
        {
        }
    }
}