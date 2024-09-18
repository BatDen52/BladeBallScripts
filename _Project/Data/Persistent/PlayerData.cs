using System;
using System.Collections.Generic;
using UniRx;

namespace _Project.Data.Persistent
{
    [Serializable]
    public class PlayerData
    {
        public IntReactiveProperty Coins = new IntReactiveProperty();
        public IntReactiveProperty Crystals = new IntReactiveProperty();
        public int LastRoundReward;
        public string LastRoundMode;
        public int CurrentWeaponId;
        public int CurrentSkillId;
        public int CurrentSkinId;
        public List<int> PurchasedWeapons = new List<int>(){0};
        public List<int> PurchasedSkills = new List<int>(){0};
        public List<int> PurchasedSkins = new List<int>(){0};
        public List<int> PlaytimeRewardsClaimed = new List<int>();
        public DateTime FirstSessionDateTime;
        public DateTime LastInterstitialDateTime;
        public DateTime LastRewardedDateTime;
        public int SessionNumber;
        public IntReactiveProperty TotalPlaytime = new IntReactiveProperty();
        public int TotalPlaytimeMinutes;
        public IntReactiveProperty CurrentSessionPlaytime = new IntReactiveProperty();
        public IntReactiveProperty CurrentDatePlaytime = new IntReactiveProperty();
        public int CurrencySpentCount;
        public int CurrencyGainedCount; 
        public int RoundsCount;
        public string CurrentScene = "none";
        public bool IsTutorial = true;
        public int StartRoundCount;
        public int StartRoundDeathmatch;
        public int StartRoundDuel_1_1;
        public int VictoriesCount;
        public int VictoriesDeathmatchCount;
        public int VictoriesDuelCount;
        public int DeathCount;
        public int DeathCountClassic;
        public int DeathCountDuel_1_1;
        public int KillsCount;
        public int OpenWeaponChestCount;
        public int OpenPremiumWeaponChestCount;
        public int ShopRewardedCount;
        
        public Dictionary<string, int> FreeBoxOpenings = new Dictionary<string, int>();
        
        public bool IsFirstBlock = true;
        public bool IsFirstOpenShop = true;
        public bool IsFirstOpenHelp = true;
        public bool IsFirstOpenWeapons = true;
        public bool IsFirstUseSkill = true;

        public DateTime LastLoginDate = DateTime.MinValue;
        public DateTime LastDailyBonusDate = DateTime.MinValue;
        public int LastDailyBonusDay = 0;

        //wheel
        public int FreeSpinsCount;
        public string LastRewardedSpinTime = string.Empty;
        public DateTime FastSpinEnd = DateTime.MinValue;
        public DateTime CoinsX2End = DateTime.MinValue;

        public bool HasFastSpin => DateTime.Now < FastSpinEnd;
        public bool HasCoinsX2 => DateTime.Now < CoinsX2End;

        //Timed
        public Dictionary<int, DateTime> TimedSkills = new();

        //Проверить, не отключается ли в бою?
    }
}