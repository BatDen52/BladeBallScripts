using UniRx;
using UnityEngine;
using VContainer;

namespace _Project.UI
{
    public class WindowSettings : Window
    {
        [SerializeField] private ButtonToggle _buttonToggleMusicMute;
        [SerializeField] private ButtonToggle _buttonToggleSoundsMute;
        [SerializeField] private ButtonToggle _buttonToggleVibrations;

        private IAudio _audio;

        protected override void Initialize()
        {
            base.Initialize();

            Time.timeScale = 0f;

            if (Application.isMobilePlatform == false && Application.isEditor == false)
            {
                _buttonToggleVibrations.gameObject.SetActive(false);
            }
        }

        protected override void Subscribe()
        {
            base.Subscribe();
            
            _buttonToggleMusicMute.Button.onClick.AddListener(ToggleMusicMute);
            _buttonToggleSoundsMute.Button.onClick.AddListener(ToggleSoundsMute);
            _buttonToggleVibrations.Button.onClick.AddListener(ToggleVibrations);
            
            PersistentData.AudioSettings.IsMusicMuted.Subscribe(value =>
            {
                _buttonToggleMusicMute.Set(!value);
            }).AddTo(Subscribes);
            
            PersistentData.AudioSettings.IsSoundsMuted.Subscribe(value =>
            {
                _buttonToggleSoundsMute.Set(!value);
            }).AddTo(Subscribes);
            
            PersistentData.VibrationsEnabled.Subscribe(value =>
            {
                _buttonToggleVibrations.Set(value);
            }).AddTo(Subscribes);

        }

        protected override void Cleanup()
        {
            base.Cleanup();

            Time.timeScale = 1f;
        }

        private void ToggleMusicMute()
        {
            PersistentData.AudioSettings.IsMusicMuted.Value = !PersistentData.AudioSettings.IsMusicMuted.Value;
            PersistentDataService.Save();
            Vibrations.VibratePop();
        }

        private void ToggleSoundsMute()
        {
            PersistentData.AudioSettings.IsSoundsMuted.Value = !PersistentData.AudioSettings.IsSoundsMuted.Value;
            PersistentDataService.Save();
            Vibrations.VibratePop();
        }

        private void ToggleVibrations()
        {
            PersistentData.VibrationsEnabled.Value = !PersistentData.VibrationsEnabled.Value;
            PersistentDataService.Save();
            Vibrations.VibratePop();
        }
    }
}