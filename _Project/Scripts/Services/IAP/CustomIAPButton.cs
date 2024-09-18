using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.IAP
{
    [RequireComponent(typeof(Button))]
    public class CustomIAPButton : MonoBehaviour
    {
        [SerializeField] private IAPProduct _productId;
        [SerializeField] private TMP_Text _text;

        private IIAPService _iapService;
        private Button _button;

        [Inject]
        public void Constructor(IObjectResolver container)
        {
            _iapService = container.Resolve<IIAPService>();
        }

        private void Start()
        {
#if UNITY_WEBGL
            gameObject.SetActive(false);
            return;
#endif
            _button = GetComponent<Button>();
            _button.onClick.RemoveListener(Purchase);
            _button.onClick.AddListener(Purchase);
            _text.text = _iapService.GetPrice(_productId);
        }

        public void SetProductId(IAPProduct productId)
        {
            _productId = productId;
        }

        public void Purchase()
        {
            _iapService.InitiatePurchase(_productId);
        }
    }
}