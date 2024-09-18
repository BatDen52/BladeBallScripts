using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI
{
    public class ButtonToggle : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject GameObjectEnabled;
        [SerializeField] private GameObject GameObjectDisabled;

        public Button Button => _button;
        
        public void Set(bool value)
        {
            GameObjectEnabled.SetActive(value);
            GameObjectDisabled.SetActive(!value);
        }
        
    }
}