using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "_Project/UIConfig", order = 0)]
    public class UIConfig : ScriptableObject
    {
        public UIRoot UIRoot;
        public LoadingScreen LoadingScreen;
        public HUD HUD;
        public RewardedRoundFinished RewardedRoundFinished;
        public List<WindowConfig> WindowConfigs;
        public GameObject MobileCameraControl;
        public GameObject MobileStick;
        public GameObject DesktopClickFullscreen;
        public Button ButtonBackToGame;
    }
}