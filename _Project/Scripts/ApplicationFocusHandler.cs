using System;
using UnityEngine;
using VContainer;

namespace _Project
{
    public class ApplicationFocusHandler : MonoBehaviour
    {
        private IAds _ads;
        
        [Inject]
        public void Construct(IAds ads)
        {
            _ads = ads;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (_ads.IsActive) return;
            AudioListener.volume = focus ? 1.0f : 0.0f;
            Time.timeScale = focus ? 1.0f : 0.0f;
        }
    }
}