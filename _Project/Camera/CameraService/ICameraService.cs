using UnityEngine;

namespace _Project
{
    public interface ICameraService
    {
        void Initialize();
        GameCameras GameCameras { get; }
        void SetCharacterCameraTarget(Transform target);
        void EnableCharacterCamera();
        void DisableCharacterCamera();
    }
}