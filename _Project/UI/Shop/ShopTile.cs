using System;
using System.Collections.Generic;
using _Project.Data.Static;
using _Project.Data.Static.Skills;
using Timers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI
{
    public class ShopTile : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _timerIcon;
        [SerializeField] private TMP_Text _timerText;

        private int _timerId = 0;

        [field: SerializeField] public Toggle Toggle { get; private set; }
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        public Skill Skill { get; private set; }
        public WeaponData Weapon { get; private set; }
        public SkinData Skin { get; private set; }

        public event Action<Skill> SelectSkill;
        public event Action<WeaponData> SelectWeapon;
        public event Action<SkinData> SelectSkin;

        private void OnDestroy()
        {
            Toggle?.onValueChanged.RemoveListener(OnValueChanged);

            if (TimersManager.GetTimer(_timerId) != null)
                TimersManager.ClearTimer(_timerId);
        }

        public async void SetSkill(Skill skill)
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

            Skill = skill;
            Toggle.onValueChanged.AddListener(OnValueChanged);
        }

        public async void SetWeapon(WeaponData weapon, Dictionary<Category, Color> categoryColors)
        {
            _icon.color = categoryColors[weapon.Category];
            var preview = Instantiate(weapon.Prefab, _icon.transform);
            preview.transform.localPosition = weapon.Offsets.IconPosition;
            preview.transform.localRotation = Quaternion.Euler(weapon.Offsets.IconRotation);
            preview.transform.localScale = weapon.Offsets.IconScale;
            preview.gameObject.layer = LayerMask.NameToLayer("UI");
            foreach (Transform child in preview.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("UI");
            }
            _title.text = await weapon.Name.GetLocalizedStringAsync2();
            Weapon = weapon;
            Toggle.onValueChanged.AddListener(OnValueChanged);
        }

        public async void SetSkin(SkinData skin, Dictionary<Category, Color> categoryColors)
        {
            _icon.color = categoryColors[skin.Category];
            Skin preview = Instantiate(skin.Prefab, _icon.transform);
            preview.Animator.speed = 0f;
            preview.transform.localPosition = new Vector3(0, -120, -120);
            preview.transform.localRotation = Quaternion.Euler(new Vector3(0, 200, 0));
            preview.transform.localScale = new Vector3(90, 90, 90);
            preview.gameObject.layer = LayerMask.NameToLayer("UI");
            foreach (Transform child in preview.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("UI");
            }
            _title.text = await skin.Name.GetLocalizedStringAsync2();
            Skin = skin;
            Toggle.onValueChanged.AddListener(OnValueChanged);
        }

        public void SetIconActive(bool value)
        {
            _icon.gameObject.SetActive(value);
        }

        public void SetTimer(DateTime dateTime, Action<Skill> onTick)
        {
            _timerIcon.gameObject.SetActive(true);
            _timerText.gameObject.SetActive(true);

            var delta = dateTime - DateTime.Now;
            _timerText.text = $"{delta.Minutes}:{delta.Seconds}";

            _timerId = TimersManager.SetLoopableTimer(this, 1f, () =>
            {
                var delta = dateTime - DateTime.Now;
                _timerText.text = $"{delta.Minutes}:{delta.Seconds:00}";
                onTick?.Invoke(Skill);

                if (delta.TotalSeconds < 0)
                {
                    TimersManager.ClearTimer(_timerId);
                }
            });
        }

        private void OnValueChanged(bool isSelected)
        {
            if (isSelected)
            {
                if (Skill != null) SelectSkill?.Invoke(Skill);
                if (Weapon != null) SelectWeapon?.Invoke(Weapon);
                if (Skin != null) SelectSkin?.Invoke(Skin);
            }
        }
    }
}
