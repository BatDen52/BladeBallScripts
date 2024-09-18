using _Project.Data.Persistent;
using System;
using _Project.Data.Static;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.UI
{
    public abstract class Window : MonoBehaviour
    {
        public Button ButtonClose;

        protected IUIService UIService;
        protected StaticData StaticData;
        protected Settings Settings => StaticData.Settings;
        protected IPersistentDataService PersistentDataService;
        protected IAnalytics Analytics;
        protected Vibrations Vibrations;
        protected PersistentData PersistentData => PersistentDataService.PersistentData;
        protected PlayerData PlayerData => PersistentDataService.PersistentData.PlayerData;
        protected CompositeDisposable Subscribes = new CompositeDisposable();

        public event Action Closing;

        [Inject]
        protected virtual void Construct(IObjectResolver container)
        {
            UIService = container.Resolve<IUIService>();
            PersistentDataService = container.Resolve<IPersistentDataService>();
            Analytics = container.Resolve<IAnalytics>();
            StaticData = container.Resolve<StaticData>();
            Vibrations = container.Resolve<Vibrations>();
        }

        private void Start()
        {
            Initialize();
            Subscribe();
        }

        protected void OnDestroy()
        {
            Closing?.Invoke();
            Cleanup();
        }

        protected virtual void Initialize(){}

        protected virtual void Subscribe()
        {
            ButtonClose.onClick.AddListener(Close);
        }

        public void Close()
        {
            UIService.Close(this);
        }
        
        protected virtual void Cleanup()
        {
            Subscribes.Clear();
        }
    }
}