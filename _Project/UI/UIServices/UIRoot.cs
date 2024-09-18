using UnityEngine;

namespace _Project.UI
{
    public class UIRoot : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        public void SetCanvasCamera(Camera camera)
        {
            _canvas.worldCamera = camera;
        }
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}