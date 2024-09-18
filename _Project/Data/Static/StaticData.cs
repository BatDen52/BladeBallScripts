using VContainer;

namespace _Project.Data.Static
{
    public class StaticData
    {
        public Settings Settings;

        [Inject]
        public StaticData(Settings settings)
        {
            Settings = settings;
        }
    }
}