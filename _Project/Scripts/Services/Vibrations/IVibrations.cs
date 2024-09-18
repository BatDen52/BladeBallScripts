namespace _Project
{
    public interface IVibrations
    {
        void Initialize();
        void Vibrate(int ms); // not working on IOS
        void Vibrate(long[] pattern); // not working on IOS
        void VibratePop();
        void VibratePeek(); }
}