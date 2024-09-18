using _Project.Data.Persistent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusPanel : MonoBehaviour
{
    [SerializeField] private BonusView _fastSpinBonus;
    [SerializeField] private BonusView _x2Bonus;

    private PlayerData _playerData;

    public void Init(PlayerData playerData)
    {
        _playerData = playerData;
        _fastSpinBonus.Init(playerData.FastSpinEnd);
        _x2Bonus.Init(playerData.CoinsX2End);
    }
}
