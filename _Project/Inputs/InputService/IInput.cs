using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project
{
    public interface IInput
    {
        void Initialize();
        void Enable();
        void Disable();
        Vector2 Look { get; }
        Vector2 Move { get; }
        Vector3 MoveDirection { get; set; }
        InputAction Jump { get; }
        Vector3 LookDirection { get; set; }
        bool IsRotationLocked { get; }
        event Action<Vector2> LookInput;
        event Action LookInputCanceled;
        event Action Block;
        event Action UseSkill;
        event Action<float> ZoomInput;
        event Action<float> MobileZoomInput;
        void SetMobileZoomInput(float value);
        void DisableActionsInputs();
        void EnableActionsInputs();
        event Action NextMusic;
        event Action OpenShop;
        event Action<bool> RotationLock;
        event Action<Vector2> LookWhenRotationLockedInput;
        public event Action InteractStart;
        public event Action InteractCancel;
        void UpdateRotationLock();
    }
}