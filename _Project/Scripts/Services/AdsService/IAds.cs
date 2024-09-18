using System;
using _Project.Timers;
using UniRx;

namespace _Project
{
    public interface IAds
    {
        void Initialize();
        bool IsActive { get; }
        SyncedTimer RewardedTimer { get; }
        void ShowInterstitial(Action onAdFinished = null, string placement = "");
        void OnInterstitialFinished();
        void ShowRewarded(Action onSuccess, Action onFailure = null, string placement = "");
        void OnRewardedFinished();
        void OnRewardedError();
        void OnRewardedClose();
    }
}