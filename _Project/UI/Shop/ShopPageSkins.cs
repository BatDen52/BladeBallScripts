using System.Collections.Generic;
using System.Linq;
using _Project.Data.Persistent;
using _Project.Data.Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.UI
{
    public class ShopPageSkins : MonoBehaviour
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
        [SerializeField] private Button _equipButton;
        [SerializeField] private GameObject _equippedButton;

        private IGameFactory _gameFactory;
        private IPersistentDataService _persistentDataService;
        private PlayerData _playerData;
        private StaticData _staticData;
        private IAnalytics _analytics;
        private List<ShopTile> _shopTiles = new List<ShopTile>();
        private Skin _skinPreview;

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
            IOrderedEnumerable<SkinData> skins = from skin in _staticData.Settings.Skins.Values orderby _playerData.PurchasedSkins.Contains(skin.Id) descending select skin;

            foreach (SkinData skin in skins)
            {
                CreateTile(skin);
            }

            _scrollRect.onValueChanged.AddListener(OnScroll);
        }

        private void OnEnable()
        {
            Timing.RunCoroutine(_HideOutOfBounds3dIcons());

            _analytics.LogEvent(AnalyticsEvents.open_window, 
                (AnalyticsParameters.name, "Skins"));
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
                tile.SelectSkin -= OnSelected;
            }

            _equipButton.onClick.RemoveAllListeners();
        }

        private async void OnSelected(SkinData skin)
        {
            _icon.color = _staticData.Settings.CategoryColors[skin.Category];
            if (_skinPreview != null)
            {
                Destroy(_skinPreview.gameObject);
            }
            _skinPreview = Instantiate(skin.Prefab, _icon.transform);
            _skinPreview.Animator.speed = 0f;
            _skinPreview.transform.localPosition = new Vector3(0, -320, -120);
            _skinPreview.transform.localRotation = Quaternion.Euler(new Vector3(0, 200, 0));
            _skinPreview.transform.localScale = new Vector3(240, 240, 240);
            _skinPreview.gameObject.layer = LayerMask.NameToLayer("UI");
            foreach (Transform child in _skinPreview.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("UI");
            }
            
            _title.text = await skin.Name.GetLocalizedStringAsync2();
            
            if (_playerData.CurrentSkinId != skin.Id)
            {
                _analytics.LogEvent(AnalyticsEvents.skin_select, 
                    (AnalyticsParameters.name, skin.Id.ToString()),
                    (AnalyticsParameters.is_owned, _playerData.PurchasedSkins.Contains(skin.Id) ? 1 : 0));
            }
            
            _equipButton.onClick.RemoveAllListeners();
            _equipButton.onClick.AddListener(() => SetSkin(skin));
            _equipButton.gameObject.SetActive(_playerData.PurchasedSkins.Contains(skin.Id) && _playerData.CurrentWeaponId != skin.Id);
            _equippedButton.SetActive(_playerData.CurrentSkinId == skin.Id);
        }

        private void CreateTile(SkinData skin)
        {
            ShopTile tile;

            if (_playerData.PurchasedSkins.Contains(skin.Id))
            {
                tile = Instantiate(_tilePrefab, _containerOwn);
            }
            else
            {
                _textUnown.gameObject.SetActive(true);
                tile = Instantiate(_tilePrefab, _containerUnown);
            }

            tile.SetSkin(skin, _staticData.Settings.CategoryColors);
            tile.SelectSkin += OnSelected;
            tile.Toggle.group = _toggleGroup;
            if (skin.Id == _playerData.CurrentSkinId)
            {
                OnSelected(skin);
            }
            _shopTiles.Add(tile);
        }

        private void SetSkin(SkinData skin)
        {
            _persistentDataService.PersistentData.PlayerData.CurrentSkinId = skin.Id;
            _gameFactory.Player.SetSkin(skin.Prefab);
            _gameFactory.Player.SetWeapon(_playerData.CurrentWeaponId);
            _persistentDataService.Save();
            OnSelected(skin);
        }
    }
}
