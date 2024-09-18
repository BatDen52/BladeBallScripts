using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Data.Static;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Components;
using VContainer;

namespace _Project.UI
{
    public class PlaytimeRewardsWindow : Window
    {
        [SerializeField] private List<PlaytimeRewardView> _rewardsSlots = new List<PlaytimeRewardView>();
        [SerializeField] private LocalizeStringEvent _timeToNextRewardText;
        [SerializeField] private GameObject _claimText;
        [SerializeField] private GameObject _allClaimedText;

        private PlaytimeRewards _playtimeRewards;
        private PlaytimeRewardsSO _playtimeRewardsData;


        [Inject]
        private void Construct(PlaytimeRewards playtimeRewards)
        {
            _playtimeRewards = playtimeRewards;
        }
        
        protected override void Initialize()
        {
            base.Initialize();

            _playtimeRewardsData = Settings.PlaytimeRewards;

            _timeToNextRewardText.gameObject.SetActive(false);
            _claimText.SetActive(false);
            _allClaimedText.SetActive(false);
            
            UpdateTexts();
            HideSlots();
            FillSlots();
        }

        protected override void Subscribe()
        {
            base.Subscribe();
            
            _playtimeRewards.SecondsToNextReward.Subscribe(value =>
            {
                _timeToNextRewardText.UpdateArgument(TimeSpan.FromSeconds(value).ToString(@"m\:ss"));
            }).AddTo(Subscribes);

            _playtimeRewards.RewardsReady.ObserveCountChanged().Subscribe(value =>
            {
                UpdateTexts();
                UpdateSlots();
            }).AddTo(Subscribes);
        }

        private void UpdateTexts()
        {
            if (_playtimeRewards.RewardsReady.Count == 0 && _playtimeRewards.RewardsWaiting.Count == 0)
            {
                _timeToNextRewardText.gameObject.SetActive(false);
                _claimText.SetActive(false);
                _allClaimedText.SetActive(true);
            }
            else if (_playtimeRewards.RewardsReady.Count != 0)
            {
                _timeToNextRewardText.gameObject.SetActive(false);
                _allClaimedText.SetActive(false);
                _claimText.SetActive(true);
            }
            else
            {
                _claimText.SetActive(false);
                _allClaimedText.SetActive(false);
                _timeToNextRewardText.gameObject.SetActive(true);
            }
        }

        private void UpdateSlots()
        {
            foreach (PlaytimeRewardView slot in _rewardsSlots)
            {
                if (_playtimeRewards.RewardsReady.Contains(slot.Timing))
                {
                    slot.SetReady();
                } else if (_playtimeRewards.RewardsClaimed.Contains(slot.Timing))
                {
                    slot.SetClaimed();
                }
            }
        }

        private void HideSlots()
        {
            foreach (PlaytimeRewardView slot in _rewardsSlots)
            {
                slot.gameObject.SetActive(false);
            }
        }

        private void FillSlots()
        {
            for (int i = 0; i < _rewardsSlots.Count; i++)
            {
                if (i >= _playtimeRewardsData.Rewards.Count)
                {
                    return;
                }

                int timing = _playtimeRewardsData.Timings[i];
                _rewardsSlots[i].Fill(_playtimeRewardsData.Rewards[i], timing, _playtimeRewards);
                
                if (_playtimeRewards.RewardsClaimed.Contains(timing))
                {
                    _rewardsSlots[i].SetClaimed();
                }
                else if (_playtimeRewards.RewardsReady.Contains(timing))
                {
                    _rewardsSlots[i].SetReady();
                }
                
                _rewardsSlots[i].gameObject.SetActive(true);
            }
        }
    }
}