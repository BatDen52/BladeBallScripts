using System.Collections.Generic;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.Timers;
using UniRx;
using VContainer;

namespace _Project
{
    public class PlaytimeRewards
    {
        public IntReactiveProperty SecondsToNextReward = new IntReactiveProperty();
        
        public IReadOnlyReactiveCollection<int> RewardsReady => _rewardsReady;
        public IReadOnlyReactiveCollection<int> RewardsClaimed => _rewardsClaimed;
        public IReadOnlyList<int> RewardsWaiting => _rewardsWaiting;
        
        private readonly IObjectResolver _container;
        private readonly TimeInvoker _timeInvoker;
        private readonly StaticData _staticData;
        private readonly IPersistentDataService _persistentDataService;
        private readonly Vibrations _vibrations;
        private readonly List<int> _rewardsWaiting = new List<int>();
        private readonly ReactiveCollection<int> _rewardsReady = new ReactiveCollection<int>();
        private readonly ReactiveCollection<int> _rewardsClaimed = new ReactiveCollection<int>();

        private PlayerData _playerData;
        private Dictionary<int, PlaytimeRewardSO> _playtimeRewards = new Dictionary<int, PlaytimeRewardSO>();


        [Inject]
        public PlaytimeRewards(IObjectResolver container)
        {
            _container = container;
            _timeInvoker = container.Resolve<TimeInvoker>();
            _staticData = container.Resolve<StaticData>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _vibrations = container.Resolve<Vibrations>();
        }

        public void Initialize()
        {
            _playerData = _persistentDataService.PersistentData.PlayerData;
            PlaytimeRewardsSO playtimeRewards = _staticData.Settings.PlaytimeRewards;
            
            for (int i = 0; i < playtimeRewards.Rewards.Count; i++)
            {
                _container.Inject(playtimeRewards.Rewards[i]);
                _playtimeRewards[playtimeRewards.Timings[i]] = playtimeRewards.Rewards[i];
                if (_playerData.PlaytimeRewardsClaimed.Contains(playtimeRewards.Timings[i]))
                {
                    _rewardsClaimed.Add(playtimeRewards.Timings[i]);
                }
                else
                {
                    _rewardsWaiting.Add(playtimeRewards.Timings[i]);
                }
            }
            
            _timeInvoker.OnOneSyncedSecondUnscaledTickedEvent += OnSecondTicked;
        }

        public void Claim(int time)
        {
            if (_rewardsReady.Contains(time) == false)
            {
                return;
            }

            _vibrations.VibratePop();
            _playtimeRewards[time].Claim(_playerData);
            _playerData.PlaytimeRewardsClaimed.Add(time);
            _rewardsClaimed.Add(time);
            _rewardsReady.Remove(time);
            
            _persistentDataService.Save();
        }

        private void OnSecondTicked()
        {
            List<int> timesToRemove = new List<int>();
            
            foreach (int time in _rewardsWaiting)
            {
                if (time <= _playerData.CurrentDatePlaytime.Value)
                {
                    timesToRemove.Add(time);
                    _rewardsReady.Add(time);
                }
            }

            foreach (int time in timesToRemove)
            {
                _rewardsWaiting.Remove(time);
            }

            if (_rewardsWaiting.Count == 0)
            {
                SecondsToNextReward.Value = 0;
            }
            else
            {
                SecondsToNextReward.Value = _rewardsWaiting[0] - _playerData.CurrentDatePlaytime.Value;
            }
        }
    }
}