using UnityEngine;

namespace _Project
{
    public class VibrationsEditor : IVibrations
    {
        public void Initialize()
        {
            Debug.Log("Vibrations initialized");
        }

        public void Vibrate(int ms)
        {
            Debug.Log("Vibrate " + ms);
        }

        public void Vibrate(long[] pattern)
        {
            Debug.Log("Vibrate " + pattern);
        }

        public void VibratePop()
        {
            Debug.Log("VibratePop");
        }

        public void VibratePeek()
        {
            Debug.Log("VibratePeek");
        }
    }
}