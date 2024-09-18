using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using _Project.Data.Static.FortuneWheel;

namespace _Project
{
    public class FortuneWheel : MonoBehaviour, ITimePressable
    {
        private IPersistentDataService _persistentDataService;
        private PlayerData _playerData;
        private IUIService _uiService;
        private IAnalytics _analytics;

        private FortuneWheelWindow _window;
        private CoroutineHandle _timerRoutine;

        public FortuneWheelSettings Settings { get; protected set; }
        public float NeedPressTime => Settings.NeedPressTime;
        public FloatReactiveProperty PressTime { get; private set; } = new FloatReactiveProperty();

        private bool _isConstructed = false;

        [Inject]
        public void Construct(IObjectResolver container)
        {
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _playerData = _persistentDataService.PersistentData.PlayerData;
            _uiService = container.Resolve<IUIService>();
            _analytics = container.Resolve<IAnalytics>();
            Settings = container.Resolve<StaticData>().Settings.FortuneWheelSettings;
            _isConstructed = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Character character))
            {
                ShowOpenButton();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Character character))
            {
                _uiService.HUD.HideInteractButton(OnPress, OnCancel, this);
            }
        }

        private void OnDestroy()
        {
            if (_isConstructed)
                _uiService.HUD.HideInteractButton(OnPress, OnCancel, this);
        }

        public bool CanOpen() => true;// Settings.TurnCost <= _playerData.Crystals.Value;

        public void ResetPressTime() => PressTime.Value = 0;

        public void OnPress() => _timerRoutine = Timing.RunCoroutine(TickTimerPress());

        public void OnCancel()
        {
            PressTime.Value = 0;
            Timing.KillCoroutines(_timerRoutine);
        }

        protected void OpenWindow()
        {
            if (_window != null)
            {
                _window.Closing -= OnClosingWindow;
                _window.Close();
            }

            _uiService.HUD.HideInteractButton(OnPress, OnCancel, this);

            _window = _uiService.Open(Settings.Window) as FortuneWheelWindow;
            //Window.Init();
            _window.Closing += OnClosingWindow;
        }

        protected async void ShowOpenButton()
        {
            string buttonText = await Settings.BtnOpenLocale.GetLocalizedStringAsync2();

            bool isInteractable = CanOpen();
            _uiService.HUD.ShowInteractButton(buttonText, isInteractable, OnPress, OnCancel, this);

            ResetPressTime();
        }

        protected IEnumerator<float> TickTimerPress()
        {
            while (PressTime.Value < NeedPressTime)
            {
                PressTime.Value += Time.deltaTime;
                yield return 0;
            }

            BuyOpen();
        }

        protected void BuyOpen()
        {
            //if (PlayerData.Crystals.Value < Settings.TurnCost)
            //    return;

            //PlayerData.Crystals.Value -= Settings.TurnCost;

            //PlayerData.OpenPremiumWeaponChestCount++;
            //Analytics.LogEvent(AnalyticsEvents.OPEN_PREMIUM_WEAPONS_CHEST, PlayerData.OpenPremiumWeaponChestCount);

            //PersistentDataService.Save();

            OpenWindow();
        }

        private void OnClosingWindow()
        {
            _window.Closing -= OnClosingWindow;
            _window = null;

            ShowOpenButton();
        }
    }
}