using System;
using DG.Tweening;
using UnityEngine;

namespace _Project
{
    [Serializable]
    public abstract class DOTweenAnimation : MonoBehaviour
    {
        [SerializeField] protected bool _animateOnEnable = true;
        [SerializeField] protected float _duration;
        [SerializeField] protected int _loops = -1;
        [SerializeField] protected LoopType _loopType = LoopType.Yoyo;
        [SerializeField] protected Ease _ease = Ease.InOutSine;
        [SerializeField] protected bool _useAnimationCurve;
        [SerializeField] protected AnimationCurve _animationCurve;
        [SerializeField] protected bool _isUnscaledTime;
        
        protected Tween _tween;

        public virtual void Animate()
        {
            _tween = GetTween().SetUpdate(_isUnscaledTime);
            _tween.Play();
        }
        public abstract Tween GetTween();
    }
}