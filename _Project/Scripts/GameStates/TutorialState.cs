using System.Collections.Generic;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.StateMachine;
using _Project.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Project
{
    public class TutorialState : IState
    {
        private readonly IObjectResolver _container;
        private readonly IUIService _uiService;
        private readonly IInput _input;
        private readonly IGameFactory _gameFactory;
        private readonly ICameraService _cameraService;
        private readonly IPersistentDataService _persistentDataService;
        private readonly StaticData _staticData;
        private readonly IAudio _audio;
        private readonly IAds _ads;
        private readonly IAnalytics _analytics;

        private PlayerData _playerData;
        private SpawnPoints _spawnPoints;
        private bool _isRoundFinished;
        private int _charactersCount;
        
        public IGameStateMachine GameStateMachine { get; set; }

        [Inject]
        public TutorialState(IObjectResolver container)
        {
            _container = container;
            _uiService = container.Resolve<IUIService>();
            _input = container.Resolve<IInput>();
            _gameFactory = container.Resolve<IGameFactory>();
            _cameraService = container.Resolve<ICameraService>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _staticData = container.Resolve<StaticData>();
            _audio = container.Resolve<IAudio>();
            _ads = container.Resolve<IAds>();
            _analytics = container.Resolve<IAnalytics>();
        }

        public void Enter()
        {
            _playerData = _persistentDataService.PersistentData.PlayerData;
            _isRoundFinished = false;
            GetSpawnPoints();
            _spawnPoints.Points.Shuffle();
            Queue<Transform> spawnPointsQueue = new Queue<Transform>(_spawnPoints.TutorialPoints);
            _uiService.HUD.HideLobbyButtons();
            _uiService.HUD.DisableActionsButtons();
            SpawnBots(spawnPointsQueue);
            _audio.SetAudioListenerActive(false);
            SpawnPlayer(spawnPointsQueue);
            _cameraService.EnableCharacterCamera();
            _uiService.HideLoadingScreen();
            SpawnBladeBall(_spawnPoints);
            SetInput();
            _input.Block += OnBlock;
            _analytics.LogEvent(AnalyticsEvents.tutorial_start);
        }

        private void SetInput()
        {
            _input.Enable();
            _input.EnableActionsInputs();
            _input.UpdateRotationLock();
            _input.DisableActionsInputs();
        }
        
        private void OnBlock()
        {
            Time.timeScale = 1;
            _input.DisableActionsInputs();
            _uiService.HUD.HidePressBlockText();
            // _uiService.HUD.DisableActionsButtons();
            _uiService.HUD.EnableBlockButton(false);
            _uiService.HUD.AnimateBlockButton(false);
            if (_playerData.IsFirstBlock)
            {
                _playerData.IsFirstBlock = false;
                _persistentDataService.Save();
                _analytics.LogEvent(AnalyticsEvents.first_block);
            }
        }

        public void Exit()
        {
            _playerData.RoundsCount++;
            _persistentDataService.Save();
        }

        private void GetSpawnPoints()
        {
            LifetimeScope sceneScope = Object.FindObjectOfType<LifetimeScope>();
            _spawnPoints = sceneScope.Container.Resolve<SpawnPoints>();
        }

        private void SpawnBladeBall(SpawnPoints spawnPoints)
        {
            _gameFactory.CreateBladeBall(spawnPoints.BladeBallSpawnPoint.position);
            foreach (Bot bot in _gameFactory.Bots)
            {
                _gameFactory.BladeBall.Targets.Add(bot.GetComponent<Character>());
                bot.BladeBall = _gameFactory.BladeBall;
            }

            _gameFactory.BladeBall.Targets.Add(_gameFactory.Player);
            _gameFactory.BladeBall.AllCharactersKilled += FinishRound;
            _gameFactory.BladeBall.KillWithBlock += OnKillWithBlock;
            _gameFactory.BladeBall.Kill += OnKill;
        }

        private void CleanUp()
        {
            _cameraService.DisableCharacterCamera();
            _uiService.CloseRewardedRoundFinished();
            _uiService.HUD.UnsubscribeFromCharacter();
            _uiService.ShowLoadingScreen();
            _gameFactory.BladeBall.AllCharactersKilled -= FinishRound;
            _gameFactory.BladeBall.KillWithBlock -= OnKillWithBlock;
            _gameFactory.Player.Died -= PlayerHasDied;
            _gameFactory.Player.DieByAccident -= CharacterDieByAccident;
            _gameFactory.Player.BladeBallCameClose -= OnBladeBallCameCloseToPlayer;
            _input.Block -= OnBlock;
            
            foreach( Bot bot in _gameFactory.Bots) 
            {
                bot.Character.DieByAccident -= CharacterDieByAccident;
            }

            _input.Disable();
            _gameFactory.CleanUp();
            EnterNextState();
        }

        private void OnKillWithBlock(ICharacter character)
        {
            if (character.IsLocalPlayer)
            {
                _playerData.KillsCount++;
                _analytics.LogEvent(AnalyticsEvents.kill,
                    (AnalyticsParameters.count, _playerData.KillsCount),
                    (AnalyticsParameters.mode, "tutorial"));
                
                _playerData.LastRoundReward += _staticData.Settings.CoinsForKill;
                _persistentDataService.Save();
                _audio.PlaySound(_audio.Sounds.KillByPlayer);
                _uiService.HUD.ShowKillText();
            }
        }

        private void OnKill()
        {
            _charactersCount -= 1;
            if (_charactersCount == 2)
            {
                _audio.PlaySound(_audio.Sounds.Standoff);
                _uiService.HUD.ShowStandoffText();
            }
        }

        private void CharacterDieByAccident(ICharacter character)
        {
            if (character.IsLocalPlayer)
            {
                _analytics.LogEvent(AnalyticsEvents.death_accident);
                PlayerHasDied();
            }
            else
            {
                OnKill();
                _gameFactory.BladeBall.CharacterDied(character);
            }
        }

        private void FinishRound(ICharacter lastStandingCharacter)
        {
            if (_isRoundFinished)
            {
                return;
            }
            
            _isRoundFinished = true;

            if (_gameFactory.Player.gameObject.activeSelf)
            {
                _uiService.HUD.ShowWinText();
                _audio.PlaySound(_audio.Sounds.Finish);
                _playerData.IsTutorial = false;
                _analytics.LogEvent(AnalyticsEvents.tutorial_complete);
                _playerData.LastRoundReward += _staticData.Settings.CoinsForWin;
                _persistentDataService.Save();
            }
            else
            {
                _playerData.DeathCount++;
                _analytics.LogEvent(AnalyticsEvents.death, 
                    (AnalyticsParameters.count, _playerData.DeathCount),
                    (AnalyticsParameters.mode, "tutorial"));
                _analytics.LogEvent(AnalyticsEvents.tutorial_failed);
                _audio.SetAudioListenerActive(true, _gameFactory.Player.transform.position);
            }
            
            Timing.RunCoroutine(_FinnishRound());
        }

        private void PlayerHasDied()
        {
            FinishRound(null);
        }

        private IEnumerator<float> _FinnishRound()
        {
            yield return Timing.WaitForSeconds(_staticData.Settings.DelayAfterAllKilled);
            if (_playerData.LastRoundReward > 0)
            {
                ProposeRewarded();
            }
            else
            {
                CleanUp();
            }
        }

        private void ProposeRewarded()
        {
            _uiService.ShowRewardedRoundFinished(
                _playerData.LastRoundReward,
                onAccept: OnRewardedAccept,
                onDecline: CleanUp
                );
        }

        private void OnRewardedAccept()
        {
            _ads.ShowRewarded(placement: "tutorial", onSuccess: () =>
            {
                _playerData.LastRoundReward *= 3;
                _persistentDataService.Save();
                CleanUp();
            });
        }

        private void EnterNextState()
        {
            GameStateMachine.Enter<LobbyState>();
        }

        private void SpawnPlayer(Queue<Transform> spawnPointsQueue)
        {
            Transform spawnPoint = spawnPointsQueue.Dequeue();
            Character player = _gameFactory.CreatePlayer(spawnPoint.position, spawnPoint.rotation);
            player.Died += PlayerHasDied;
            player.IsLocalPlayer = true;
            player.DieByAccident += CharacterDieByAccident;
            player.BladeBallCameClose += OnBladeBallCameCloseToPlayer;
            _cameraService.SetCharacterCameraTarget(player.CameraTarget);
            _uiService.HUD.SubscribeToCharacter(player);
        }

        private void OnBladeBallCameCloseToPlayer(float speed)
        {
            _input.EnableActionsInputs();
            _uiService.HUD.ShowPressBlockText();
            // _uiService.HUD.EnableActionsButtons();
            _uiService.HUD.EnableBlockButton(true);
            _uiService.HUD.AnimateBlockButton(true);
            Time.timeScale = 0f;
        }

        private void SpawnBots(Queue<Transform> spawnPointsQueue)
        {
            int botsCount = _staticData.Settings.TutorialBotsCount;
            for (int i = 0; i < botsCount; i++)
            {
                Transform spawnPoint = spawnPointsQueue.Dequeue();
                var bot = _gameFactory.CreateBot(spawnPoint.position, spawnPoint.rotation, _staticData.Settings.BotConfigsTutorial);
                var character = bot.GetComponent<Character>();
                character.DieByAccident += CharacterDieByAccident;
            }
        }
    }
}