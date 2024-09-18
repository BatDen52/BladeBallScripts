using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project
{
    public class SceneScope : LifetimeScope
    {
        [SerializeField] private Camera _levelEditorCamera;
        [SerializeField] private SpawnPoints _spawnPoints;
        [SerializeField] private Lobby _lobby;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            Destroy(_levelEditorCamera.gameObject);
            
            builder.RegisterInstance(_spawnPoints);
            builder.RegisterInstance(_lobby);
        }
    }
}