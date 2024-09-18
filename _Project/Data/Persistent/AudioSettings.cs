using System;
using UniRx;

namespace _Project.Data.Persistent
{
    [Serializable]
    public class AudioSettings
    {
        public BoolReactiveProperty IsMusicMuted = new BoolReactiveProperty();
        public BoolReactiveProperty IsSoundsMuted = new BoolReactiveProperty();
    }
}