using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Data.Static.Boxes;
using _Project.Data.Static.FortuneWheel;
using _Project.Data.Static.Skills;
using _Project.IAP;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Purchasing;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "Settings", menuName = "_Project/Settings", order = 0)]
    public class Settings : ScriptableObject
    {
        public int MinPlayers = 6;
        public int MaxPlayers = 12;
        public bool IsWeaponAlwaysInHand = true;
        public int CoinsForWin = 10;
        public int CoinsForKill = 5;
        public int CoinsForRewarded = 100;
        public int NewSaveCoins = 0;
        public int NewSaveCrystals = 0;
        public float DelayAfterAllKilled = 4f;
        public float CharacterDeathHeightBottom = -100.0f;
        public float CharacterDeathHeightTop    = 500.0f;
        public int TutorialBotsCount = 3;
        public LevelData FirstVizitLevel;
        public LevelData TestLevel;
        public AssetReference AFKSceneRef;
        public List<LevelData> Levels = new List<LevelData>();
        
        [NonSerialized]
        public Dictionary<int, WeaponData> Weapons = new Dictionary<int, WeaponData>();
        [NonSerialized]
        public Dictionary<int, Skill> Skills = new Dictionary<int, Skill>();
        [NonSerialized]
        public Dictionary<int, SkinData> Skins = new Dictionary<int, SkinData>();
        
        public Dictionary<Category, Color> CategoryColors = new Dictionary<Category, Color>();
        public Dictionary<SkillShopType, LocalizedString> SkillsShopTypeText = new Dictionary<SkillShopType, LocalizedString>();

        public List<Tuple<BotConfig, int>> BotConfigs = new List<Tuple<BotConfig, int>>();
        public List<Tuple<BotConfig, int>> BotConfigsTutorial = new List<Tuple<BotConfig, int>>();
        
        [SerializeField] private List<BotConfigData> _botConfigs = new List<BotConfigData>();
        [SerializeField] private List<BotConfigData> _botConfigsTutorial = new List<BotConfigData>();
        [SerializeField] private List<Skill> _skillsList = new List<Skill>();
        [SerializeField] private List<WeaponData> _weaponsList = new List<WeaponData>();
        [SerializeField] private List<SkinData> _skinsList = new List<SkinData>();
        [SerializeField] private List<CategoryColor> _categoryColors = new List<CategoryColor>();
        [SerializeField] private List<SkillTypeText> _skillsShopTypeText = new List<SkillTypeText>();

        public BoxSettings[] BoxesSettings;
        public float RewardedCooldown = 2 * 60f;
        public int InterstitialInterval = 2 * 60;
        public int InterstitialDelayAfterRewarded = 2 * 60;
        
        public InAppProductData[] InApps;

        public Material DiscardMaterial;
        public FortuneWheelSettings FortuneWheelSettings;
        public DailyBonusSettings DailyBonus;
        public PlaytimeRewardsSO PlaytimeRewards;
        public TimedRewardSettings TimedReward;

        public void Initialize()
        {
            Weapons = _weaponsList.ToDictionary(e => e.Id, e => e);
            Skills = _skillsList.ToDictionary(e => e.Id, e => e);
            Skins = _skinsList.ToDictionary(e => e.Id, e => e);
            CategoryColors = _categoryColors.ToDictionary(e => e.Category, e => e.Color);
            SkillsShopTypeText = _skillsShopTypeText.ToDictionary(e => e.ShopType, e => e.Text);

            foreach (BotConfigData botConfigData in _botConfigs)
            {
                BotConfigs.Add(Tuple.Create(botConfigData.Config, botConfigData.Weight));
            }
            foreach (BotConfigData botConfigData in _botConfigsTutorial)
            {
                BotConfigsTutorial.Add(Tuple.Create(botConfigData.Config, botConfigData.Weight));
            }
        }
    }
    
    [Serializable]
    public class LevelData
    {
        public string Name;
        public AssetReference SceneRef;
    }

    [Serializable]
    public class BotConfigData
    {
        public BotConfig Config;
        public int Weight;
    }
    
    [Serializable]
    public class WeaponData
    {
        public int Id;
        public LocalizedString Name;
        public int Price;
        public Weapon Prefab;
        public Category Category;
        public WeaponOffsets Offsets;

        [Serializable]
        public class WeaponOffsets
        {
            public Vector3 Offset;
            public Vector3 Rotation;
            public Vector3 IdleOffset;
            public Vector3 IdleRotation;
            public Vector3 PreviewPosition = new Vector3(-190f, -180f, 0f);
            public Vector3 PreviewRotation = new Vector3(45f,90f,0f);
            public Vector3 PreviewScale = new Vector3(420f, 420f, 420f);
            public Vector3 IconPosition = new Vector3(-60f, -60f, 0f);
            public Vector3 IconRotation = new Vector3(45f,90f,0f);
            public Vector3 IconScale = new Vector3(170f, 170f, 170f);
        }
    }

    [Serializable]
    public class SkinData
    {
        public int Id;
        public LocalizedString Name;
        public Skin Prefab;
        public Category Category;
    }

    [Serializable]
    public class InAppProductData
    {
        public IAPProduct Id;
        public ProductType Type;
    }
    
    [Serializable]
    public class CategoryColor
    {
        public Category Category;
        public Color Color;
    }

    [Serializable]
    public class SkillTypeText
    {
        public SkillShopType ShopType;
        public LocalizedString Text;
    }
    
    [Serializable]
    public class CategoryChance
    {
        public Category Category;
        public float Chance;
    }
    
    public enum Category
    {
        Grey = 0,
        Blue,
        Green,
        Red
    }
}