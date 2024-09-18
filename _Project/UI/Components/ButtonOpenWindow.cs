using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.UI
{
    public class ButtonOpenWindow : MonoBehaviour
    {
        [SerializeField] private WindowId _windowId;
        [SerializeField] private Button _button;

        private IUIService _uiService;

        public WindowId WindowId => _windowId; 

        [Inject]
        public void Construct(IUIService uiService)
        {
            _uiService = uiService;
        }

        private void Awake()
        {
            _button.onClick.AddListener(Open);
        }

        private void Open()
        {
            _uiService.Open(_windowId);
        }
    }
}