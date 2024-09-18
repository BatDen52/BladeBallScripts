using _Project.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using System;

namespace _Project.Data.Static.Boxes
{
    [CreateAssetMenu(menuName = "Boxes/BoxSettings")]
    public class BoxSettings : ScriptableObject
    {
        public BoxContentType BoxContentType = BoxContentType.Weapon;
        public TypeBox TypeBox = TypeBox.Simple;
        public int OpenPrice = 80;
        public LocalizedString BtnOpenLocale;
        public int FailCashBack = 50;
        public LocalizedString FailLocale;
        public Sprite FailSprite;
        public WindowId BoxWindow = WindowId.Box;
        public List<CategoryChance> CategoriesChances = new List<CategoryChance>();
        public bool IncludeOnlyNotPurchased = false;
        public float NeedPressTime = 1f;
        public int ClickCount = 3;
        public float ShakeForce = 15;

        private void OnValidate()
        {
            Category[] categories = (Category[])Enum.GetValues(typeof(Category));

            if (categories.Length > CategoriesChances.Count)
            {
                for (int i = CategoriesChances.Count; i < categories.Length; i++)
                    CategoriesChances.Add(new CategoryChance());
            }
            else if (categories.Length < CategoriesChances.Count)
            {
                for (int i = CategoriesChances.Count - 1; i >= categories.Length; i--)
                    CategoriesChances.RemoveAt(i);
            }

            for (int i = 0; i < categories.Length; i++)
            {
                if (CategoriesChances[i].Category != categories[i])
                    CategoriesChances[i].Category = categories[i];

                CategoriesChances[i].Chance = Mathf.Clamp(CategoriesChances[i].Chance, 0, 100);
            }
        }
    }

    public enum BoxContentType
    {
        Weapon,
        Skin
    }

    public enum TypeBox
    {
        Simple,
        Premium
    }
}
