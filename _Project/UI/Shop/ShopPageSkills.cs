using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.Data.Static.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.UI
{
    public class ShopPageSkills : MonoBehaviour
    {
        [SerializeField] private ShopTile _tilePrefab;
        [SerializeField] private Transform _containerOwn;
        [SerializeField] private Transform _containerUnown;
        [SerializeField] private TMP_Text _textUnown;
        [SerializeField] private ToggleGroup _toggleGroup;

        [Header("Description")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _buyButtonText;
        [SerializeField] private GameObject _equippedButton;
        [SerializeField] private Button _upgradeButton;

        private IGameFactory _gameFactory;
        private IPersistentDataService _persistentDataService;
        private PlayerData _playerData;
        private StaticData _staticData;
        private IAnalytics _analytics;
        private List<ShopTile> _shopTiles = new List<ShopTile>();

        [Inject]
        public void Construct(IObjectResolver container)
        {
            _gameFactory = container.Resolve<IGameFactory>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _playerData = _persistentDataService.PersistentData.PlayerData;
            _staticData = container.Resolve<StaticData>();
            _analytics = container.Resolve<IAnalytics>();
        }

        private void Start()
        {
            IOrderedEnumerable<Skill> skills = from skill in _staticData.Settings.Skills.Values orderby _playerData.PurchasedSkills.Contains(skill.Id) descending, skill.Price ascending select skill;

            foreach (Skill skill in skills)
            {
                CreateTile(skill);
            }
        }

        private void OnDestroy()
        {
            foreach (ShopTile tile in _shopTiles)
            {
                tile.SelectSkill -= OnSelected;
            }

            _equipButton.onClick.RemoveAllListeners();
        }

        private async void OnSelected(Skill skill)
        {
            _icon.sprite = skill.Icon;

            if (!skill.Name.IsEmpty)
            {
                _title.text = await skill.Name.GetLocalizedStringAsync2();
            }
            else
            {
                _title.text = "[" + skill.name + "]";
            }

            if (!skill.Description.IsEmpty)
            {
                _description.text = await skill.Description.GetLocalizedStringAsync2();
            }
            else
            {
                _description.text = "[ Here should be localized description of skill " + skill.name + "]";
            }

            if (_playerData.CurrentSkillId != skill.Id)
            {
                _analytics.LogEvent(AnalyticsEvents.skill_select,
                    (AnalyticsParameters.name, skill.Title),
                    (AnalyticsParameters.is_owned, _playerData.PurchasedSkills.Contains(skill.Id) ? 1 : 0));
            }

            _equipButton.onClick.RemoveAllListeners();
            _equipButton.onClick.AddListener(() => EquipSkill(skill));
            _equipButton.gameObject.SetActive((_playerData.PurchasedSkills.Contains(skill.Id) || _playerData.TimedSkills.ContainsKey(skill.Id))
                && _playerData.CurrentSkillId != skill.Id);

            switch (skill.ShopType)
            {
                case SkillShopType.FortuneWheel:
                    _buyButtonText.SetText(await _staticData.Settings.SkillsShopTypeText[skill.ShopType].GetLocalizedStringAsync2());
                    break;
                case SkillShopType.Purchasable:
                default:
                    _buyButtonText.SetText(skill.Price.ToString());
                    break;
            }

            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(() => BuySkill(skill));
            _buyButton.gameObject.SetActive(_playerData.PurchasedSkills.Contains(skill.Id) == false);
            _equippedButton.SetActive(_playerData.CurrentSkillId == skill.Id);
            _buyButton.interactable = skill.Price <= _playerData.Coins.Value && skill.ShopType == SkillShopType.Purchasable;

            //_upgradeButton;
        }

        private void CreateTile(Skill skill)
        {
            ShopTile tile;

            if (_playerData.PurchasedSkills.Contains(skill.Id))
            {
                tile = Instantiate(_tilePrefab, _containerOwn);
            }
            else if (_playerData.TimedSkills.ContainsKey(skill.Id))
            {
                tile = Instantiate(_tilePrefab, _containerOwn);
                tile.transform.SetAsFirstSibling();
                tile.SetTimer(_playerData.TimedSkills[skill.Id], TryRemoveTimedSkill);
            }
            else
            {
                _textUnown.gameObject.SetActive(true);
                tile = Instantiate(_tilePrefab, _containerUnown);
            }

            tile.SetSkill(skill);
            tile.SelectSkill += OnSelected;
            tile.Toggle.group = _toggleGroup;

            if (skill.Id == _playerData.CurrentSkillId)
            {
                OnSelected(skill);
            }

            _shopTiles.Add(tile);
        }

        private void TryRemoveTimedSkill(Skill skill)
        {
            if (_playerData.TimedSkills.ContainsKey(skill.Id) && _playerData.TimedSkills[skill.Id] >= DateTime.Now)
                return;

            if (_playerData.CurrentSkillId == skill.Id)
                EquipSkill(_staticData.Settings.Skills[_playerData.PurchasedSkills[_playerData.PurchasedSkills.Count - 1]]);

            var tile = _shopTiles.First(i => i.Skill.Id == skill.Id);
            Destroy(tile.gameObject);
            _shopTiles.Remove(tile);
            CreateTile(skill);
        }

        private void BuySkill(Skill skill)
        {
            if (skill.Price > _playerData.Coins.Value)
            {
                return;
            }

            _playerData.Coins.Value -= skill.Price;
            _playerData.PurchasedSkills.Add(skill.Id);
            _playerData.CurrencySpentCount++;
            _persistentDataService.Save();
            LogAnalyticsEvent(skill);
            _equipButton.gameObject.SetActive(true);
            _buyButton.gameObject.SetActive(false);

            var tile = _shopTiles.First(i => i.Skill.Id == skill.Id);
            Destroy(tile.gameObject);
            _shopTiles.Remove(tile);
            CreateTile(skill);
        }

        private void LogAnalyticsEvent(Skill skill)
        {
            _analytics.LogEvent(AnalyticsEvents.currency_spent,
                (AnalyticsParameters.currency, "coins"),
                (AnalyticsParameters.type, "buy_skill"),
                (AnalyticsParameters.item, skill.Title),
                (AnalyticsParameters.price, skill.Price),
                (AnalyticsParameters.count, _playerData.CurrencySpentCount)
                );
            _analytics.LogEvent(AnalyticsEvents.skill_buy,
                (AnalyticsParameters.name, skill.Title));
        }

        private void EquipSkill(Skill skill)
        {
            _persistentDataService.PersistentData.PlayerData.CurrentSkillId = skill.Id;
            _persistentDataService.Save();
            _gameFactory.Player.SkillActivator.SetSkill(
                _staticData.Settings.Skills[_persistentDataService.PersistentData.PlayerData.CurrentSkillId]);
            OnSelected(skill);
        }
    }
}
