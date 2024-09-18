using UnityEngine;
using VContainer;

namespace _Project.UI
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private float _visibilityDistance;
        [SerializeField] private MeshRenderer _meshRenderer;
        
        private Camera _camera;
        private bool _isCameraInjected;

        [Inject]
        private void Construct(ICameraService cameraService)
        {
            _camera = cameraService.GameCameras.MainCamera;
            _isCameraInjected = true;
        }

        private void LateUpdate()
        {
            if (_isCameraInjected == false)
            {
                return;
            }
            
            float distance = Vector3.Distance(_camera.transform.position, transform.position);
            _meshRenderer.enabled = distance < _visibilityDistance;
            transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward,
                _camera.transform.rotation * Vector3.up);
        }
    }
}