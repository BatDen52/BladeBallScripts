using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.UI
{
    public class LocalizedText3d : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private LocalizedString _localizedString;

        private void Awake()
        {
            _localizedString.StringChanged += UpdateText;
        }

        private void Start()
        {
            UpdateText(_localizedString.GetLocalizedString());
        }

        private void OnDestroy()
        {
            _localizedString.StringChanged -= UpdateText;
        }

        private void UpdateText(string newText)
        {
            _text.text = newText;
        }
    }
}