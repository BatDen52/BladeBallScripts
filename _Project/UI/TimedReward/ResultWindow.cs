using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Project.Data.Persistent;
using System;

namespace _Project.UI
{
    public class ResultWindow : Window
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private ParticleSystem _particle;
        [SerializeField] private Button _backPanel;
        [SerializeField] private Button _equipPanel;

        public void Init(IAudio audio, Sprite icon, string text, Action onClick = null)
        {
            if (_particle)
                _particle.Play();

            audio.PlaySound(audio.Sounds.ChestOpenSound);
            _itemImage.sprite = icon;
            _resultText.text = text;

            _equipPanel.onClick.AddListener(() => onClick?.Invoke());
            _equipPanel.onClick.AddListener(Close);
            _equipPanel.gameObject.SetActive(onClick != null);

            _backPanel.onClick.AddListener(Close);
        }
    }
}