#if UNITY_WEBGL || UNITY_EDITOR
using System;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.Timers;
using UnityEngine;
using VContainer;

namespace _Project
{
    public class AdsYandex : IAds
    {
        private readonly IAudio _audio;
        private readonly IInput _input;
        private readonly StaticData _staticData;
        private readonly TimeInvoker _timeInvoker;
        
        private bool _isRewardedFinished;
        
        private Action _onAdFinished;
        private Action _onAdSuccess;
        private Action _onAdFailure;

        [Inject]
        public AdsYandex(IAudio audio, IInput input, StaticData staticData, TimeInvoker timeInvoker)
        {
            _audio = audio;
            _input = input;
            _staticData = staticData;
            _timeInvoker = timeInvoker;
        }

        public bool IsActive { get; private set; }
        public SyncedTimer RewardedTimer { get; private set; }

        public void Initialize()
        {
            RewardedTimer = new SyncedTimer(TimerType.OneSecTickUnscaled, _staticData.Settings.RewardedCooldown, _timeInvoker);
            RewardedTimer.Start();
        }

        public void ShowInterstitial(Action onAdFinished, string placement = "")
        {
            if (IsActive)
            {
                return;
            }
            
            IsActive = true;
            Time.timeScale = 0;
            _input.Disable();
            _onAdFinished = onAdFinished;
            _audio.Mute(true);
            Yandex.SDK.ShowAdv();
        }

        public void OnInterstitialFinished()
        {
            UnmuteIfApplicationInFocus();
            Time.timeScale = 1;
            _input.Enable();
            _onAdFinished?.Invoke();
        }

        public void ShowRewarded(Action onSuccess, Action onFailure = null, string placement = "")
        {
            if (IsActive)
            {
                return;
            }
            
            IsActive = true;
            Time.timeScale = 0;
            _input.Disable();
            _audio.Mute(true);
            _isRewardedFinished = false;
            _onAdSuccess = onSuccess;
            _onAdFailure = onFailure;
            Yandex.SDK.ShowRewarded();
        }

        public void OnRewardedFinished()
        {
            _isRewardedFinished = true;
        }

        public void OnRewardedError()
        {
            UnmuteIfApplicationInFocus();
            Time.timeScale = 1;
            _input.Enable();
            _onAdFailure?.Invoke();
        }

        public void OnRewardedClose()
        {
            UnmuteIfApplicationInFocus();
            Time.timeScale = 1;
            _input.Enable();
            if (_isRewardedFinished) _onAdSuccess?.Invoke();
        }

        private void UnmuteIfApplicationInFocus()
        {
            IsActive = false;
            if (Application.isFocused) _audio.Mute(false);
        }
    }
}
#endif