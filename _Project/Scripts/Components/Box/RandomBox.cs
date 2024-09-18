using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.Data.Static.Boxes;
using _Project.UI;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project
{
    public abstract class RandomBox : MonoBehaviour, ITimePressable
    {
        [SerializeField] protected BoxContentType BoxContentType = BoxContentType.Weapon;
        [SerializeField] protected TypeBox TypeBox;

        protected IPersistentDataService PersistentDataService;
        protected PlayerData PlayerData;
        protected StaticData StaticData;
        protected IUIService UiService;
        protected IAnalytics Analytics;

        protected Button OpenBoxButton;
        protected BoxView Window;
        private CoroutineHandle _timerRoutine;
        private bool _isConstructed = false;

        public BoxSettings Settings { get; protected set; }
        public float NeedPressTime => Settings.NeedPressTime;
        public FloatReactiveProperty PressTime { get; private set; } = new FloatReactiveProperty();

        [Inject]
        public void Construct(IObjectResolver container)
        {
            PersistentDataService = container.Resolve<IPersistentDataService>();
            PlayerData = PersistentDataService.PersistentData.PlayerData;
            StaticData = container.Resolve<StaticData>();
            UiService = container.Resolve<IUIService>();
            Analytics = container.Resolve<IAnalytics>();
            Settings = StaticData.Settings.BoxesSettings
                .First(i => i.BoxContentType == BoxContentType && i.TypeBox == TypeBox);
            Init();

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
                UiService.HUD.HideInteractButton(OnPress, OnCancel, this);
            }
        }

        private void OnDestroy()
        {
            if (_isConstructed)
                UiService.HUD.HideInteractButton(OnPress, OnCancel, this);
        }

        public async Task<string> GetOpenPriceText()
        {
            return string.Format(await Settings.BtnOpenLocale.GetLocalizedStringAsync2(), Settings.OpenPrice);
        }

        public bool CanOpen()
        {
            return
                (Settings.OpenPrice <=
                 (TypeBox == TypeBox.Simple ? PlayerData.Coins.Value : PlayerData.Crystals.Value)) || CanOpenFree();
        }

        public bool CanOpenFree()
        {
            return PlayerData.FreeBoxOpenings.ContainsKey(BoxContentType + "_" + TypeBox) &&
                   PlayerData.FreeBoxOpenings[BoxContentType + "_" + TypeBox] > 0;
        }

        public void ResetPressTime()
        {
            PressTime.Value = 0;
        }

        public void OnPress()
        {
            _timerRoutine = Timing.RunCoroutine(TickTimerPress());
        }

        public void OnCancel()
        {
            PressTime.Value = 0;
            Timing.KillCoroutines(_timerRoutine);
        }

        protected void OpenWindow()
        {
            if (Window != null)
            {
                Window.Closing -= OnClosingWindow;
                Window.Close();
            }

            UiService.HUD.HideInteractButton(OnPress, OnCancel, this);

            Window = UiService.Open(Settings.BoxWindow) as BoxView;
            Window.Init(this, OnFailOpen, PlayerData);
            Window.Closing += OnClosingWindow;
        }

        protected async void ShowOpenButton()
        {
            string buttonText = await GetOpenPriceText();

            bool isInteractable = CanOpen() && HasItemForGeneration();

            OpenBoxButton =
                UiService.HUD.ShowInteractButton(buttonText, isInteractable, OnPress, OnCancel, this, CanOpenFree());

            ResetPressTime();
        }

        protected IEnumerator<float> TickTimerPress()
        {
            if (PlayerData.HasFastSpin == false)
            {
                while (PressTime.Value < NeedPressTime)
                {
                    PressTime.Value += Time.deltaTime;
                    yield return 0;
                }
            }

            BuyOpen();
        }

        protected void BuyOpen()
        {
            if (CanOpenFree())
            {
                PlayerData.FreeBoxOpenings[BoxContentType + "_" + TypeBox]--;
            }
            else if (TypeBox == TypeBox.Simple)
            {
                if (PlayerData.Coins.Value < Settings.OpenPrice)
                    return;

                PlayerData.Coins.Value -= Settings.OpenPrice;

                PlayerData.CurrencySpentCount++;
                Analytics.LogEvent(AnalyticsEvents.currency_spent,
                    (AnalyticsParameters.currency, "coins"),
                    (AnalyticsParameters.type, "open_chest"),
                    (AnalyticsParameters.item, BoxContentType + "_" + TypeBox),
                    (AnalyticsParameters.price, Settings.OpenPrice),
                    (AnalyticsParameters.count, PlayerData.CurrencySpentCount)
                );
            }
            else if (TypeBox == TypeBox.Premium)
            {
                if (PlayerData.Crystals.Value < Settings.OpenPrice)
                    return;

                PlayerData.Crystals.Value -= Settings.OpenPrice;
                
                PlayerData.CurrencySpentCount++;
                Analytics.LogEvent(AnalyticsEvents.currency_spent,
                    (AnalyticsParameters.currency, "crystals"),
                    (AnalyticsParameters.type, "open_chest"),
                    (AnalyticsParameters.item, BoxContentType + "_" + TypeBox),
                    (AnalyticsParameters.price, Settings.OpenPrice),
                    (AnalyticsParameters.count, PlayerData.CurrencySpentCount)
                );
            }

            PersistentDataService.Save();

            OpenWindow();
        }

        private void OnClosingWindow()
        {
            Window.Closing -= OnClosingWindow;
            Window = null;

            ShowOpenButton();
        }

        private void OnFailOpen()
        {
            if (TypeBox == TypeBox.Simple)
            {
                PlayerData.Coins.Value += Settings.FailCashBack;
            }
            else if (TypeBox == TypeBox.Premium)
            {
                PlayerData.Crystals.Value += Settings.FailCashBack;
            }

            PersistentDataService.Save();
        }

        public abstract void Init();
        public abstract bool UnlockItem(int id);
        public abstract int GetRandomItemId();
        public abstract void ShowItem(Image image, TMP_Text text, string textAddon = "");
        public abstract bool HasItemForGeneration();
    }
}