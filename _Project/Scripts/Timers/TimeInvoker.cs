using System;
using UnityEngine;

namespace _Project.Timers
{
    public class TimeInvoker : MonoBehaviour
    {
        public event Action<float> OnUpdateTimeTickedEvent;
        public event Action<float> OnUpdateTimeUnscaledTickedEvent;
        public event Action OnOneSyncedSecondTickedEvent;
        public event Action OnOneSyncedSecondUnscaledTickedEvent;
        
        private float _oneSecTimer;
        private float _oneSecUnscaledTimer;

        private void Update()
        {
            var deltaTimer = Time.deltaTime;
			
            OnUpdateTimeTickedEvent?.Invoke(deltaTimer);

            _oneSecTimer += deltaTimer;
			
            if (_oneSecTimer >= 1f)
            {
                _oneSecTimer -= 1f;
				
                OnOneSyncedSecondTickedEvent?.Invoke();
            }

            var unscaledDeltaTimer = Time.unscaledDeltaTime;
			
            OnUpdateTimeUnscaledTickedEvent?.Invoke(Time.unscaledDeltaTime);
			
            _oneSecUnscaledTimer += unscaledDeltaTimer;
			
            if (_oneSecUnscaledTimer >= 1f)
            {
                _oneSecUnscaledTimer -= 1f;
				
                OnOneSyncedSecondUnscaledTickedEvent?.Invoke();
            }
        }
    }
}