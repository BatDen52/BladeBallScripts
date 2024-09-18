using _Project.Data.Persistent;
using _Project;
using _Project.Data.Static;
using DG.Tweening;
using System;
using Timers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using System.Linq;
using System.Collections;

public class TimedRewardIcon : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _bar;
    [SerializeField] private TMP_Text _timeText;

    private Settings _settings;
    private IPersistentDataService _persistentDataService;
    private PlayerData _playerData;
    private IGameFactory _gameFactory;
    private Timer _timerShowingIcon;

    [Inject]
    public void Constructor(IObjectResolver container)
    {
        _gameFactory = container.Resolve<IGameFactory>();
        _persistentDataService = container.Resolve<IPersistentDataService>();
        _playerData = _persistentDataService.PersistentData.PlayerData;
        _settings = container.Resolve<StaticData>().Settings;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(HideIcon);
        (transform as RectTransform).DOAnchorPosY(200, 0.01f);
        StartCoroutine(StartTimers());
    }

    private void OnDisable()
    {
        TimersManager.ClearTimer(ShowIcon);
        TimersManager.ClearTimer(HideIcon);
        TimersManager.ClearTimer(TryRemoveTimedSkill);
        TimersManager.ClearTimer(RefreshProgressBar);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }

    private IEnumerator StartTimers()
    {
        if (_settings == null)
            yield return new WaitUntil(() => _settings == null);

        TimersManager.SetTimer(this, _settings.TimedReward.FirstShowTime, ShowIcon);
        TimersManager.SetLoopableTimer(this, 1, TryRemoveTimedSkill);
        TryRemoveTimedSkill();
    }

    private void TryRemoveTimedSkill()
    {
        if (_playerData.TimedSkills.Count == 0)
            return;

        var ids = _playerData.TimedSkills.Where(i => i.Value < DateTime.Now).Select(i => i.Key).ToList();

        foreach (var id in ids)
        {
            if (_playerData.CurrentSkillId == id)
            {
                _playerData.CurrentSkillId = _playerData.PurchasedSkills[_playerData.PurchasedSkills.Count - 1];
                _persistentDataService.Save();
                _gameFactory.Player.SkillActivator.SetSkill(_settings.Skills[_playerData.CurrentSkillId]);
            }

            _playerData.TimedSkills.Remove(id);
        }
    }

    private void ShowIcon()
    {
        TimersManager.SetTimer(this, _settings.TimedReward.TimedIconDuration, HideIcon);
        int id = TimersManager.SetTimer(this, 1, (uint)_settings.TimedReward.TimedIconDuration, RefreshProgressBar);
        _timerShowingIcon = TimersManager.GetTimer(id);

        (transform as RectTransform).DOAnchorPosY(-100, 1f)
            .OnComplete(() => (transform as RectTransform).DOAnchorPosY(-75, 0.5f));

        RefreshProgressBar();
    }

    private void RefreshProgressBar()
    {
        _timeText.text = _timerShowingIcon.RemainingLoopsCount.ToString();
        _bar.fillAmount = (float)_timerShowingIcon.RemainingLoopsCount / _settings.TimedReward.TimedIconDuration;
    }

    private void HideIcon()
    {
        TimersManager.ClearTimer(HideIcon);

        (transform as RectTransform).DOAnchorPosY(-100, 0.5f)
            .OnComplete(() => (transform as RectTransform).DOAnchorPosY(200, 1f));

        TimersManager.SetTimer(this, _settings.TimedReward.TimedIconInterval, ShowIcon);
    }
}
