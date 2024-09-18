using _Project.Data.Static;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace _Project.UI
{
    public class PlaytimeRewardView : MonoBehaviour
    {
        public int Timing;
        
        [SerializeField] private LocalizeStringEvent _timeText;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private GameObject _frame;
        [SerializeField] private GameObject _frameReady;
        [SerializeField] private GameObject _frameClaimed;
        [SerializeField] private Button _buttonClaim;
        [SerializeField] private GameObject _textClaimed;

        private PlaytimeRewards _playtimeRewards;
        
        public async void Fill(PlaytimeRewardSO reward, int timing, PlaytimeRewards playtimeRewards)
        {
            _playtimeRewards = playtimeRewards;
            _buttonClaim.onClick.AddListener(OnClaim);
            Timing = timing;
            _frame.SetActive(true);
            _frameReady.SetActive(false);
            _frameClaimed.SetActive(false);
            _buttonClaim.gameObject.SetActive(false);
            _textClaimed.SetActive(false);
            _icon.sprite = reward.Icon;
            _nameText.text = await reward.Name.GetLocalizedStringAsync2();
            _timeText.UpdateArgument(timing / 60);
        }

        private void OnClaim()
        {
            _playtimeRewards.Claim(Timing);
        }

        public void SetReady()
        {
            _frame.SetActive(false);
            _frameReady.SetActive(true);
            _buttonClaim.gameObject.SetActive(true);
        }

        public void SetClaimed()
        {
            _frame.SetActive(false);
            _buttonClaim.gameObject.SetActive(false);
            _frameClaimed.SetActive(true);
            _textClaimed.SetActive(true);
        }
        
        private void OnDestroy()
        {
            _buttonClaim.onClick.RemoveAllListeners();
        }
    }
}