using VContainer;

namespace _Project
{
    public class Vibrations
    {
        public VibrationPatterns Patterns { get; private set; }

        public class VibrationPatterns
        {
            public readonly long[] Reload = { 0, 50, 50, 50 };
            public readonly int Death = 200;
        }

        private readonly IPersistentDataService _persistentDataService;

        private IVibrations _implementation;

        private bool IsEnabled => _persistentDataService.PersistentData.VibrationsEnabled.Value;

        [Inject]
        public Vibrations(IPersistentDataService persistentDataService)
        {
            Patterns = new VibrationPatterns();
            _persistentDataService = persistentDataService;
        }

        public void Initialize()
        {
#if UNITY_EDITOR
            _implementation = new VibrationsEditor();
#elif UNITY_WEBGL
            _implementation = new VibrationsWebGL();
#elif UNITY_ANDROID || UNITY_IOS
            _implementation = new VibrationsMobile();
#endif
            _implementation.Initialize();
        }

        public void Vibrate(int ms) // not working on IOS
        {
            if (IsEnabled == false)
            {
                return;
            }
            
            _implementation.Vibrate(ms);
        }
        
        public void Vibrate(long[] pattern) // not working on IOS
        {
            if (IsEnabled == false)
            {
                return;
            }
            
            _implementation.Vibrate(pattern);
        }

        public void VibratePop()
        {
            if (IsEnabled == false)
            {
                return;
            }
            
            _implementation.VibratePop();
        }

        public void VibratePeek()
        {
            if (IsEnabled == false)
            {
                return;
            }
            
            _implementation.VibratePeek();
        }
    }
}