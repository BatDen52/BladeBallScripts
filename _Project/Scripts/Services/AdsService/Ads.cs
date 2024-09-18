using System;
using System.Collections.Generic;
using _Project.Data.Static;
using _Project.Timers;
using UnityEngine;
using VContainer;

namespace _Project
{
    public class Ads : IAds
    {
        private readonly IAudio _audio;
        private readonly IInput _input;
        private readonly StaticData _staticData;
        private readonly TimeInvoker _timeInvoker;
        
        private Action _onAdFinished;
        private Action _onAdSuccess;
        
        [Inject]
        public Ads(IAudio audio, IInput input, StaticData staticData, TimeInvoker timeInvoker)
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
            
            Debug.Log("Fake Ads Service ShowInterstitial");
            IsActive = true;
            _input.Disable();
            _onAdFinished = onAdFinished;
            _audio.Mute(true);
            Timing.RunCoroutine(_ShowInterstitial(), Segment.RealtimeUpdate);
        }

        private IEnumerator<float> _ShowInterstitial()
        {
            yield return Timing.WaitForSeconds(1f);
            OnInterstitialFinished();
        }

        public void OnInterstitialFinished()
        {
            Debug.Log("Fake Ads Service OnInterstitialFinished");
            UnmuteIfApplicationInFocus();
            _input.Enable();
            _onAdFinished?.Invoke();
        }

        public void ShowRewarded(Action onSuccess, Action onFailure = null, string placement = "")
        {
            if (IsActive)
            {
                return;
            }
            
            Debug.Log("Fake Ads Service ShowRewarded");
            IsActive = true;     
            _input.Disable();
            _audio.Mute(true);
            _onAdSuccess = onSuccess;
            Timing.RunCoroutine(_ShowRewarded(), Segment.RealtimeUpdate);
        }

        private IEnumerator<float> _ShowRewarded()
        {
            yield return Timing.WaitForSeconds(1f);
            OnRewardedClose();
        }

        public void OnRewardedFinished()
        {
        }

        public void OnRewardedError()
        {
            
        }

        public void OnRewardedClose()
        {
            UnmuteIfApplicationInFocus();
            _input.Enable();
            _onAdSuccess?.Invoke();
        }

        private void UnmuteIfApplicationInFocus()
        {
            IsActive = false;
            if (Application.isFocused) _audio.Mute(false);
        }
    }
}