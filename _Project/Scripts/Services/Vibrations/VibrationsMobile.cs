#if UNITY_ANDROID || UNITY_IOS
namespace _Project
{
    public class VibrationsMobile : IVibrations
    {
        public void Initialize()
        {
            Vibration.Init();
        }

        public void Vibrate(int ms)
        {
            Vibration.Vibrate(ms);
        }

        public void Vibrate(long[] pattern)
        {
            Vibration.Vibrate(pattern, -1);
        }

        public void VibratePop()
        {
            Vibration.VibratePop();
        }

        public void VibratePeek()
        {
            Vibration.VibratePeek();
        }
    }
}
#endif