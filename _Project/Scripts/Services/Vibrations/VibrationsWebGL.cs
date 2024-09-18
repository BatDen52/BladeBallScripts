using System.Linq;

#if UNITY_WEBGL
namespace _Project
{
    public class VibrationsWebGL : IVibrations
    {
        public void Initialize()
        {
        }

        public void Vibrate(int ms)
        {
            VibrationWebGL.Vibration.Vibrate(ms);
        }

        public void Vibrate(long[] pattern)
        {
            int[] intArray = pattern.Select(x => (int)x).ToArray();
            VibrationWebGL.Vibration.VibratePattern(intArray, pattern.Length);
        }

        public void VibratePop()
        {
            VibrationWebGL.Vibration.Vibrate(50);
        }

        public void VibratePeek()
        {
            VibrationWebGL.Vibration.Vibrate(50);
        }
    }
}
#endif
