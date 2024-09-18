using _Project.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.UI
{
    public class ButtonWithCooldown : MonoBehaviour
    {
        public Button Button;
        
        [SerializeField] private GameObject _cooldownObject;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private string _format = "{0:0}:{1:00}";
        
        private TimeInvoker _timeInvoker;
        private SyncedTimer _timer;
        private bool _isTimerActive;

        [Inject]
        private void Construct(TimeInvoker timeInvoker)
        {
            _timeInvoker = timeInvoker;
        }
        
        public void Cooldown(float seconds, bool restart=true)
        {
            if (_isTimerActive && !restart)
            {
                return;
            }
            
            _timer = new SyncedTimer(TimerType.OneSecTick, _timeInvoker);
            _isTimerActive = true;
            Button.interactable = false;
            _timer.SetTime(seconds);
            _timer.Start();
            _cooldownText.SetText(seconds.ToTimeString(_format));
            _cooldownObject.SetActive(true);
            _timer.TimerFinished += OnTimerFinished;
            _timer.TimerValueChanged += OnTimerValueChanged;
        }
        
        public void Cooldown(SyncedTimer timer, bool restart=true)
        {
            Unsubscribe();
            
            if (_isTimerActive && !restart)
            {
                return;
            }
            
            _timer = timer;
            
            if (_timer.IsActive)
            {
                _isTimerActive = true;
                Button.interactable = false;
                _cooldownText.SetText(_timer.RemainingSeconds.ToTimeString(_format));
                _cooldownObject.SetActive(true);
            }
            
            _timer.TimerFinished += OnTimerFinished;
            _timer.TimerValueChanged += OnTimerValueChanged;
            _timer.TimerStarted += OnTimerTimerStarted;
        }

        private void Unsubscribe()
        {
            if (_timer != null)
            {
                _timer.TimerFinished -= OnTimerFinished;
                _timer.TimerValueChanged -= OnTimerValueChanged;
                _timer.TimerStarted -= OnTimerTimerStarted;
            }
        }

        private void OnTimerTimerStarted(SyncedTimer timer)
        {
            Cooldown(timer);
        }

        private void OnTimerFinished()
        {
            Button.interactable = true;
            _isTimerActive = false;
            _cooldownObject.SetActive(false);
        }

        private void OnTimerValueChanged(float remainingSeconds, TimeChangingSource changingSource)
        {
            _cooldownText.SetText(remainingSeconds.ToTimeString(_format));
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}