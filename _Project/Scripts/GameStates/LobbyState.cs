using System;
using System.Collections.Generic;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.StateMachine;
using _Project.UI;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using IState = _Project.StateMachine.IState;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Project
{
    public class LobbyState : IState
    {
        private readonly IUIService _uiService;
        private readonly IInput _input;
        private readonly IGameFactory _gameFactory;
        private readonly ICameraService _cameraService;
        private readonly IPersistentDataService _persistentDataService;
        private readonly StaticData _staticData;
        private readonly SceneLoader _sceneLoader;
        private readonly IAudio _audio;
        private readonly IAnalytics _analytics;
        private readonly IAds _ads;

        private PlayerData _playerData;
        private Lobby _lobby;

        public IGameStateMachine GameStateMachine { get; set; }

        [Inject]
        public LobbyState(IObjectResolver container)
        {
            _uiService = container.Resolve<IUIService>();
            _input = container.Resolve<IInput>();
            _gameFactory = container.Resolve<IGameFactory>();
            _cameraService = container.Resolve<ICameraService>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _staticData = container.Resolve<StaticData>();
            _sceneLoader = container.Resolve<SceneLoader>();
            _audio = container.Resolve<IAudio>();
            _analytics = container.Resolve<IAnalytics>();
            _ads = container.Resolve<IAds>();
        }
        
        public void Enter()
        {
            _playerData = _persistentDataService.PersistentData.PlayerData;
            LoadRandomScene(then: PrepareLobby);
        }

        public void Exit()
        {
            _lobby.StartRoundTrigger.PlayerEntered -= OnPlayerEnteredStartRoundTrigger;
            _lobby.StartDuel_1_1Trigger.PlayerEntered -= OnPlayerEnteredStartDuel_1_1_Trigger;
        }

        private void LoadRandomScene(Action then)
        {
            List<LevelData> levels = _staticData.Settings.Levels;
            LevelData levelData = levels[Random.Range(0, levels.Count)];
            
#if UNITY_EDITOR
            if (_staticData.Settings.TestLevel.SceneRef.RuntimeKeyIsValid())
            {
                levelData = _staticData.Settings.TestLevel;
            }
#endif
            _playerData.CurrentScene = levelData.Name;
            
            if (_playerData.RoundsCount == 0)
            {
                _analytics.LogEvent(AnalyticsEvents.load_first_scene_start, (AnalyticsParameters.name, levelData.Name));
            }
            else
            {
                _analytics.LogEvent(AnalyticsEvents.load_scene_start, (AnalyticsParameters.name, levelData.Name));
            }
            
            LoadScene(levelData, then);
        }
        
        private void LoadScene(LevelData levelData, Action then)
        {
            _sceneLoader.LoadAddressable(levelData.SceneRef, then: sceneInstance =>
            {
                LifetimeScope sceneScope = Object.FindObjectOfType<LifetimeScope>();
                _lobby = sceneScope.Container.Resolve<Lobby>();
                sceneScope.Container.InjectGameObject(_lobby.gameObject);
                
                if (_playerData.RoundsCount == 0)
                {
                    _analytics.LogEvent(AnalyticsEvents.load_first_scene_complete, (AnalyticsParameters.name, levelData.Name));
                }
                else
                {
                    _analytics.LogEvent(AnalyticsEvents.load_scene_complete, (AnalyticsParameters.name, levelData.Name));
                }
                
                then.Invoke();
            });
        }

        private void PrepareLobby()
        {
            _audio.PlayMusic();
            // _uiService.CreateHUD();
            _uiService.HUD.DisableActionsButtons();
            _uiService.HUD.ShowLobbyButtons();
            _uiService.HUD.HideDuelUI();
            _uiService.HUD.HideCountdown();
            _audio.SetAudioListenerActive(false);
            SpawnPlayer();
            _cameraService.EnableCharacterCamera();
            _uiService.HideLoadingScreen();
            SetInput();

            _lobby.StartRoundTrigger.PlayerEntered += OnPlayerEnteredStartRoundTrigger;
            //If player played tutorial + started 2 rounds then
            //activate Duel portal
            if (_playerData.StartRoundCount > 0)
            {
                _lobby.StartDuel_1_1_ActivePortal.SetActive(true);
                _lobby.StartDuel_1_1_InactivePortal.SetActive(false);
                _lobby.StartDuel_1_1Trigger.PlayerEntered += OnPlayerEnteredStartDuel_1_1_Trigger;
            }
            else
            {
                _lobby.StartDuel_1_1_ActivePortal.SetActive(false);
                _lobby.StartDuel_1_1_InactivePortal.SetActive(true);
            }

            if (_playerData.CurrentSessionPlaytime.Value > 30)
            {
                ShowInterstitial();
            }
            
            RewardPlayerForLastRound();
        }

        
        
        private void SetInput()
        {
            if (_ads.IsActive == false)
            {
                _input.Enable();
            }
            _input.DisableActionsInputs();
            _input.UpdateRotationLock();
        }

        private void RewardPlayerForLastRound()
        {
            if (_playerData.LastRoundReward > 0)
            {
                _persistentDataService.PersistentData.PlayerData.Coins.Value += _playerData.LastRoundReward * (_playerData.HasCoinsX2 ? 2 : 1);
                _playerData.CurrencyGainedCount++;
                _analytics.LogEvent(AnalyticsEvents.currency_gained,
                    (AnalyticsParameters.currency, "coins"),
                    (AnalyticsParameters.source, _playerData.LastRoundMode),
                    (AnalyticsParameters.amount, _playerData.LastRoundReward),
                    (AnalyticsParameters.count, _playerData.CurrencyGainedCount)
                );
                _playerData.LastRoundReward = 0;
                _persistentDataService.Save();
                _audio.PlaySound(_audio.Sounds.Coins);
                
            }
        }

        private void OnPlayerEnteredStartRoundTrigger()
        {
            _analytics.LogEvent(AnalyticsEvents.portal_enter);
            _audio.PlaySound(_audio.Sounds.Teleport);
            _uiService.HUD.UnsubscribeFromCharacter();
            _gameFactory.DestroyPlayer();
            _input.Disable();
            _uiService.ShowLoadingScreen();
            
            if (_playerData.IsTutorial)
            {
                GameStateMachine.Enter<TutorialState>();
            }
            else
            {
                GameStateMachine.Enter<GameplayState, GameplayStateConfig>
                    (GameplayStateConfig.CreateClassicConfig());
            }
            
        }

        private void OnPlayerEnteredStartDuel_1_1_Trigger()
        {
            _analytics.LogEvent(AnalyticsEvents.portal_enter);
            _audio.PlaySound(_audio.Sounds.Teleport);
            _uiService.HUD.UnsubscribeFromCharacter();
            _gameFactory.DestroyPlayer();
            _input.Disable();
            _uiService.ShowLoadingScreen();

            if (_playerData.IsTutorial)
            {
                GameStateMachine.Enter<TutorialState>();
            }
            else
            {
                GameStateMachine.Enter<GameplayState, GameplayStateConfig>
                    (GameplayStateConfig.CreateDuel_1_1Config());
            }

        }

        private void SpawnPlayer()
        {
            Character character = _gameFactory.CreatePlayer(_lobby.SpawnPoint.position, _lobby.SpawnPoint.rotation);
            character.IsLocalPlayer = true;
            _cameraService.SetCharacterCameraTarget(character.CameraTarget);
            _uiService.HUD.SubscribeToCharacter(character);
        }
        
        private void ShowInterstitial()
        {
            if ((DateTime.Now - _playerData.LastInterstitialDateTime).TotalSeconds >= _staticData.Settings.InterstitialInterval && (DateTime.Now - _playerData.LastRewardedDateTime).TotalSeconds >= _staticData.Settings.InterstitialDelayAfterRewarded)
            {
                _ads.ShowInterstitial(placement: _playerData.LastRoundMode);
            }
        }
    }
}