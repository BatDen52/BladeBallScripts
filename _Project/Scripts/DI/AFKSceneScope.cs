using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project
{
    public class AFKSceneScope : LifetimeScope
    {
        [SerializeField] private Camera _levelEditorCamera;
        [SerializeField] private AFKZone _afkZone;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            Destroy(_levelEditorCamera.gameObject);
            
            builder.RegisterInstance(_afkZone);
        }
    }
}