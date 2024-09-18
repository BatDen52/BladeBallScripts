using DG.Tweening;
using UnityEngine;

namespace _Project
{
    public class DOTweenScale : DOTweenAnimation
    {
        [SerializeField] private float _endScale;
        
        private Vector3 _initialScale; 
        
        private void OnEnable()
        {
            _initialScale = transform.localScale;
            if (_animateOnEnable) Animate();
        }

        private void OnDisable()
        {
            _tween.Kill();
            transform.localScale = _initialScale;
        }
        
        public override Tween GetTween()
        {
            _tween = transform.DOScale(_endScale, _duration).SetLoops(_loops, _loopType);
            if (_useAnimationCurve) _tween.SetEase(_animationCurve);
            else _tween.SetEase(_ease);
            _tween.Pause();
            return _tween;
        }
    }
}