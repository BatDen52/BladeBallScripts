using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project
{
    public class Input : IInput
    {
        private InputActions _inputActions = new InputActions();
        public Vector2 Move => _inputActions.Default.Move.ReadValue<Vector2>();
        public Vector2 Look => _inputActions.Default.Look.ReadValue<Vector2>();
        public Vector3 MoveDirection { get; set; }
        public Vector3 LookDirection { get; set; }
        public bool IsEnabled { get; private set; }
        public bool IsRotationLocked { get; private set; }
        public InputAction Jump => _inputActions.Default.Jump;

        private HashSet<InputAction> _disabledInputActions = new HashSet<InputAction>();
        
        public event Action<Vector2> LookInput;
        public event Action<Vector2> LookWhenRotationLockedInput;
        public event Action LookInputCanceled;  
        public event Action<float> ZoomInput;
        public event Action<float> MobileZoomInput;
        public event Action Block;
        public event Action UseSkill;
        public event Action<bool> RotationLock;
        public event Action NextMusic;
        public event Action OpenShop;
        public event Action InteractStart;
        public event Action InteractCancel;

        public Input()
        {
            _inputActions.Default.Look.performed += OnLook;
            _inputActions.Default.RightMouseHold.canceled += OnLookCanceled;
            _inputActions.Default.Block.performed += OnBlock;
            _inputActions.Default.RotationLock.performed += OnRotationLock;
            _inputActions.Default.UseSkill.performed += OnUseSkill;
            _inputActions.Default.CameraZoom.performed += OnCameraZoom;
            _inputActions.Default.NextMusic.performed += OnNextMusic;
            _inputActions.Default.LookWhenRotationLocked.performed += OnLookWhenRotationLocked;
            _inputActions.Default.Click.performed += OnClick;
            _inputActions.Default.Interact.performed += OnStartInteract;
            _inputActions.Default.Interact.canceled += OnStopInteract;
            _inputActions.Default.OpenShop.performed += OnOpenShop;
        }

        public void Initialize()
        {
            if (Application.isMobilePlatform == false)
            {
                SetRotationLock(true);
            }
        }
        
        private void OnOpenShop(InputAction.CallbackContext obj)
        {
            OpenShop?.Invoke();
        }

        private void OnStartInteract(InputAction.CallbackContext obj)
        {
            InteractStart?.Invoke();
        }

        private void OnStopInteract(InputAction.CallbackContext obj)
        {
            InteractCancel?.Invoke();
        }

        private void OnNextMusic(InputAction.CallbackContext obj)
        {
            NextMusic?.Invoke();
        }

        public void Enable()
        {
            IsEnabled = true;
            _inputActions.Enable();
            foreach (InputAction inputAction in _disabledInputActions)
            {
                inputAction.Disable();
            }
        }

        public void Disable()
        {
            IsEnabled = false;
            _inputActions.Disable();
            _inputActions.Default.OpenShop.Enable();
            _inputActions.Default.Interact.Enable();
        }

        public void DisableActionsInputs()
        {
            _inputActions.Default.Block.Disable();
            _inputActions.Default.UseSkill.Disable();
            _inputActions.Default.Click.Disable();
            _disabledInputActions.Add(_inputActions.Default.Block);
            _disabledInputActions.Add(_inputActions.Default.UseSkill);
            _disabledInputActions.Add(_inputActions.Default.Click);
        }

        public void EnableActionsInputs()
        {
            _inputActions.Default.Block.Enable();
            _inputActions.Default.UseSkill.Enable();
            _inputActions.Default.Click.Enable();
            _disabledInputActions.Remove(_inputActions.Default.Block);
            _disabledInputActions.Remove(_inputActions.Default.UseSkill);
            _disabledInputActions.Remove(_inputActions.Default.Click);
        }

        public void SetMobileZoomInput(float value)
        {
            MobileZoomInput?.Invoke(value);
        }

        public void UpdateRotationLock()
        {
            Cursor.lockState = IsRotationLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = IsRotationLocked == false;
            RotationLock?.Invoke(IsRotationLocked);
        }

        private void OnLookWhenRotationLocked(InputAction.CallbackContext context)
        {
            LookWhenRotationLockedInput?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnRotationLock(InputAction.CallbackContext context)
        {
            IsRotationLocked = !IsRotationLocked;
            // RotationLock?.Invoke(IsRotationLocked);
            UpdateRotationLock();
        }

        public void SetRotationLock(bool value)
        {
            IsRotationLocked = value;
            UpdateRotationLock();
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (IsRotationLocked)
            {
                Block?.Invoke();
            }
        }

        private void OnCameraZoom(InputAction.CallbackContext context)
        {
            ZoomInput?.Invoke(context.ReadValue<float>());
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            LookInputCanceled?.Invoke();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            LookInput?.Invoke(context.ReadValue<Vector2>());
        }
        
        private void OnBlock(InputAction.CallbackContext context)
        {
            Block?.Invoke();
        }
        
        private void OnUseSkill(InputAction.CallbackContext context)
        {
            UseSkill?.Invoke();
        }

        private void InvokeInput(Action input)
        {
            if (!IsEnabled)
            {
                return;
            }
            
            input?.Invoke();
        }
    }
}