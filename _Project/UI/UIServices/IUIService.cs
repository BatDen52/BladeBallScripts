using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI
{
    public interface IUIService
    {
        void Initialize(Camera uiCamera);
        Window Open(WindowId windowId);
        void CreateHUD();
        void Close(Window window);
        void ShowLoadingScreen();
        void HideLoadingScreen();
        HUD HUD { get; }
        void ShowRewardedRoundFinished(int reward, Action onAccept, Action onDecline);
        void CloseRewardedRoundFinished();
        void CreateDesktopClickFullscreen();
        Button CreateBackToGameButton();
    }
}