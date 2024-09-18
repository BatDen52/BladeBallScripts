using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using System.Linq;
using System;
using System.Collections;
using _Project.Data.Static;
using _Project.Data.Static.FortuneWheel;

namespace _Project.UI
{
    public class FortuneWheelWindow : Window
    {
        private const float CIRCLE_RADIUS = 35f;
        private const int FULL_CIRCLE_DEGREE = 360;

        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private ParticleSystem _particle;
        [SerializeField] private Image _center;
        [SerializeField] private TMP_Text _priceSpinText;
        [SerializeField] private Button FreeSpinButton;
        
        public GameObject Roller;
        public GameObject Pointer;
        public Button PaidTurnButton;
        public Button RewardedTurnButton;
        public TMP_Text DeltaCoinsText;
        public GameObject NextTurnTimerWrapper;
        public TMP_Text NextRewardedTurnTimerText;

        [SerializeField] private GameObject[] _keyboardKeys;

        private bool _isRewardedTurnAvailable;
        private FortuneWheelSector _finalSector;

        private bool _isStarted;
        private float _finalAngle;
        private float _startAngle;
        private float _currentLerpRotationTime;

        private int _timerRemainingHours;
        private int _timerRemainingMinutes;
        private int _timerRemainingSeconds;

        private DateTime _nextFreeTurnTime;
        private int _fortuneSize;
        private float _sliceSize;
        private GameObject _wheel;
        private Vector3 _offsetDistance;

        private IAudio _audio;
        private IAds _ads;

        //private IInput _input;
        private FortuneWheelSettings _settings;

        [Inject]
        protected override void Construct(IObjectResolver container)
        {
            base.Construct(container);
            
            _audio = container.Resolve<IAudio>();
            _ads = container.Resolve<IAds>();
            //_input = container.Resolve<IInput>();
            ValidateSettings(container.Resolve<StaticData>().Settings.FortuneWheelSettings);
            Init();
        }

        public void Init()
        {
            _priceSpinText.text = _settings.TurnCost.ToString();

            if (Application.isMobilePlatform)
                foreach (GameObject keyboardKey in _keyboardKeys)
                    keyboardKey.SetActive(false);

            if (_settings.IsFreeTurnEnabled)
            {
                SetNextFreeTime();

                if (PlayerData.LastRewardedSpinTime == string.Empty)
                {
                    PlayerData.LastRewardedSpinTime = DateTime.Now.Ticks.ToString();
                    PersistentDataService.Save();
                }
            }
            else
            {
                NextTurnTimerWrapper.gameObject.SetActive(false);
            }

            _wheel = new GameObject("Wheel");
            _wheel.transform.SetParent(Roller.transform);
            _wheel.transform.localPosition = Vector3.zero;
            _wheel.transform.localScale = Vector3.one;

            _fortuneSize = _settings.Sectors.Length;
            _sliceSize = 1.0f / _fortuneSize;

            CreateFortune();

            Pointer.transform.parent = transform;
        }

        private void OnEnable()
        {
            PaidTurnButton.onClick.AddListener(TurnWheelButtonClick);
            RewardedTurnButton.onClick.AddListener(TurnWheelButtonClick);
            FreeSpinButton.onClick.AddListener(TurnWheelButtonClick);
        }

        private void OnDisable()
        {
            PaidTurnButton.onClick.RemoveListener(TurnWheelButtonClick);
            RewardedTurnButton.onClick.RemoveListener(TurnWheelButtonClick);
            FreeSpinButton.onClick.RemoveListener(TurnWheelButtonClick);
        }

        private new void OnDestroy()
        {
            ClearSubscribes();
            base.OnDestroy();
        }

        private void Update()
        {
            ShowTurnButtons();

            if (_settings.IsFreeTurnEnabled)
                UpdateRewardedTurnTimer();

            if (_isStarted == false)
                return;

            _currentLerpRotationTime += Time.deltaTime;

            if (PlayerData.HasFastSpin || _currentLerpRotationTime > _settings.RotationTime || _wheel.transform.eulerAngles.z == _finalAngle)
            {
                _wheel.transform.eulerAngles = new Vector3(0, 0, _finalAngle);
                _currentLerpRotationTime = _settings.RotationTime;
                _isStarted = false;
                _startAngle = _finalAngle % FULL_CIRCLE_DEGREE;

                GetReward(_finalSector.RewardType);
                StartCoroutine(HideCoinsDelta());
            }
            else
            {
                float timePercent = _currentLerpRotationTime / _settings.RotationTime;

                timePercent = timePercent * timePercent * timePercent * (timePercent * (6f * timePercent - 15f) + 10f);

                float angle = Mathf.Lerp(_startAngle, _finalAngle, timePercent);
                _wheel.transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }

        private void ValidateSettings(FortuneWheelSettings settings)
        {
            _settings = Instantiate(settings);

            for (int i = 0; i < _settings.Sectors.Length; i++)
            {
                switch (_settings.Sectors[i].RewardType)
                {
                    case WheelRewardType.sword
                        when _settings.Sectors[i].RewardIds.Count == 0
                        || _settings.Sectors[i].RewardIds.All(j => PlayerData.PurchasedWeapons.Contains(j)):
                    case WheelRewardType.skill
                        when _settings.Sectors[i].RewardIds.Count == 0
                        || _settings.Sectors[i].RewardIds.All(j => PlayerData.PurchasedSkills.Contains(j)):
                    case WheelRewardType.coins_x2 when PlayerData.HasCoinsX2:
                    case WheelRewardType.fast_spin when PlayerData.HasFastSpin:

                        _settings.Sectors[i] = _settings.AlternativeSectors[i];
                        break;
                }

                switch (_settings.Sectors[i].RewardType)
                {
                    case WheelRewardType.sword:
                        _settings.Sectors[i].RewardIds.RemoveAll(j => PlayerData.PurchasedWeapons.Contains(j));
                        break;
                    case WheelRewardType.skill:
                        _settings.Sectors[i].RewardIds.RemoveAll(j => PlayerData.PurchasedSkills.Contains(j));
                        break;
                }
            }
        }

        private void ClearSubscribes()
        {
            //_input.InteractStart -= _box.OnPress;
            //_input.InteractCancel -= _box.OnCancel;
        }

        private async void GetReward(WheelRewardType rewardType)
        {
            _resultText.gameObject.SetActive(true);
            
            Vibrations.VibratePeek();

            switch (rewardType)
            {
                case WheelRewardType.coins_x2:
                    PlayerData.CoinsX2End = DateTime.Now.AddMinutes(_finalSector.RewardValue);
                    _resultText.text = string.Format(await _finalSector.RewardText.GetLocalizedStringAsync2(), _finalSector.RewardValue);
                    break;

                case WheelRewardType.sword:
                    int index = UnityEngine.Random.Range(0, _finalSector.RewardIds.Count);
                    PlayerData.PurchasedWeapons.Add(_finalSector.RewardIds[index]);
                    _resultText.text = string.Format(await _finalSector.RewardText.GetLocalizedStringAsync2(),
                        await StaticData.Settings.Weapons[_finalSector.RewardIds[index]].Name.GetLocalizedStringAsync2());
                    _finalSector.RewardIds.Remove(_finalSector.RewardIds[index]);

                    if (_finalSector.RewardIds.Count == 0)
                        _finalSector.Probability = 0;

                    break;

                case WheelRewardType.crystals:
                    PlayerData.Crystals.Value += _finalSector.RewardValue;
                    _resultText.text = string.Format(await _finalSector.RewardText.GetLocalizedStringAsync2(), _finalSector.RewardValue);
                    PlayerData.CurrencyGainedCount++;
                    Analytics.LogEvent(AnalyticsEvents.currency_gained,
                        (AnalyticsParameters.currency, "crystals"),
                        (AnalyticsParameters.source, "wheel"),
                        (AnalyticsParameters.amount, _finalSector.RewardValue),
                        (AnalyticsParameters.count, PlayerData.CurrencyGainedCount)
                    );
                    break;

                case WheelRewardType.coins:
                    int amount = _finalSector.RewardValue * (PlayerData.HasCoinsX2 ? 2 : 1);
                    PlayerData.Coins.Value += amount;
                    _resultText.text = string.Format(await _finalSector.RewardText.GetLocalizedStringAsync2(), _finalSector.RewardValue);
                    PlayerData.CurrencyGainedCount++;
                    Analytics.LogEvent(AnalyticsEvents.currency_gained,
                        (AnalyticsParameters.currency, "coins"),
                        (AnalyticsParameters.source, "wheel"),
                        (AnalyticsParameters.amount, amount),
                        (AnalyticsParameters.count, PlayerData.CurrencyGainedCount)
                    );
                    break;

                case WheelRewardType.skill:
                    index = UnityEngine.Random.Range(0, _finalSector.RewardIds.Count);
                    PlayerData.PurchasedSkills.Add(_finalSector.RewardIds[index]);
                    _resultText.text = string.Format(await _finalSector.RewardText.GetLocalizedStringAsync2(),
                        await StaticData.Settings.Skills[_finalSector.RewardIds[index]].Name.GetLocalizedStringAsync2());
                    _finalSector.RewardIds.Remove(_finalSector.RewardIds[index]);

                    if (_finalSector.RewardIds.Count == 0)
                        _finalSector.Probability = 0;

                    break;

                case WheelRewardType.fast_spin:
                    PlayerData.FastSpinEnd = DateTime.Now.AddMinutes(_finalSector.RewardValue);
                    _resultText.text = string.Format(await _finalSector.RewardText.GetLocalizedStringAsync2(), _finalSector.RewardValue);
                    break;
            }

            PersistentDataService.Save();

            if (_particle)
                _particle.Play();

            _audio.PlaySound(_audio.Sounds.ChestOpenSound);

            ButtonClose.interactable = true;

            UIService.HUD.RefreshBonusPanel();
        }

        private void TurnWheelForFree()
        {
            _ads.ShowRewarded(onSuccess: () =>
            {
                TurnWheel();
                
                PlayerData.LastRewardedSpinTime = DateTime.Now.Ticks.ToString();
                PersistentDataService.Save();
                SetNextFreeTime();
            });
        }

        private void TurnWheelForCoins()
        {
            TurnWheel();
            
            PlayerData.Crystals.Value -= _settings.TurnCost;
            PersistentDataService.Save();

            DeltaCoinsText.text = String.Format("-{0}", _settings.TurnCost);
            DeltaCoinsText.gameObject.SetActive(true);

            StartCoroutine(HideCoinsDelta());
        }  

        private void FreeSpin()
        {
            TurnWheel();

            PlayerData.FreeSpinsCount--;
            PersistentDataService.Save();
        }

        private void TurnWheel()
        {
            _currentLerpRotationTime = 0f;
            int[] sectorsAngles = new int[_settings.Sectors.Length];

            for (int i = 0; i < _settings.Sectors.Length; i++)
                sectorsAngles[i] = FULL_CIRCLE_DEGREE / _settings.Sectors.Length * i;

            double necessaryPercent = UnityEngine.Random.Range(1, _settings.Sectors.Sum(sector => sector.Probability));

            int cumulativeProbability = 0;
            int randomFinalAngle = sectorsAngles[0];
            _finalSector = _settings.Sectors[0];

            for (int i = 0; i < _settings.Sectors.Length; i++)
            {
                cumulativeProbability += _settings.Sectors[i].Probability;

                if (necessaryPercent <= cumulativeProbability)
                {
                    randomFinalAngle = sectorsAngles[i];
                    _finalSector = _settings.Sectors[i];
                    break;
                }
            }

            _finalAngle = _settings.FullTurnovers * FULL_CIRCLE_DEGREE + randomFinalAngle;
            _isStarted = true;
            ButtonClose.interactable = false;
        }

        public void TurnWheelButtonClick()
        {
            Vibrations.VibratePop();
            if (PlayerData.FreeSpinsCount > 0)
            {
                FreeSpin();
            }
            else if (_isRewardedTurnAvailable)
            {
                TurnWheelForFree();
            }
            else if (PlayerData.Crystals.Value >= _settings.TurnCost)
            {
                TurnWheelForCoins();
            }
        }

        public void SetNextFreeTime()
        {
            _timerRemainingHours = _settings.TimerMaxHours;
            _timerRemainingMinutes = _settings.TimerMaxMinutes;
            _timerRemainingSeconds = _settings.TimerMaxSeconds;

            // Get last free turn time value from storage
            // We can't save long int to PlayerPrefs so store this value as string and convert to long
            _nextFreeTurnTime = new DateTime(
                Convert.ToInt64(PlayerData.LastRewardedSpinTime != string.Empty ? PlayerData.LastRewardedSpinTime : DateTime.Now.Ticks.ToString()))
                .AddHours(_timerRemainingHours)
                .AddMinutes(_timerRemainingMinutes)
                .AddSeconds(_timerRemainingSeconds);

            _isRewardedTurnAvailable = false;
        }

        private void ShowTurnButtons()
        {
            if (PlayerData.FreeSpinsCount > 0)
            {
                ShowFreeSpinButton();
                
                if (_isStarted)
                    DisableButton(FreeSpinButton);
                else
                    EnableButton(FreeSpinButton);
            }
            else if (_isRewardedTurnAvailable)
            {
                ShowRewardedTurnButton();
                
                if (_isStarted)
                    DisableButton(RewardedTurnButton);
                else
                    EnableButton(RewardedTurnButton);
            }
            else
            {
                ShowPaidTurnButton();

                if (_isStarted || PlayerData.Crystals.Value < _settings.TurnCost)
                    DisablePaidTurnButton();
                else
                    EnablePaidTurnButton();
            }
        }

        private IEnumerator HideCoinsDelta()
        {
            yield return new WaitForSeconds(1f);
            DeltaCoinsText.gameObject.SetActive(false);
        }

        // Change remaining time to next free turn every 1 second
        private void UpdateRewardedTurnTimer()
        {
            // Don't count the time if we have free turn already
            if (_isRewardedTurnAvailable)
                return;

            // Update the remaining time values
            _timerRemainingHours = (int)(_nextFreeTurnTime - DateTime.Now).Hours;
            _timerRemainingMinutes = (int)(_nextFreeTurnTime - DateTime.Now).Minutes;
            _timerRemainingSeconds = (int)(_nextFreeTurnTime - DateTime.Now).Seconds;

            // If the timer has ended
            if (_timerRemainingHours <= 0 && _timerRemainingMinutes <= 0 && _timerRemainingSeconds <= 0)
            {
                // Now we have a free turn
                _isRewardedTurnAvailable = true;
            }
            else
            {
                // Show the remaining time
                NextRewardedTurnTimerText.text = String.Format("{0:00}:{1:00}:{2:00}", _timerRemainingHours, _timerRemainingMinutes, _timerRemainingSeconds);
                // We don't have a free turn yet
                _isRewardedTurnAvailable = false;
            }
        }

        private void EnableButton(Button button)
        {
            button.interactable = true;
            button.GetComponent<Image>().color = new Color(255, 255, 255, 1f);
        }

        private void DisableButton(Button button)
        {
            button.interactable = false;
            button.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
        }

        private void EnablePaidTurnButton() => EnableButton(PaidTurnButton);

        private void DisablePaidTurnButton() => DisableButton(PaidTurnButton);
        
        private void ShowFreeSpinButton()
        {
            FreeSpinButton?.gameObject.SetActive(true);
            RewardedTurnButton?.gameObject.SetActive(false);
            PaidTurnButton?.gameObject.SetActive(false);
        }
        
        private void ShowRewardedTurnButton()
        {
            RewardedTurnButton?.gameObject.SetActive(true);
            PaidTurnButton?.gameObject.SetActive(false);
            FreeSpinButton?.gameObject.SetActive(false);
        }

        private void ShowPaidTurnButton()
        {
            PaidTurnButton?.gameObject.SetActive(true);
            RewardedTurnButton?.gameObject.SetActive(false);
            FreeSpinButton?.gameObject.SetActive(false);
        }

        public void CreateFortune()
        {
            _offsetDistance = new Vector3(2 * Mathf.PI * CIRCLE_RADIUS * _sliceSize * 0.5f, CIRCLE_RADIUS / 1.1f);

            Image[] imageSlices = new Image[_fortuneSize];

            SpawnElements(ref imageSlices);
            SetSlicesText(imageSlices);
            SetSlicesIcons(imageSlices);

            Roller.transform.localScale *= _settings.WheelSize;
            _center.transform.localScale *= _settings.WheelSize;
        }

        private void SpawnElements(ref Image[] imageSlices)
        {
            float totalSliceSize = 0;

            for (int i = 0; i < _fortuneSize; ++i)
            {
                Slice newSlice = Instantiate(_settings.CirclePrefab, _wheel.transform);
                newSlice.transform.localPosition = new Vector3(0, 0, 0);
                newSlice.transform.localScale *= 1.1f;
                newSlice.name = _settings.Sectors[i].RewardType.ToString();

                totalSliceSize += _sliceSize;
                float rotAngle = FULL_CIRCLE_DEGREE * (1 + _sliceSize - totalSliceSize);
                newSlice.transform.Rotate(0, 0, rotAngle + FULL_CIRCLE_DEGREE * _sliceSize / 2);

                Image image = newSlice.GetComponent<Image>();
                image.fillAmount = _sliceSize;
                image.color = _settings.Sectors[i].FillColor;
                imageSlices[i] = image;
            }
        }

        private async void SetSlicesText(Image[] imageSlices)
        {
            for (int i = 0; i < _fortuneSize; ++i)
            {
                TMP_Text labelText = imageSlices[i].GetComponentInChildren<TMP_Text>();
                labelText.transform.Rotate(0, 0, _settings.LabelRotation);
                labelText.color = _settings.Sectors[i].LabelColor;
                labelText.fontSize = _settings.Sectors[i].FontSize;
                labelText.font = _settings.Font;
                labelText.text = await _settings.Sectors[i].Label.GetLocalizedStringAsync2();
                labelText.transform.localPosition = _offsetDistance * _settings.LabelOffset;

                labelText.gameObject.SetActive(_settings.Sectors[i].ShowLabelInWheel);
            }
        }

        private void SetSlicesIcons(Image[] imageSlices)
        {
            for (int i = 0; i < _fortuneSize; ++i)
            {
                Image icon = imageSlices[i].transform.GetChild(0).GetComponent<Image>();
                icon.transform.localPosition = _offsetDistance * _settings.IconOffset;
                icon.transform.localScale *= Mathf.Clamp(6 * _sliceSize, 0, 1.2f);
                icon.sprite = _settings.Sectors[i].Icon;
                icon.preserveAspect = true;

                icon.gameObject.SetActive(_settings.Sectors[i].Icon != null);
            }
        }
    }
}