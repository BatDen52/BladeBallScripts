using System;
using System.Collections.Generic;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.IAP;
using _Project.StateMachine;
using _Project.UI;
using Facebook.Unity;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project
{
    public class LoadingState : IState, IPostStartable
    {
        private const string BootSceneName = "_boot";

        private readonly IObjectResolver _container;
        private readonly SceneLoader _sceneLoader;
        private readonly IUIService _uiService;
        private readonly IAudio _audio;
        private readonly IPersistentDataService _persistentDataService;
        private readonly ICameraService _cameraService;
        private readonly StaticData _staticData;
        private readonly IAnalytics _analytics;
        private readonly IIAPService _iapService;
        private readonly IInput _input;
        private readonly IAds _ads;
        private readonly PlaytimeRewards _playtimeRewards;
        private readonly Vibrations _vibrations;

        [Inject]
        public LoadingState(IObjectResolver container)
        {
            _container = container;
            _sceneLoader = container.Resolve<SceneLoader>();
            _uiService = container.Resolve<IUIService>();
            _audio = container.Resolve<IAudio>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _cameraService = container.Resolve<ICameraService>();
            _staticData = container.Resolve<StaticData>();
            _analytics = container.Resolve<IAnalytics>();
            _iapService = container.Resolve<IIAPService>();
            _input = container.Resolve<IInput>();
            _ads = container.Resolve<IAds>();
            _playtimeRewards = container.Resolve<PlaytimeRewards>();
            _vibrations = container.Resolve<Vibrations>();
        }

        public IGameStateMachine GameStateMachine { get; set; }

        public void Enter()
        {
            _staticData.Settings.Initialize();
            _cameraService.Initialize();
            _vibrations.Initialize();
            _uiService.Initialize(_cameraService.GameCameras.UICamera);
            _uiService.ShowLoadingScreen();
        }

        public void Exit()
        {
        }

        public void PostStart()
        {
            _persistentDataService.Load(onLoaded: (persistentData) =>
            {
                _persistentDataService.PersistentData ??= CreateNewPersistentData();
                _analytics.Initialize();
                UpdatePlayerData();
                _container.Resolve<ApplicationFocusHandler>();
                _uiService.CreateHUD();
                _audio.Initialize();
                _iapService.Initialize();
                _input.Initialize();
                _ads.Initialize();
                _playtimeRewards.Initialize();
                
                FB.Init(onInitComplete: () =>
                {
                    FB.ActivateApp();
                });
                
                _sceneLoader.Load(BootSceneName, then: () =>
                {
                    GameStateMachine.Enter<LobbyState>();
                });
            });
        }

        private void UpdatePlayerData()
        {
            PlayerData playerData = _persistentDataService.PersistentData.PlayerData;
            playerData.SessionNumber++;
            playerData.CurrentSessionPlaytime.Value = 0;
            
            if (playerData.LastLoginDate != DateTime.Now.Date)
            {
                playerData.CurrentDatePlaytime.Value = 0;
                playerData.PlaytimeRewardsClaimed = new List<int>();
            }

            playerData.LastLoginDate = DateTime.Now.Date;
                
            playerData.CurrentScene = "none";
            
            if (playerData.FirstSessionDateTime == DateTime.MinValue)
            {
                playerData.FirstSessionDateTime = DateTime.Now;
            }
            
            // Debug.Log(playerData.ToJson());
            
            _analytics.LogEvent(
                AnalyticsEvents.game_start,
                (AnalyticsParameters.count, playerData.SessionNumber)
                );
        }

        private PersistentData CreateNewPersistentData()
        {
            return new PersistentData
            {
                PlayerData = new PlayerData
                {
                    Coins = new IntReactiveProperty(_staticData.Settings.NewSaveCoins),
                    Crystals = new IntReactiveProperty(_staticData.Settings.NewSaveCrystals),
                    CurrentWeaponId = 0,
                    CurrentSkillId = 0,
                    CurrentSkinId = 0,
                }
            };
        }
    }
}