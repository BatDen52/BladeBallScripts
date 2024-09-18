using System;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.StateMachine;
using _Project.UI;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using VContainer;
using VContainer.Unity;
using IState = _Project.StateMachine.IState;
using Object = UnityEngine.Object;

namespace _Project
{
    public class AFKState : IState
    {
        private readonly IObjectResolver _container;
        private readonly IUIService _uiService;
        private readonly IInput _input;
        private readonly IGameFactory _gameFactory;
        private readonly ICameraService _cameraService;
        private readonly IPersistentDataService _persistentDataService;
        private readonly StaticData _staticData;
        private readonly SceneLoader _sceneLoader;
        private readonly IAudio _audio;

        private PlayerData _playerData;
        private SceneInstance _currentSceneInstance;
        private AFKZone _afkZone;

        public IGameStateMachine GameStateMachine { get; set; }

        [Inject]
        public AFKState(IObjectResolver container)
        {
            _container = container;
            _uiService = container.Resolve<IUIService>();
            _input = container.Resolve<IInput>();
            _gameFactory = container.Resolve<IGameFactory>();
            _cameraService = container.Resolve<ICameraService>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _staticData = container.Resolve<StaticData>();
            _sceneLoader = container.Resolve<SceneLoader>();
            _audio = container.Resolve<IAudio>();
        }
        
        public void Enter()
        {
            _playerData = _persistentDataService.PersistentData.PlayerData;
            LoadLevel(then: PrepareAFKZone);
        }

        public void Exit()
        {
            _afkZone.BackToGameTrigger.PlayerEntered -= OnPlayerEnteredBackToGameTrigger;
        }

        private void LoadLevel(Action then)
        {
            _sceneLoader.LoadAddressable(_staticData.Settings.AFKSceneRef, then: sceneInstance =>
            {
                _currentSceneInstance = sceneInstance;
                LifetimeScope sceneScope = Object.FindObjectOfType<LifetimeScope>();
                _afkZone = sceneScope.Container.Resolve<AFKZone>();
                then.Invoke();
            });
        }

        private void PrepareAFKZone()
        {
            _audio.PlayMusic();
            _uiService.HUD.DisableActionsButtons();
            _uiService.HUD.ShowLobbyButtons();
            _audio.SetAudioListenerActive(false);
            SpawnPlayer();
            _cameraService.EnableCharacterCamera();
            _uiService.HideLoadingScreen();
            _input.Enable();
            _input.DisableActionsInputs();
            _afkZone.BackToGameTrigger.PlayerEntered += OnPlayerEnteredBackToGameTrigger;
        }

        private void OnPlayerEnteredBackToGameTrigger()
        {
            _audio.PlaySound(_audio.Sounds.Teleport);
            _uiService.HUD.UnsubscribeFromCharacter();
            _gameFactory.DestroyPlayer();
            _input.Disable();
            _uiService.ShowLoadingScreen();
            GameStateMachine.Enter<LobbyState>();
        }
        
        private void SpawnPlayer()
        {
            Character character = _gameFactory.CreatePlayer(_afkZone.SpawnPoint.position, _afkZone.SpawnPoint.rotation);
            _cameraService.SetCharacterCameraTarget(character.CameraTarget);
            _uiService.HUD.SubscribeToCharacter(character);
        }
        
    }
}