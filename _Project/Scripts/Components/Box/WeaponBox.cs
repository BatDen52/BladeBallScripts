using System.Collections.Generic;
using System.Linq;
using _Project.Data.Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project
{
    public class WeaponBox : RandomBox
    {
        protected List<WeaponData> Items;
        private WeaponData _selectedItem;
        private Weapon _weaponPreview;

        public override void Init()
        {
            Items = StaticData.Settings.Weapons.Values.ToList();

            if (Settings.IncludeOnlyNotPurchased)
            {
                Items = Items.Where(i => PlayerData.PurchasedWeapons.Contains(i.Id) == false).ToList();
            }

            Items = Items.Where(i => Settings.CategoriesChances.First(j => j.Category == i.Category).Chance > 0)
                .OrderBy(i => Random.Range(int.MinValue, int.MaxValue)).ToList();
        }

        public override bool UnlockItem(int id)
        {
            if (id == -1 || PlayerData.PurchasedWeapons.Contains(id))
                return false;

            PlayerData.PurchasedWeapons.Add(id);
            PersistentDataService.Save();

            return true;
        }

        public override int GetRandomItemId()
        {
            float sumChancesNotEmptyCategory = Settings.CategoriesChances.Where(i => i.Chance > 0 &&
                Items.Any(j => j.Category == i.Category))
                .Sum(i => i.Chance);

            // Debug.Log($"sumChancesNotEmptyCategory - {sumChancesNotEmptyCategory}");

            if (sumChancesNotEmptyCategory == 0)
                return -1;

            float categoryRandomValue = Random.Range(0, sumChancesNotEmptyCategory);
            float sum = 0;

            Category selectedCategory = Category.Grey;

            foreach (CategoryChance categoryChance in Settings.CategoriesChances)
            {
                if (categoryChance.Chance > 0 && Items.Any(i => i.Category == categoryChance.Category))
                {
                    sum += categoryChance.Chance;
                }

                if (sum >= categoryRandomValue)
                {
                    selectedCategory = categoryChance.Category;
                    break;
                }
            }

            List<WeaponData> itemsOfCategory = Items.Where(i => i.Category == selectedCategory).ToList();

            _selectedItem = itemsOfCategory[Random.Range(0, itemsOfCategory.Count)];

            if (Settings.IncludeOnlyNotPurchased)
                Items.Remove(_selectedItem);

            return _selectedItem.Id;
        }

        public async override void ShowItem(Image image, TMP_Text text, string textAddon = "")
        {
            if (_weaponPreview != null)
            {
                Destroy(_weaponPreview.gameObject);
            }

            _weaponPreview = Instantiate(_selectedItem.Prefab, image.transform);
            _weaponPreview.transform.localPosition = _selectedItem.Offsets.PreviewPosition;
            _weaponPreview.transform.localRotation = Quaternion.Euler(_selectedItem.Offsets.PreviewRotation);
            _weaponPreview.transform.localScale = _selectedItem.Offsets.PreviewScale;
            _weaponPreview.gameObject.layer = LayerMask.NameToLayer("UI");

            foreach (Transform child in _weaponPreview.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("UI");
            }

            // image.color = new Color(255, 255, 255, 0);
            image.enabled = true;
            image.color = StaticData.Settings.CategoryColors[_selectedItem.Category];
            text.text = await _selectedItem.Name.GetLocalizedStringAsync2() + textAddon;
        }

        public override bool HasItemForGeneration()
        {
            return Settings.CategoriesChances.Any(i => i.Chance > 0 && Items.Any(j => j.Category == i.Category
                && PlayerData.PurchasedWeapons.Contains(j.Id) == false));
        }
    }
}