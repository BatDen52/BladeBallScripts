using System;
using Cinemachine;
using UnityEngine;
using VContainer;

namespace _Project
{
    public class CharacterCamera : MonoBehaviour
    {
        [field: SerializeField] public CinemachineFreeLook Camera { get; private set; }
        
        [Header("Look")]
        [SerializeField] private float _xMultiplier = 0.3f;
        [SerializeField] private float _yMultiplier = -0.001f;
        [SerializeField] private float _xMultiplierWhenRotationLocked = 0.15f;
        [SerializeField] private float _yMultiplierWhenRotationLocked = - 0.0005f;

        [Header("Zoom")] 
        [SerializeField] private float _startZoom = 10f;
        [SerializeField] private float _currentZoom;
        [SerializeField] private float _newZoom;
        [SerializeField] private float _minZoom = 10f;
        [SerializeField] private float _maxZoom = 50f;
        [SerializeField] private float _zoomSensitivity = 25f;
        [SerializeField] private float _zoomSensitivityMobile = 2f;
        [SerializeField] private float _zoomSpeed = 2.5f;

        [Header("Offset")] 
        [SerializeField] private Vector3 _aimOffset;
        [SerializeField] private Vector3 _rotationLockAimOffset;

        [Header("StartPostion")] 
        private float _startZOffset = -10f;
        private float _startYOffset = 2f;
        
        [Range(0f, 1f)] [SerializeField] private float _startY = 0.6f;
        
        private IInput _input;
        private ICameraService _cameraService;
        private Transform _mainCameraTransform;
        private bool _isRotationLocked;
        
        [Inject]
        private void Construct(IInput input, ICameraService cameraService)
        {
            _input = input;
            _cameraService = cameraService;
            _mainCameraTransform = cameraService.GameCameras.MainCamera.transform;
        }
        
        public void SetTarget(Transform target)
        {
            Camera.LookAt = target;
            Camera.Follow = target;
            Vector3 desiredPosition = target.position + target.forward * _startZOffset + new Vector3(0f, _startYOffset, 0f);
            Quaternion desiredRotation = Quaternion.LookRotation(target.forward);
            Camera.ForceCameraPosition(desiredPosition, desiredRotation);
            Camera.m_YAxis.Value = _startY;
        }

        public void Enable()
        {
            Camera.enabled = true;
        }

        public void Disable()
        {
            Camera.enabled = false;
        }

        private void Start()
        {
            _currentZoom = _newZoom = _startZoom;
            Camera.m_Orbits[1].m_Radius = _startZoom;

            _input.LookInput += OnLookInput;
            _input.LookInputCanceled += OnLookInputCanceled;
            _input.ZoomInput += OnZoom;
            _input.MobileZoomInput += OnMobileZoom;
            _input.RotationLock += OnRotationLock;
        }

        private void Update()
        {
            SetMoveDirection();
            SetLookDirection();
        }

        private void LateUpdate()
        {
            UpdateZoom();
        }

        private void OnRotationLock(bool value)
        {
            if (_isRotationLocked == value)
            {
                return;
            }

            _isRotationLocked = value;
            if (value)
            {
                _input.LookInputCanceled -= OnLookInputCanceled;
                _input.LookInput -= OnLookInput;
                _input.LookWhenRotationLockedInput += OnLookWhenRotationLockedInput;
                // SetAimOffset(_rotationLockAimOffset);
            }
            else
            {
                _input.LookInputCanceled += OnLookInputCanceled;
                _input.LookWhenRotationLockedInput -= OnLookWhenRotationLockedInput;
                _input.LookInput += OnLookInput;
                // SetAimOffset(_AimOffset);
            }
        }

        private void OnZoom(float value)
        {
            if (value == 0)
            {
                return;
            }
            if (value < 0)
            {
                _newZoom = _currentZoom + _zoomSensitivity;
            } 
            if (value > 0)
            {
                _newZoom = _currentZoom - _zoomSensitivity;
            } 
            _newZoom = Mathf.Clamp(_newZoom, _minZoom, _maxZoom);
        }

        private void OnMobileZoom(float value)
        {
            if (value == 0)
            {
                return;
            }
            if (value < 0)
            {
                _newZoom = _currentZoom + _zoomSensitivityMobile;
            } 
            if (value > 0)
            {
                _newZoom = _currentZoom - _zoomSensitivityMobile;
            } 
            _newZoom = Mathf.Clamp(_newZoom, _minZoom, _maxZoom);
        }

        private void UpdateZoom()
        {
            if (Math.Abs(_currentZoom - _newZoom) < 0.01f)
            {
                return;
            }
            
            _currentZoom = Mathf.Lerp(_currentZoom, _newZoom, _zoomSpeed * Time.deltaTime);
            _currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);
            
            Camera.m_Orbits[1].m_Radius = _currentZoom;
            Camera.m_Orbits[0].m_Height = _currentZoom;
            Camera.m_Orbits[2].m_Height = -_currentZoom;
        }

        private void SetAimOffset(Vector3 value)
        {
            Camera.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = value;
            Camera.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = value;
            Camera.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = value;
        }
        
        private void OnLookInputCanceled()
        {
            if (_input.IsRotationLocked == false)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void OnLookInput(Vector2 value)
        {
            // Debug.Log("OnLookInput");
            Cursor.lockState = CursorLockMode.Locked;
            Camera.m_YAxis.Value += value.y * _yMultiplier;
            Camera.m_XAxis.Value += value.x * _xMultiplier;
        }
        
        private void OnLookWhenRotationLockedInput(Vector2 value)
        {
            // Debug.Log("OnLookWhenRotationLockedInput");
            Camera.m_YAxis.Value += value.y * _yMultiplierWhenRotationLocked;
            Camera.m_XAxis.Value += value.x * _xMultiplierWhenRotationLocked;
        }

        private void SetMoveDirection()
        {
            Vector2 moveInput = _input.Move;
            Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);
            Quaternion rotation = Quaternion.AngleAxis(_mainCameraTransform.eulerAngles.y, Vector3.up);
            direction = rotation * direction;
            direction = direction.normalized;
            _input.MoveDirection = direction;
        }
        
        private void SetLookDirection()
        {
            Vector3 direction = _mainCameraTransform.forward;
            direction.y = 0f;
            _input.LookDirection = direction;
        }
    }
}