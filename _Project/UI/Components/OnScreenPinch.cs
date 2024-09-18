using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Project.UI
{
    public class OnScreenPinch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private IInput _input;
        
        [Inject]
        private void Construct(IInput input)
        {
            _input = input;
        }

        private int _currentMainFinger = -1;
        private int _currentSecondFinger = -1;

        private Vector2 _posA;
        private Vector2 _posB;

        private float _previousDistance = -1f;
        private float _distance;
        private float _pinchDelta;

        public void OnPointerDown(PointerEventData data)
        {
            if (_currentMainFinger == -1)
            {
                _currentMainFinger = data.pointerId;
                _posA = data.position;
                return;
            }

            if (_currentSecondFinger == -1)
            {
                _currentSecondFinger = data.pointerId;
                _posB = data.position;
                GetDelta();
                _previousDistance = _distance;
            }
        }

        public void OnDrag(PointerEventData data)
        {
            if (_currentMainFinger == data.pointerId)
            {
                _posA = data.position;
            }

            if (_currentSecondFinger == -1) return;

            if (_currentMainFinger == data.pointerId) _posA = data.position;
            if (_currentSecondFinger == data.pointerId) _posB = data.position;

            GetDelta();
            _pinchDelta = _distance - _previousDistance;
            _previousDistance = _distance;

            Pinch();
        }

        private void GetDelta()
        {
            _distance = Vector2.Distance(_posA, _posB);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (_currentMainFinger == data.pointerId)
            {
                _currentMainFinger = -1;
            }

            if (_currentSecondFinger == data.pointerId)
            {
                _currentSecondFinger = -1;
            }
        }

        private void Pinch()
        {
            _input.SetMobileZoomInput(_pinchDelta);
        }
    }
}