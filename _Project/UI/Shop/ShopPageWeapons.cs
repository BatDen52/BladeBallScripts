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
    public class ShopPageWeapons : MonoBehaviour
    {
        [SerializeField] private ShopTile _tilePrefab;
        [SerializeField] private Transform _containerOwn;
        [SerializeField] private Transform _containerUnown;
        [SerializeField] private TMP_Text _textUnown;
        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewport;

        [Header("Description")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _buyButtonText;
        [SerializeField] private GameObject _equippedButton;

        private IGameFactory _gameFactory;
        private IPersistentDataService _persistentDataService;
        private PlayerData _playerData;
        private StaticData _staticData;
        private IAnalytics _analytics;
        private List<ShopTile> _shopTiles = new List<ShopTile>();
        private Weapon _weaponPreview;

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
            IOrderedEnumerable<WeaponData> weapons = from weapon in _staticData.Settings.Weapons.Values orderby _playerData.PurchasedWeapons.Contains(weapon.Id) descending, weapon.Price ascending select weapon;

            foreach (WeaponData weapon in weapons)
            {
                CreateTile(weapon);
            }

            _scrollRect.onValueChanged.AddListener(OnScroll);
        }

        private void OnEnable()
        {
            Timing.RunCoroutine(_HideOutOfBounds3dIcons());

            _analytics.LogEvent(AnalyticsEvents.open_window, 
                (AnalyticsParameters.name, "Weapons"));
        }

        private IEnumerator<float> _HideOutOfBounds3dIcons()
        {
            yield return Timing.WaitForOneFrame;
            HideOutOfBounds3dIcons();
        }

        private void OnScroll(Vector2 value)
        {
            HideOutOfBounds3dIcons();
        }

        private void HideOutOfBounds3dIcons()
        {
            foreach (ShopTile shopTile in _shopTiles)
            {
                Vector3 position = _viewport.InverseTransformPoint(shopTile.RectTransform.position);
                shopTile.SetIconActive(_viewport.rect.Contains(position));
            }
        }

        private void OnDestroy()
        {
            foreach (ShopTile tile in _shopTiles)
            {
                tile.SelectWeapon -= OnSelected;
            }

            _equipButton.onClick.RemoveAllListeners();
        }

        private async void OnSelected(WeaponData weapon)
        {
            _icon.color = _staticData.Settings.CategoryColors[weapon.Category];
            if (_weaponPreview != null)
            {
                Destroy(_weaponPreview.gameObject);
            }
            _weaponPreview = Instantiate(weapon.Prefab, _icon.transform);
            _weaponPreview.transform.localPosition = weapon.Offsets.PreviewPosition;
            _weaponPreview.transform.localRotation = Quaternion.Euler(weapon.Offsets.PreviewRotation);
            _weaponPreview.transform.localScale = weapon.Offsets.PreviewScale;
            _weaponPreview.gameObject.layer = LayerMask.NameToLayer("UI");
            foreach (Transform child in _weaponPreview.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("UI");
            }

            _title.text = await weapon.Name.GetLocalizedStringAsync2();
            // _description.text = await weapon.Description.GetLocalizedStringAsync2();

            if (_playerData.CurrentWeaponId != weapon.Id)
            {
                _analytics.LogEvent(AnalyticsEvents.weapon_select, 
                    (AnalyticsParameters.name, weapon.Id.ToString()),
                    (AnalyticsParameters.is_owned, _playerData.PurchasedWeapons.Contains(weapon.Id) ? 1 : 0));
            }
            
            _equipButton.onClick.RemoveAllListeners();
            _equipButton.onClick.AddListener(() => EquipWeapon(weapon));
            _equipButton.gameObject.SetActive(_playerData.PurchasedWeapons.Contains(weapon.Id) && _playerData.CurrentWeaponId != weapon.Id);
            _buyButton.gameObject.SetActive(false);
            // _buyButtonText.SetText(weapon.Price.ToString());
            // _buyButton.onClick.RemoveAllListeners();
            // _buyButton.onClick.AddListener(() => BuyWeapon(weapon));
            // _buyButton.gameObject.SetActive(_playerData.PurchasedWeapons.Contains(weapon.Id) == false);
            // _buyButton.interactable = weapon.Price <= _playerData.Coins.Value;
            _equippedButton.SetActive(_playerData.CurrentWeaponId == weapon.Id);
        }

        private void CreateTile(WeaponData weapon)
        {
            ShopTile tile;

            if (_playerData.PurchasedWeapons.Contains(weapon.Id))
            {
                tile = Instantiate(_tilePrefab, _containerOwn);
            }
            else
            {
                _textUnown.gameObject.SetActive(true);
                tile = Instantiate(_tilePrefab, _containerUnown);
            }

            tile.SetWeapon(weapon, _staticData.Settings.CategoryColors);
            tile.SelectWeapon += OnSelected;
            tile.Toggle.group = _toggleGroup;
            if (weapon.Id == _playerData.CurrentWeaponId)
            {
                OnSelected(weapon);
            }
            _shopTiles.Add(tile);
        }

        private void BuyWeapon(WeaponData weapon)
        {
            if (weapon.Price > _playerData.Coins.Value)
            {
                return;
            }

            _playerData.Coins.Value -= weapon.Price;
            _playerData.PurchasedWeapons.Add(weapon.Id);
            _persistentDataService.Save();
            _equipButton.gameObject.SetActive(true);
            _buyButton.gameObject.SetActive(false);

            Destroy(_shopTiles.First(i => i.Weapon.Id == weapon.Id).gameObject);
            CreateTile(weapon);
        }

        private void EquipWeapon(WeaponData weapon)
        {
            _persistentDataService.PersistentData.PlayerData.CurrentWeaponId = weapon.Id;
            _gameFactory.Player.SetWeapon(weapon.Id);
            _persistentDataService.Save();
            OnSelected(weapon);
        }
    }
}
