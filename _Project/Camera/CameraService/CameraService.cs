using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project
{
    public class CameraService : ICameraService
    {
        private readonly IObjectResolver _container;
        private readonly GameCameras _gameCamerasPrefab;
        public GameCameras GameCameras { get; private set; }

        [Inject]
        public CameraService(IObjectResolver container, GameCameras gameCamerasPrefab)
        {
            _container = container;
            _gameCamerasPrefab = gameCamerasPrefab;
        }

        public void Initialize()
        {
            GameCameras = Object.Instantiate(_gameCamerasPrefab);
            GameCameras.CharacterCamera = Object.Instantiate(GameCameras.CharacterCamera, GameCameras.transform);
            GameCameras.VirtualCameras.Add(GameCameras.CharacterCamera.Camera);
            _container.InjectGameObject(GameCameras.gameObject);
            
            foreach (CinemachineVirtualCameraBase camera in GameCameras.VirtualCameras)
            {
                camera.enabled = false;
            }

            // ResetUICamera();
            Object.DontDestroyOnLoad(GameCameras);
        }

        public void EnableCharacterCamera()
        {
            GameCameras.CharacterCamera.Enable();
        }
        
        public void DisableCharacterCamera()
        {
            GameCameras.CharacterCamera.Disable();
        }
        
        public void SetCharacterCameraTarget(Transform target)
        {
            GameCameras.CharacterCamera.SetTarget(target);
        }
        
        private void ResetUICamera()
        {
            GameCameras.UICamera.enabled = false;
            GameCameras.UICamera.enabled = true;
        }
    }
}