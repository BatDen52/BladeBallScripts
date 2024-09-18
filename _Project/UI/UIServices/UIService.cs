using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _Project.UI
{
    public class UIService : IUIService
    {
        private readonly IObjectResolver _container;
        private readonly Dictionary<WindowId, WindowConfig> _windowConfigs;
        private readonly UIConfig _uiConfig;
        private readonly IInput _input;
        private readonly IAnalytics _analytics;
        
        private UIRoot _uiRoot;
        private LoadingScreen _loadingScreen;
        private RewardedRoundFinished _rewardedRoundFinished;
        private int _openedWindowsCount;

        public HUD HUD { get; private set; }
        
        public UIService(IObjectResolver container)
        {
            _container = container;
            _uiConfig = container.Resolve<UIConfig>();
            _windowConfigs = _uiConfig.WindowConfigs
                .ToDictionary(e => e.WindowId, e => e);
            _input = container.Resolve<IInput>();
            _analytics = container.Resolve<IAnalytics>();
        }


        public void Initialize(Camera uiCamera)
        {
            _loadingScreen = _container.Instantiate(_uiConfig.LoadingScreen);
            _uiRoot = _container.Instantiate(_uiConfig.UIRoot);
            _uiRoot.SetCanvasCamera(uiCamera);
            
            if (Application.isMobilePlatform)
            {
                CreateMobileCameraControl();
                CreateMobileStick();
                
            }
            else
            {
                CreateDesktopClickFullscreen();
            }
        }

        public void ShowLoadingScreen()
        {
            _loadingScreen.Show();
        }

        public void HideLoadingScreen()
        {
            _loadingScreen.FadeOut();
        }
        
        public Window Open(WindowId windowId)
        {
            _openedWindowsCount++;
            _analytics.LogEvent(AnalyticsEvents.open_window, 
                (AnalyticsParameters.name, windowId.ToString()));
            _input.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return _container.Instantiate(_windowConfigs[windowId].Prefab, _uiRoot.transform);
        }

        public void Close(Window window)
        {
            Object.Destroy(window.gameObject);
            _openedWindowsCount--;
            if (_openedWindowsCount <= 0)
            {
                _input.Enable();
                if (_input.IsRotationLocked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        public void CreateHUD()
        {
            HUD = _container.Instantiate(_uiConfig.HUD, _uiRoot.transform);
            if (Application.isMobilePlatform)
            {
                HUD.HideKeyboardKeys();
                HUD.HideCursorButton();
            }
            else
            {
                HUD.HideJumpButton();
                HUD.SubscribeShopOpen();
            }
        }
        
        public void ShowRewardedRoundFinished(int reward, Action onAccept, Action onDecline)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _rewardedRoundFinished = _container.Instantiate(_uiConfig.RewardedRoundFinished, _uiRoot.transform);
            _rewardedRoundFinished.TextReward.SetText(reward.ToString());
            _rewardedRoundFinished.ButtonAccept.onClick.AddListener(() => onAccept());
            _rewardedRoundFinished.ButtonDecline.onClick.AddListener(() => onDecline());
            DOTween.Sequence().AppendInterval(_rewardedRoundFinished.DeclineDelay).AppendCallback(() => _rewardedRoundFinished.ButtonDecline.gameObject.SetActive(true));
        }

        public void CloseRewardedRoundFinished()
        {
            if (_rewardedRoundFinished != null)
            {
                Object.Destroy(_rewardedRoundFinished.gameObject);
                if (_input.IsRotationLocked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
        
        public void CreateMobileCameraControl()
        {
            _container.Instantiate(_uiConfig.MobileCameraControl, _uiRoot.transform);
        }
        
        public void CreateMobileStick()
        {
            _container.Instantiate(_uiConfig.MobileStick, _uiRoot.transform);
        }
        
        public void CreateDesktopClickFullscreen()
        {
            _container.Instantiate(_uiConfig.DesktopClickFullscreen, _uiRoot.transform);
        }

        public Button CreateBackToGameButton()
        {
            return _container.Instantiate(_uiConfig.ButtonBackToGame, _uiRoot.transform);
        }
    }
}