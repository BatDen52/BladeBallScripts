using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using UniRx;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using _Project.Data.Persistent;

namespace _Project.UI
{
    public class BoxView : Window, IPointerClickHandler
    {
        [FormerlySerializedAs("_shakebleView")][SerializeField] private Transform _shakableView;
        [SerializeField] private Image _chestImage;
        [SerializeField] private Image _itemImage;
        [SerializeField] private Image _failImage;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private ParticleSystem _particle;
        [SerializeField] private Button _backPanel;
        [SerializeField] private Button _openNewBoxButton;
        [SerializeField] private TMP_Text _openNewBoxText;
        [SerializeField] private Image _openNewBoxOverlay;
        [SerializeField] private GameObject[] _keyboardKeys;

        private int _clickCount;
        private float _shakeForce;
        private Sprite _failSprite;
        private PlayerData _playerData;
        private string _failText;
        private RandomBox _box;
        private int _currentClickCount;
        private bool _isShaking;
        private Vector3 _originPosition;
        private IAudio _audio;
        private IInput _input;
        private Vibrations _vibrations;

        private Action _onFail;

        [Inject]
        public void Constructor(IObjectResolver container)
        {
            _audio = container.Resolve<IAudio>();
            _input = container.Resolve<IInput>();
            _vibrations = container.Resolve<Vibrations>();
            //ClearSubscribes();
        }

        public async void Init(RandomBox box, Action OnFail, PlayerData playerData)
        {
            _box = box;
            _failSprite = _box.Settings.FailSprite;
            _playerData = playerData;
            _onFail = OnFail;
            _clickCount = box.Settings.ClickCount;
            _shakeForce = box.Settings.ShakeForce;
            ButtonClose.gameObject.SetActive(false);

            if (Application.isMobilePlatform)
                foreach (GameObject keyboardKey in _keyboardKeys)
                    keyboardKey.SetActive(false);

            _failText = string.Format(await _box.Settings.FailLocale.GetLocalizedStringAsync2(), _box.Settings.FailCashBack);

            if (_playerData.HasFastSpin)
            {
                _currentClickCount = _clickCount - 1;
            }
        }

        private void Update()
        {
            if (_isShaking)
            {
                _shakableView.position = _originPosition + Random.insideUnitSphere * Time.deltaTime * _shakeForce;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_currentClickCount >= _clickCount)
                return;

            _currentClickCount++;
            StartShake();
            _vibrations.VibratePop();

            if (_currentClickCount >= _clickCount)
            {
                _vibrations.VibratePeek();
                Open();
            }
        }

        private new void OnDestroy()
        {
            ClearSubscribes();
            base.OnDestroy();
        }

        private async void Open()
        {
            int id = _box.GetRandomItemId();

            if (_particle)
            {
                _chestImage.gameObject.SetActive(false);
                _particle.Play();
            }

            _audio.PlaySound(_audio.Sounds.ChestOpenSound);

            _resultText.gameObject.SetActive(true);

            if (_box.UnlockItem(id))
            {
                _box.ShowItem(_itemImage, _resultText);
            }
            else
            {
                _box.ShowItem(_itemImage, _resultText);
                // _box.ShowItem(_itemImage, _resultText, "\n" + _failText);
                //_failImage.gameObject.SetActive(true);
                //_failImage.sprite = _failSprite;
                _onFail?.Invoke();
            }

            ButtonClose.gameObject.SetActive(true);
            _backPanel.onClick.RemoveAllListeners();
            _backPanel.onClick.AddListener(Close);

            if (_box.CanOpen() && _box.HasItemForGeneration())
                await ShowOpenNewBoxButton();
        }

        private async Task ShowOpenNewBoxButton()
        {
            _openNewBoxButton.gameObject.SetActive(true);

            if (_box.CanOpenFree() == false)
            {
                _openNewBoxText.text = await _box.GetOpenPriceText();
            }

            _input.InteractStart += _box.OnPress;
            _input.InteractCancel += _box.OnCancel;

            _box.PressTime.Subscribe(value =>
            {
                _openNewBoxOverlay.fillAmount = math.remap(
                    0f, _box.NeedPressTime, 0f, 1f, value
                );
            }).AddTo(Subscribes);

            _box.ResetPressTime();
        }

        private void ClearSubscribes()
        {
            _input.InteractStart -= _box.OnPress;
            _input.InteractCancel -= _box.OnCancel;
        }

        private void StartShake()
        {
            Timing.RunCoroutine(Shake());

            _audio.PlaySound(_audio.Sounds.ChestShakeSound);
        }

        private IEnumerator<float> Shake()
        {
            _originPosition = _shakableView.position;

            if (_isShaking == false)
                _isShaking = true;

            yield return Timing.WaitForSeconds(0.25f);

            _isShaking = false;

            yield return 0f;

            _shakableView.position = _originPosition;
        }
    }
}