using _Project.Data.Persistent;
using _Project.Data.Static;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using VContainer;

namespace _Project.UI
{
    [System.Serializable]
    public class HUD_TimeCountdown
    {
        [SerializeField] private List<TextMeshProUGUI> Numbers = new List<TextMeshProUGUI>();
        [SerializeField] private bool ShowZero = true;
        [SerializeField] private bool EnableSounds = true;

        [SerializeField] private Sound CountSound;
        [SerializeField] private Sound FinalSound;

        public void Hide(bool hide)
        {
            for (int i = 0; i < Numbers.Count; i++)
            {
                Numbers[i].gameObject.SetActive(!hide);
            }
        }

        public void StartCountdown(float seconds, IAudio audio)
        {
            for (int i = 0; i < Numbers.Count; i++)
            {
                var c = Numbers[i].color;
                Numbers[i].color = new Color(c.r, c.g, c.b, 0.0f);
                Numbers[i].gameObject.SetActive(true);
            }

            Timing.RunCoroutine(_Countdown(seconds, ShowZero, audio));
        }

        private IEnumerator<float> _Countdown(float seconds, bool showZero, IAudio audio)
        {
            float startTime = Time.time;
            float delay = seconds - Numbers.Count - 1;
            delay = Mathf.Max(delay, 0.0f);
            if (delay > 0)
                yield return Timing.WaitForSeconds(delay);
            float delayed = Time.time - startTime;
            float numberShowDuration = 1.0f - (delayed - delay);

            int idx = Numbers.Count - 1;
            idx = Math.Min(idx, (int)Mathf.Floor(seconds));

            Sequence numberSequence = DOTween.Sequence();

            float yMoveOffset = 1.0f;

            if (numberShowDuration > 0)
            {
                if (CountSound != null && EnableSounds)
                {
                    numberSequence.AppendCallback(() =>
                    {
                        audio.PlaySound(CountSound);
                    });
                }

                var number = Numbers[idx];
                var c = number.color;

                float duration = numberShowDuration * 0.2f;
                var t0 = DOTween.To(() => Numbers[idx].color, x => Numbers[idx].color = x, new Color(c.r, c.g, c.b, 1.0f), duration);
                numberSequence.Append(t0);

                duration = Mathf.Min(0.5f, numberShowDuration * 0.5f);
                var pos = number.transform.position;
                number.transform.position = new Vector3(pos.x , pos.y - yMoveOffset, pos.z);
                var jumpTweener = number.transform.DOMoveY(pos.y, duration).SetEase(Ease.OutBack);
                numberSequence.Join(jumpTweener);

                //var fadeOutTweener = mat.DOFade(0.0f, Mathf.Min(0.5f, numberShowDuration * 0.5f));
                //numberSequence.Append(fadeOutTweener);
                duration = Mathf.Min(0.5f, numberShowDuration * 0.5f);
                var t1 = DOTween.To(() => Numbers[idx].color, x => Numbers[idx].color = x, new Color(c.r, c.g, c.b, 0.0f), duration);
                numberSequence.Append(t1);
            }
            else
            {
                numberShowDuration += 1.0f;
            }

            for (int i = idx - 1; i >= 0; i--)
            {
                if (numberShowDuration <= 0)
                {
                    numberShowDuration += 1.0f;
                    continue;
                }
                if (i == 0 && !showZero)
                {
                    numberSequence.AppendInterval(1.0f);
                    break;
                }

                if (CountSound != null && EnableSounds)
                {
                    if (i > 0)
                    {
                        numberSequence.AppendCallback(() =>
                        {
                            audio.PlaySound(CountSound);
                        });
                    } else
                    {
                        numberSequence.AppendCallback(() =>
                        {
                            audio.PlaySound(FinalSound);
                        });
                    }
                }

                var number = Numbers[i];

                var c = number.color;
                float duration = numberShowDuration * 0.2f;
                var t0 = DOTween.To(() => number.color, x => number.color = x, new Color(c.r, c.g, c.b, 1.0f), duration);
                numberSequence.Append(t0);


                //var jumpTweener = number.transform.DOLocalJump(jumpEndValue, 2.0f, 1, Mathf.Min(0.5f, numberShowDuration * 0.5f), false);
                //numberSequence.Join(jumpTweener);

                duration = Mathf.Min(0.5f, numberShowDuration * 0.5f);
                var pos = number.transform.position;
                number.transform.position = new Vector3(pos.x, pos.y - yMoveOffset, pos.z);
                var jumpTweener = number.transform.DOMoveY(pos.y, duration).SetEase(Ease.OutBack);
                numberSequence.Join(jumpTweener);

                duration = Mathf.Min(0.5f, numberShowDuration * 0.5f);
                var t1 = DOTween.To(() => number.color, x => number.color = x, new Color(c.r, c.g, c.b, 0.0f), duration);
                numberSequence.Append(t1);

                if (numberShowDuration > 0.0f)
                    numberShowDuration = 1.0f;
            }

            numberSequence.AppendCallback(() => { Hide(true); });

        }



    }
}