using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI
{
    public class WindowShop : Window
    {
        [SerializeField] private Toggle _firstPageSelector;

        private void OnEnable()
        {
            _firstPageSelector.isOn = true;
            _firstPageSelector.Select();
        }
    }
}