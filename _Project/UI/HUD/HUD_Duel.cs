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
    public class HUD_Duel
    {
        [SerializeField] private List<Image> RedStarsBlockers = new List<Image>();
        [SerializeField] private List<Image> BlueStarsBlockers = new List<Image>();

        [SerializeField] private TextMeshProUGUI RedScoreText;
        [SerializeField] private TextMeshProUGUI BlueScoreText;

        [SerializeField] private GameObject DuelHUDRoot;
        [SerializeField] private GameObject TextYouKill;

        public bool IsActive => DuelHUDRoot.activeSelf;

        public void Hide(bool hide)
        {
            DuelHUDRoot.SetActive(!hide);
        }

        public void ConfigureDuelHUD(GameplayState gameplayState)
        {
            gameplayState.PlayerDuelScore.Subscribe(value =>
            {
                if (value > 0)
                {
                    int idx = value - 1;
                    RedStarsBlockers[idx].transform.DOScaleY(0.0f, 0.7f)
                                .SetEase(Ease.InElastic);

                    RedScoreText.text = value.ToString();
                }
            });

            RedScoreText.text = gameplayState.PlayerDuelScore.Value.ToString();
            for (int i = 0; i < RedStarsBlockers.Count; i++) 
            {
                RedStarsBlockers[i].transform.localScale = Vector3.one;
            }

            gameplayState.BotsDuelScore.Subscribe(value =>
            {
                if (value > 0)
                {
                    int idx = value - 1;
                    BlueStarsBlockers[idx].transform.DOScaleY(0.0f, 0.7f)
                                .SetEase(Ease.InElastic);

                    BlueScoreText.text = value.ToString();
                }
            });

            BlueScoreText.text = gameplayState.BotsDuelScore.Value.ToString();
            for (int i = 0; i < BlueStarsBlockers.Count; i++)
            {
                BlueStarsBlockers[i].transform.localScale = Vector3.one;
            }
        }

        public void ShowKillText(float duration)
        {
            TextYouKill.SetActive(true);
            DOTween.Sequence().AppendInterval(duration).AppendCallback(() => TextYouKill.SetActive(false));
        }

    }
}