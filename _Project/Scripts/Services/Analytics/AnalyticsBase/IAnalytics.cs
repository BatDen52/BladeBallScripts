namespace _Project
{
    public interface IAnalytics
    {
        void Initialize();
        void LogEvent(string name, params (string Key, object Value)[] parameters);
    }
}