using System.Collections;
using _Project.Data.Static;
using _Project.IAP;
using _Project.StateMachine;
using _Project.Timers;
using _Project.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VContainer;
using VContainer.Unity;

namespace _Project
{
    public class ProjectScope : LifetimeScope
    {
        [SerializeField] private StaticDataDependencies _staticDataDependencies;
        [SerializeField] private AudioDependencies _audioDependencies;
        [SerializeField] private UIConfig _uiConfig;
        [SerializeField] private GameCameras _gameCamerasPrefab;
        [SerializeField] private BladeBall _bladeBallPrefab;
        [SerializeField] private Character _characterPrefab;
        [SerializeField] private Bot _botPrefab;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<IAssets, Assets>(Lifetime.Singleton);
            builder.Register<IInput, Input>(Lifetime.Singleton);
            builder.Register<IRandomService, RandomService>(Lifetime.Singleton);
            RegisterPersistentDataService(builder);
            RegisterStaticData(builder);
            RegisterAudio(builder);
            RegisterAdsService(builder);
            RegisterAnalytics(builder);
            RegisterStates(builder);
            RegisterStateMachine(builder);
            builder.RegisterEntryPoint<EntryPoint>();
            RegisterUI(builder);
            builder.RegisterComponent(_bladeBallPrefab);
            builder.RegisterComponent(_characterPrefab);
            builder.RegisterComponent(_botPrefab);
            builder.RegisterComponent(_gameCamerasPrefab);
            builder.Register<ICameraService, CameraService>(Lifetime.Singleton);
            builder.Register<IGameFactory, GameFactory>(Lifetime.Singleton);
            builder.Register<PlaytimeRewards>(Lifetime.Singleton);
            builder.Register<Vibrations>(Lifetime.Singleton);
            builder.RegisterComponentOnNewGameObject<ApplicationFocusHandler>(Lifetime.Scoped, "ApplicationFocusHandler")
                .DontDestroyOnLoad();
            builder.RegisterComponentOnNewGameObject<TimeInvoker>(Lifetime.Scoped, "TimeInvoker")
                .DontDestroyOnLoad();
            RegisterIAP(builder);
            
            builder.RegisterBuildCallback(RegisterCallback);
        }

        private void RegisterCallback(IObjectResolver container)
        {
            if (Application.isEditor == false)
            {
                YandexCallbackReceiver yandexObject = container.Resolve<YandexCallbackReceiver>();
                yandexObject.SetLanguage();
            }
            
        }

        private void RegisterStaticData(IContainerBuilder builder)
        {
            builder.RegisterInstance(_staticDataDependencies.Settings);
            builder.Register<StaticData>(Lifetime.Singleton);
        }

        private void RegisterUI(IContainerBuilder builder)
        {
            builder.RegisterInstance(_uiConfig);
            builder.Register<IUIService, UIService>(Lifetime.Singleton);
        }

        private void RegisterAudio(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_audioDependencies.MusicAudioSource, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_audioDependencies.SoundsAudioSource, Lifetime.Singleton);
            builder.RegisterInstance(_audioDependencies.AudioMixer);
            builder.RegisterInstance(_audioDependencies.Sounds);
            builder.Register<Audio>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterStates(IContainerBuilder builder)
        {
            builder.Register<LoadingState>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<LobbyState>(Lifetime.Singleton);
            builder.Register<GameplayState>(Lifetime.Singleton);
            builder.Register<TutorialState>(Lifetime.Singleton);
        }

        private void RegisterStateMachine(IContainerBuilder builder)
        {
            builder.Register<IGameStateMachine, GameStateMachine>(Lifetime.Singleton);
        }

        private void RegisterAdsService(IContainerBuilder builder)
        {
            if (Application.isEditor)
            {
                builder.Register<IAds, Ads>(Lifetime.Singleton);
            }
            else
            {
                builder.RegisterComponentOnNewGameObject<YandexCallbackReceiver>(Lifetime.Scoped, "YandexCallbackReceiver")
                    .DontDestroyOnLoad();
#if UNITY_WEBGL
                builder.Register<IAds, AdsYandex>(Lifetime.Singleton);
#elif UNITY_ANDROID
                builder.Register<IAds, AdsIronSource>(Lifetime.Singleton);
#endif

            }
            
        }
        
        private void RegisterAnalytics(IContainerBuilder builder)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                builder.Register<IAnalytics, Analytics>(Lifetime.Singleton);
            }
#else
#if UNITY_WEBGL
            builder.Register<IAnalytics, AnalyticsMetrika>(Lifetime.Singleton);
#elif UNITY_ANDROID
            builder.Register<IAnalytics, AnalyticsFirebaseMetrica>(Lifetime.Singleton);
#endif
#endif
        }
        
        private void RegisterIAP(IContainerBuilder builder)
        {
#if UNITY_WEBGL
            builder.Register<IIAPService, IAPServiceYandex>(Lifetime.Scoped);
#elif UNITY_ANDROID
            builder.Register<IIAPService, IAPServiceUnity>(Lifetime.Scoped);
#endif
        }

        private void RegisterPersistentDataService(IContainerBuilder builder)
        {
#if UNITY_WEBGL
            if (Application.isEditor)
            {
                builder.Register<IPersistentDataService, PersistentDataPlayerPrefs>(Lifetime.Singleton);
            }
            else
            {
                builder.Register<IPersistentDataService, PersistentDataYandex>(Lifetime.Singleton);
            }
#elif UNITY_ANDROID
            builder.Register<IPersistentDataService, PersistentDataPlayerPrefs>(Lifetime.Singleton);
#endif
        }
    }
}