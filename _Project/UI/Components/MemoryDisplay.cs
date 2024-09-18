using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

namespace _Project.UI
{
    public class MemoryDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _memoryText;
        [SerializeField] private float _hudRefreshRate = 1f;
        
        private float _timer;
        
        void Update()
        {
            if (Time.unscaledTime > _timer)
            {
                long totalMemory = Profiler.GetTotalAllocatedMemoryLong();
                float totalMemoryMB = (totalMemory / 1024f) / 1024f;
                _memoryText.text = $"Memory: {totalMemoryMB:F1} MB";
                _timer = Time.unscaledTime + _hudRefreshRate;
            }
            
        }
    }
}