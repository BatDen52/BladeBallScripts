using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.Data.Static.Skills;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.UI
{
    public class ConfirmMessageWindow : Window
    {
        [SerializeField] private Button _skillButton;
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Image _skillImage;
        [SerializeField] private Image _coinsImage;
        [SerializeField] private TMP_Text _skillText;
        [SerializeField] private TMP_Text _coinsText;

        private IAds _ads;
        private IUIService _uiService;
        private Settings _settings;
        private IPersistentDataService _persistentDataService;
        private PlayerData _playerData;
        private IAudio _audio;
        private IGameFactory _gameFactory;

        [Inject]
        public void Constructor(IObjectResolver container)
        {
            _ads = container.Resolve<IAds>();
            _uiService = container.Resolve<IUIService>();
            _settings = container.Resolve<StaticData>().Settings;
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _playerData = _persistentDataService.PersistentData.PlayerData;
            _audio = container.Resolve<IAudio>();
            _gameFactory = container.Resolve<IGameFactory>();

            InitSkillButton();
            InitCoinsButton();
        }

        private void OnDestroy()
        {
            _skillButton.onClick.RemoveAllListeners();
            _coinsButton.onClick.RemoveAllListeners();
        }

        private async void InitSkillButton()
        {
            List<int> noPurchaserSkills = _settings.Skills
                .Where(i => _playerData.PurchasedSkills.Contains(i.Key) == false)
                .Select(i => i.Key)
                .ToList();

            _skillButton.interactable = noPurchaserSkills.Count > 0;

            int id = noPurchaserSkills.Count > 0 ?
                noPurchaserSkills[Random.Range(0, noPurchaserSkills.Count)] :
                -1;

            Sprite icon = id == -1 ? null : _settings.Skills[id].Icon;
            string rewardText = id == -1 ?
                await _settings.TimedReward.NoSkillsLocale.GetLocalizedStringAsync2() :
                await _settings.Skills[id].Name.GetLocalizedStringAsync2();

            _skillImage.sprite = icon;
            _skillText.text = rewardText;

            _skillButton.onClick.AddListener(() => _ads.ShowRewarded(() => SkillGetReward(id, icon, rewardText)));
            _skillButton.onClick.AddListener(Close);
        }

        private async void InitCoinsButton()
        {
            Sprite icon = _settings.TimedReward.TimedFailIcon;
            string rewardText = string.Format(await _settings.TimedReward.GetCoinsLocale.GetLocalizedStringAsync2(),
                _settings.TimedReward.TimedSkillFailReward);

            _coinsImage.sprite = icon;
            _coinsText.text = "x" + _settings.TimedReward.TimedSkillFailReward;

            _coinsButton.onClick.AddListener(() => _ads.ShowRewarded(() => CoinsGetReward(icon, rewardText)));
            _coinsButton.onClick.AddListener(Close);
        }

        private void SkillGetReward(int id, Sprite icon, string rewardText)
        {
            _playerData.TimedSkills[id] = System.DateTime.Now.AddSeconds(_settings.TimedReward.TimedSkillDuration);

            _persistentDataService.Save();

            ResultWindow window = _uiService.Open(WindowId.RewardedResult) as ResultWindow;
            window.Init(_audio, icon, rewardText, () => SetSkill(id));
        }

        private void CoinsGetReward(Sprite icon, string rewardText)
        {
            _playerData.Coins.Value += _settings.TimedReward.TimedSkillFailReward;

            _persistentDataService.Save();

            ResultWindow window = _uiService.Open(WindowId.RewardedResult) as ResultWindow;
            window.Init(_audio, icon, rewardText);
        }

        private void SetSkill(int id)
        {
            _playerData.CurrentSkillId = id;
            _persistentDataService.Save();
            _gameFactory.Player.SkillActivator.SetSkill(_settings.Skills[id]);
        }
    }
}