using System;
using UnityEngine;
using VContainer;

namespace _Project.Timers
{
	public delegate void TimerValueChangedHandler(float remainingSeconds, TimeChangingSource changingSource);
	
    public class SyncedTimer
    {
        public event TimerValueChangedHandler TimerValueChanged;
		public event Action TimerFinished;
		public event Action<SyncedTimer> TimerStarted;

		public TimerType Type { get; }
		public bool IsActive { get; private set; }
		public bool IsPaused { get; private set; }
		public float RemainingSeconds { get; private set; }

		private TimeInvoker _timeInvoker;
		
		[Inject]
		public SyncedTimer(TimerType type, TimeInvoker timeInvoker)
		{
			_timeInvoker = timeInvoker;
			Type = type;
		}

		public SyncedTimer(TimerType type, float seconds, TimeInvoker timeInvoker)
		{
			_timeInvoker = timeInvoker;
			Type = type;

			SetTime(seconds);
		}

		public void SetTime(float seconds)
		{
			RemainingSeconds = seconds;
			TimerValueChanged?.Invoke(RemainingSeconds, TimeChangingSource.TimeForceChanged);
		}

		public void Start()
		{
			if (IsActive)
				return;

			if (Math.Abs(RemainingSeconds) < Mathf.Epsilon)
			{
				TimerFinished?.Invoke();
			}

			IsActive = true;
			IsPaused = false;
			SubscribeOnTimeInvokerEvents();

			TimerStarted?.Invoke(this);
			TimerValueChanged?.Invoke(RemainingSeconds, TimeChangingSource.TimerStarted);
		}

		public void Start(float seconds)
		{
			if (IsActive)
				return;

			SetTime(seconds);
			Start();
		}

		public void Restart(float seconds)
		{
			IsActive = false;
			
			SetTime(seconds);
			Start();
		}

		public void Pause()
		{
			if (IsPaused || !IsActive)
				return;

			IsPaused = true;
			UnsubscribeFromTimeInvokerEvents();

			TimerValueChanged?.Invoke(RemainingSeconds, TimeChangingSource.TimerPaused);
		}

		public void Unpause()
		{
			if (!IsPaused || !IsActive)
				return;

			IsPaused = false;
			SubscribeOnTimeInvokerEvents();

			TimerValueChanged?.Invoke(RemainingSeconds, TimeChangingSource.TimerUnpaused);
		}

		public void Stop()
		{
			if (IsActive)
			{
				UnsubscribeFromTimeInvokerEvents();
				
				RemainingSeconds = 0f;
				IsActive = false;
				IsPaused = false;

				TimerValueChanged?.Invoke(RemainingSeconds, TimeChangingSource.TimerFinished);
				TimerFinished?.Invoke();
			}
		}

		private void SubscribeOnTimeInvokerEvents()
		{
			switch (Type)
			{
				case TimerType.UpdateTick:
					_timeInvoker.OnUpdateTimeTickedEvent += OnTicked;
					break;
				case TimerType.UpdateTickUnscaled:
					_timeInvoker.OnUpdateTimeUnscaledTickedEvent += OnTicked;
					break;
				case TimerType.OneSecTick:
					_timeInvoker.OnOneSyncedSecondTickedEvent += OnSyncedSecondTicked;
					break;
				case TimerType.OneSecTickUnscaled:
					_timeInvoker.OnOneSyncedSecondUnscaledTickedEvent += OnSyncedSecondTicked;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void UnsubscribeFromTimeInvokerEvents()
		{
			switch (Type)
			{
				case TimerType.UpdateTick:
					_timeInvoker.OnUpdateTimeTickedEvent -= OnTicked;
					break;
				case TimerType.UpdateTickUnscaled:
					_timeInvoker.OnUpdateTimeUnscaledTickedEvent -= OnTicked;
					break;
				case TimerType.OneSecTick:
					_timeInvoker.OnOneSyncedSecondTickedEvent -= OnSyncedSecondTicked;
					break;
				case TimerType.OneSecTickUnscaled:
					_timeInvoker.OnOneSyncedSecondUnscaledTickedEvent -= OnSyncedSecondTicked;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void CheckFinish()
		{
			if (RemainingSeconds <= 0f)
			{
				Stop();
			}
		}

		private void NotifyAboutTimePassed()
		{
			if (RemainingSeconds >= 0f)
			{
				TimerValueChanged?.Invoke(RemainingSeconds, TimeChangingSource.TimePassed);
			}
		}

		private void OnTicked(float deltaTime)
		{
			RemainingSeconds -= deltaTime;
			
			NotifyAboutTimePassed();
			CheckFinish();
		}

		private void OnSyncedSecondTicked()
		{
			RemainingSeconds -= 1;
			
			NotifyAboutTimePassed();
			CheckFinish();
		}
    }
}