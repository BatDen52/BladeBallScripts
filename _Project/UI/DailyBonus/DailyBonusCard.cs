using _Project;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class DailyBonusCard : MonoBehaviour
{
    [SerializeField] private GameObject _blockPanel;
    [SerializeField] private TMP_Text _blockPanelText;
    [SerializeField] private TMP_Text _dayText;
    [SerializeField] private Image _rewardImage;
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private Button _getButton;
    [SerializeField] private Image _borderImage;

    private DailyBonusData _bonusData;
    private IPersistentDataService _persistentDataService;
    private PlayerData _playerData;
    private StaticData _staticData;
    private IAnalytics _analytics;
    private HUD _hud;
    protected Vibrations _vibrations;

    public void Init(DailyBonusData bonusData, IPersistentDataService persistentDataService, StaticData staticData,
        HUD hud, IAnalytics analytics, Vibrations vibrations)
    {
        _bonusData = bonusData;
        _persistentDataService = persistentDataService;
        _playerData = persistentDataService.PersistentData.PlayerData;
        _staticData = staticData;
        _analytics = analytics;
        _hud = hud;
        _vibrations = vibrations;

        _ = FillData(bonusData);
    }

    private void OnDestroy()
    {
        _getButton.onClick.RemoveAllListeners();
    }

    private async Task FillData(DailyBonusData bonusData)
    {
        _dayText.text = string.Format(await bonusData.DayLocale.GetLocalizedStringAsync2(), bonusData.DayNumber);
        _rewardText.text = await GetDescription(bonusData);
        _rewardImage.sprite = bonusData.Icon;

        int deltaDay = (DateTime.Now.Date - _playerData.LastDailyBonusDate).Days;

        if (bonusData.DayNumber <= _playerData.LastDailyBonusDay)
        {
            await BlockGetting(bonusData);
        }
        else if (bonusData.DayNumber == _playerData.LastDailyBonusDay + 1 && deltaDay >= 1)
        {
            transform.localScale = Vector3.one * 1.1f;
            _blockPanel.SetActive(false);
            _getButton.onClick.AddListener(TryGetReward);
            _borderImage.gameObject.SetActive(true);
        }
        else
        {
            _blockPanel.SetActive(false);
            _getButton.gameObject.SetActive(false);
        }
    }

    private async Task BlockGetting(DailyBonusData bonusData)
    {
        _blockPanel.SetActive(true);
        _blockPanelText.text = await bonusData.ReceivedLocale.GetLocalizedStringAsync2();
        _getButton.gameObject.SetActive(false);
    }

    private async Task<string> GetDescription(DailyBonusData bonusData)
    {
        switch (bonusData.Type)
        {
            case RewardType.Coin:
                return string.Format(await bonusData.RewardLocale.GetLocalizedStringAsync2(), bonusData.Amount);
            case RewardType.Sword:
                return string.Format(await bonusData.RewardLocale.GetLocalizedStringAsync2(), await _staticData.Settings.Weapons[bonusData.Amount].Name.GetLocalizedStringAsync2());
            default:
                return string.Empty;
        }
    }

    private void TryGetReward()
    {
        switch (_bonusData.Type)
        {
            case RewardType.Coin:
                _playerData.Coins.Value += _bonusData.Amount;
                _playerData.CurrencyGainedCount++;
                _analytics.LogEvent(AnalyticsEvents.currency_gained,
                    (AnalyticsParameters.currency, "coins"),
                    (AnalyticsParameters.source, "daily_bonus"),
                    (AnalyticsParameters.amount, _staticData.Settings.CoinsForRewarded),
                    (AnalyticsParameters.count, _playerData.CurrencyGainedCount)
                );
                break;
            case RewardType.Sword:
                _playerData.PurchasedWeapons.Add(_bonusData.Amount);
                break;
        }

        _vibrations.VibratePop();
        
        _playerData.LastDailyBonusDay = _bonusData.DayNumber;
        _playerData.LastDailyBonusDate = DateTime.Now.Date;

        _persistentDataService.Save();

        _ = BlockGetting(_bonusData);
        _hud.RefreshDailyBonusAlert();
    }
}
