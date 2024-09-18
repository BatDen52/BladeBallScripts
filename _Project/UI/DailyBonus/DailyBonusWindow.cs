using UnityEngine;
using System;
using TMPro;
using _Project.Data.Static;
using _Project.UI;
using _Project;
using VContainer;
using _Project.Data.Persistent;

public class DailyBonusWindow : Window
{
    [SerializeField] private DailyBonusCard _bonusPanelPrefab;
    [SerializeField] private Transform _bonusPanelParent;

    private DailyBonusData[] _bonusSchedule;
    private HUD _hud;

    protected override void Initialize()
    {
        base.Initialize();
        
        _bonusSchedule = StaticData.Settings.DailyBonus.BonusSchedule;
        _hud = UIService.HUD;
        UpdateBonusList();
    }

    private void OnDestroy()
    {
        ClearCards();
        _hud.RefreshDailyBonusAlert();
    }

    private void UpdateBonusList()
    {
        ClearCards();

        for (int i = 0; i < _bonusSchedule.Length; i++)
        {
            DailyBonusCard newPanel = Instantiate(_bonusPanelPrefab, _bonusPanelParent);
            newPanel.Init(_bonusSchedule[i], PersistentDataService, StaticData, _hud, Analytics, Vibrations);
        }
    }

    private void ClearCards()
    {
        foreach (Transform child in _bonusPanelParent)
            Destroy(child.gameObject);
    }

    [ContextMenu("NextDay")]
    private void AddDay()
    {
        PlayerData.LastDailyBonusDate = PlayerData.LastDailyBonusDate.AddDays(-1);
        PersistentDataService.Save();
    }
}