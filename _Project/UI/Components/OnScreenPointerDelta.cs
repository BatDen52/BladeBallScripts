using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace _Project.UI
{
    public class OnScreenPointerDelta : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [InputControl(layout = "Vector2")]
        [SerializeField] private string _controlPath;

        private int _activeTouches;
        
        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _activeTouches++;
            SendValueToControl(Vector2.zero);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_activeTouches > 0) _activeTouches--;
            SendValueToControl(Vector2.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_activeTouches > 1)
            {
                return;
            }
            
            SendValueToControl(eventData.delta);
        }
    }
}