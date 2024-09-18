using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace _Project.UI
{
    public class OnScreenTap : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [InputControl(layout = "Button")]
        [SerializeField] private string _controlPath;
        [SerializeField] private float _maxHold = 0.2f;

        private float _startTime;
        
        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            SendValueToControl(0f);
            _startTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Time.time - _startTime < _maxHold)
            {
                SendValueToControl(1f);
            }
            
            SendValueToControl(0f);
        }
    }
}