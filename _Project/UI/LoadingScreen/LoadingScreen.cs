using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _loadingIndicator;
        [SerializeField] private float _stepTime = 0.03f;
        [SerializeField] private float _stepAlfa = 0.03f;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1;
        }

        public void FadeIn(Action then = null)
        {
            gameObject.SetActive(true);
            Timing.RunCoroutine(_FadeInCoroutine(then), Segment.RealtimeUpdate);
        }

        public void FadeOut(Action then = null)
        {
            gameObject.SetActive(true);
            Timing.RunCoroutine(_FadeOutCoroutine(then), Segment.RealtimeUpdate);
        }

        private IEnumerator<float> _FadeInCoroutine(Action then)
        {
            while (_canvasGroup.alpha < 1)
            {
                _canvasGroup.alpha += _stepAlfa;
                yield return Timing.WaitForSeconds(_stepTime);
            }
      
            then?.Invoke();
        }

        private IEnumerator<float> _FadeOutCoroutine(Action then)
        {
            while (_canvasGroup.alpha > 0)
            {
                _canvasGroup.alpha -= _stepAlfa;
                yield return Timing.WaitForSeconds(_stepTime);
            }
      
            gameObject.SetActive(false);
            then?.Invoke();
        }
        
        public void ShowIndicator() => _loadingIndicator.SetActive(true);
        public void HideIndicator() => _loadingIndicator.SetActive(false);
    }
}