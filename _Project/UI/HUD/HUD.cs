using _Project.Data.Persistent;
using _Project.Data.Static;
using DG.Tweening;
using System;
using TMPro;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private float _standoffTextDuration = 3f;
        [SerializeField] private float _winTextDuration = 3f;
        [SerializeField] private float _killTextDuration = 3f;
        [SerializeField] private Button _buttonShop;
        [SerializeField] private Button _buttonDailyBonus;
        [SerializeField] private Image _dailyBonusAlert;
        [SerializeField] private Button _buttonPlaytimeRewards;
        [SerializeField] private GameObject _playtimeRewardsAlert;
        [SerializeField] private Button _buttonHelp;
        [SerializeField] private Button _buttonBlock;
        [SerializeField] private DOTweenAnimation _buttonBlockAnimation;
        [SerializeField] private Button _buttonJump;
        [SerializeField] private Button _buttonInterect;
        [SerializeField] private TextMeshProUGUI _textInterectButton;
        [SerializeField] private Image _interectOverlay;
        [SerializeField] private Button _buttonCursor;
        [SerializeField] private Image _blockCooldownOverlay;
        [SerializeField] private Button _buttonSkill;
        [SerializeField] private Image _skillCooldownOverlay;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _skillActivationsLeftCount;
        [SerializeField] private GameObject      _skillActivationsLeftCountRoot;
        [SerializeField] private TextMeshProUGUI _textCoins;
        [SerializeField] private TextMeshProUGUI _textCrystals;
        [SerializeField] private TextMeshProUGUI _textStandoff;
        [SerializeField] private TextMeshProUGUI _textYouWin;
        [SerializeField] private Image _crossHair;
        [SerializeField] private TextMeshProUGUI _textYouKill;
        [SerializeField] private TextMeshProUGUI _textPressBlock;
        [SerializeField] private BonusPanel _bonusPanel;
        [SerializeField] private TimedRewardIcon _timedReward;
        [SerializeField] private GameObject[] _keyboardKeys;

        [SerializeField] private HUD_Duel _hudDuel = new HUD_Duel();

        [SerializeField] private HUD_TimeCountdown _hudTimeCountdown = new HUD_TimeCountdown();

        private IPersistentDataService _persistentDataService;
        private PlayerData _playerData;
        private IInput _input;
        private StaticData _staticData;
        private IUIService _uiService;
        private IAnalytics _analytics;
        private IAudio _audio;
        private PlaytimeRewards _playtimeRewards;

        private CompositeDisposable _subscribes = new CompositeDisposable();
        private CompositeDisposable _characterSubscribes = new CompositeDisposable();
        private IDisposable _interactOverlaySubscribe;
        private Window _shopWindow;

        private bool _displaySkillActivationsCountLeft = false;

        [Inject]
        private void Construct(IObjectResolver container)
        {
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _input = container.Resolve<IInput>();
            _staticData = container.Resolve<StaticData>();
            _uiService = container.Resolve<IUIService>();
            _analytics = container.Resolve<IAnalytics>();
            _audio = container.Resolve<IAudio>();
            _playtimeRewards = container.Resolve<PlaytimeRewards>();
            
            if (Application.isMobilePlatform == false)
            {
                _input.RotationLock += OnRotationLock;
            }
        }

        private void Start()
        {
            _playerData = _persistentDataService.PersistentData.PlayerData;
            SubscribeTextCoins();
            SubscribePlaytimeRewardsAlert();
            RefreshDailyBonusAlert();
            RefreshBonusPanel();
        }

        private void OnDestroy()
        {
            _subscribes.Clear();

            if (Application.isMobilePlatform == false)
            {
                _input.RotationLock -= OnRotationLock;
            }

            _input.OpenShop -= OnOpenShop;
        }

        private void SubscribeTextCoins()
        {
            _playerData.Coins.Subscribe(value =>
                {
                    _textCoins.SetText(value.ToString());
                })
                .AddTo(_subscribes);
            _playerData.Crystals.Subscribe(value =>
                {
                    _textCrystals.SetText(value.ToString());
                })
                .AddTo(_subscribes);
        }

        private void SubscribePlaytimeRewardsAlert()
        {
            _playtimeRewardsAlert.SetActive(_playtimeRewards.RewardsReady.Count > 0);
            
            _playtimeRewards.RewardsReady.ObserveCountChanged().Subscribe(value =>
            {
                _playtimeRewardsAlert.SetActive(value > 0);
            }).AddTo(_subscribes);
        }

        public void SubscribeToCharacter(Character character)
        {
            character.BlockCooldown.Subscribe(value =>
            {
                _blockCooldownOverlay.fillAmount = math.remap(
                    0f, character.MaxBlockCooldown, 0f, 1f, value
                );
            }).AddTo(_characterSubscribes);

            character.SkillActivator.SkillCooldown.Subscribe(value =>
            {
                _skillCooldownOverlay.fillAmount = math.remap(
                    0f, character.SkillActivator.MaxSkillCooldown, 0f, 1f, value
                );
            }).AddTo(_characterSubscribes);

            character.SkillActivator.SkillId.Subscribe(value =>
            {
                var skill = _staticData.Settings.Skills[value];
                _skillIcon.sprite = skill.Icon;

                _displaySkillActivationsCountLeft = skill.ActivationsCount > 1;
            }).AddTo(_characterSubscribes);

            character.SkillActivator.SkillActivationsCountLeft.Subscribe( value =>
            {
                _skillActivationsLeftCount.text = value.ToString();
                _skillActivationsLeftCountRoot.SetActive(_displaySkillActivationsCountLeft);
            }).AddTo(_characterSubscribes);
        }

        public void UnsubscribeFromCharacter()
        {
            _characterSubscribes.Clear();
        }

        public void ShowStandoffText()
        {
            _textStandoff.gameObject.SetActive(true);
            DOTween.Sequence().AppendInterval(_standoffTextDuration).AppendCallback(() => _textStandoff.gameObject.SetActive(false));
        }

        public void ShowWinText()
        {
            _textYouWin.gameObject.SetActive(true);
            DOTween.Sequence().AppendInterval(_winTextDuration).AppendCallback(() => _textYouWin.gameObject.SetActive(false));
        }

        public void ShowKillText()
        {
            if (_hudDuel.IsActive)
            {
                _hudDuel.ShowKillText(_killTextDuration);
            } else
            {
                _textYouKill.gameObject.SetActive(true);
                DOTween.Sequence().AppendInterval(_killTextDuration).AppendCallback(() => _textYouKill.gameObject.SetActive(false));
            }
        }

        public void ShowPressBlockText()
        {
            _textPressBlock.gameObject.SetActive(true);
        }

        public void HidePressBlockText()
        {
            _textPressBlock.gameObject.SetActive(false);
        }

        public void DisableActionsButtons()
        {
            _buttonBlock.interactable = false;
            _buttonSkill.interactable = false;
        }

        public void EnableActionsButtons()
        {
            _buttonBlock.interactable = true;
            _buttonSkill.interactable = true;
        }

        public void EnableBlockButton(bool value)
        {
            _buttonBlock.interactable = value;
        }

        public void ShowLobbyButtons()
        {
            _buttonShop.gameObject.SetActive(true);
            _buttonHelp.gameObject.SetActive(true);
            _buttonPlaytimeRewards.gameObject.SetActive(true);
            _buttonDailyBonus.gameObject.SetActive(true);
            _bonusPanel.gameObject.SetActive(true);
        }

        public void HideLobbyButtons()
        {
            _buttonShop.gameObject.SetActive(false);
            _buttonHelp.gameObject.SetActive(false);
            _buttonPlaytimeRewards.gameObject.SetActive(false);
            _buttonDailyBonus.gameObject.SetActive(false);
            _bonusPanel.gameObject.SetActive(false);
            _timedReward.gameObject.SetActive(false);
        }

        public void HideJumpButton()
        {
            _buttonJump.gameObject.SetActive(false);
        }

        public void HideCursorButton()
        {
            _buttonCursor.gameObject.SetActive(false);
        }
        
        public void HideDuelUI()
        {
            _hudDuel.Hide(true);
        }

        public void ConfigureDuelHUD(GameplayState gameplayState)
        {
            _hudDuel.Hide(false);
            _hudDuel.ConfigureDuelHUD(gameplayState);
        }

        public void HideKeyboardKeys()
        {
            foreach (GameObject keyboardKey in _keyboardKeys)
            {
                keyboardKey.SetActive(false);
            }
        }

        public void AnimateBlockButton(bool value)
        {
            _buttonBlockAnimation.enabled = value;
        }

        public void RefreshDailyBonusAlert()
        {
            _dailyBonusAlert.gameObject.SetActive((DateTime.Now.Date - _playerData.LastDailyBonusDate).Days > 0);
        }

        public void RefreshBonusPanel()
        {
            _bonusPanel.Init(_playerData);
        }

        public Button ShowInteractButton(string text, bool isInteractable, Action onPress, Action onCancel, ITimePressable box, bool isFree = false)
        {
            if (_buttonInterect == null)
                return null;

            _buttonInterect.gameObject.SetActive(true);
            _buttonInterect.interactable = isInteractable;

            if (isInteractable)
            {
                _input.InteractStart += onPress;
                _input.InteractCancel += onCancel;
            }

            _interactOverlaySubscribe = box.PressTime.Subscribe(value =>
            {
                _interectOverlay.fillAmount = math.remap(
                    0f, box.NeedPressTime, 0f, 1f, value
                );
            });

            if (isFree == false)
            {
                _textInterectButton.text = text;
            }

            return _buttonInterect;
        }

        public void HideInteractButton(Action onPress, Action onCencel, ITimePressable box)
        {
            if (_buttonInterect != null)
                _buttonInterect.gameObject.SetActive(false);

            _input.InteractStart -= onPress;
            _input.InteractCancel -= onCencel;

            if (_interactOverlaySubscribe != null)
            {
                _interactOverlaySubscribe.Dispose();
            }
        }

        public void SubscribeShopOpen()
        {
            _input.OpenShop += OnOpenShop;
        }

        private void OnOpenShop()
        {
            if (_buttonShop.gameObject.activeSelf == false)
            {
                return;
            }
            
            WindowId windowId = _buttonShop.GetComponent<ButtonOpenWindow>().WindowId;
            
            if (_shopWindow == null)
            {
                _shopWindow = _uiService.Open(windowId);
                _shopWindow.Closing += OnShopClosing;
            }
            else
            {
                _uiService.Close(_shopWindow);
            }

        }

        private void OnShopClosing()
        {
            _shopWindow.Closing -= OnShopClosing;
            _shopWindow = null; 
        }

        public void StartCountdown(float seconds)
        {
            _hudTimeCountdown.StartCountdown(seconds, _audio);
        }

        public void HideCountdown()
        {
            _hudTimeCountdown.Hide(true);
        }

        private void OnRotationLock(bool value)
        {
            // _crossHair.gameObject.SetActive(value);
        }
    }
}