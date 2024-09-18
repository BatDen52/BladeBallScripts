using _Project.StateMachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project
{
    public class EntryPoint : IInitializable
    {
        private readonly IGameStateMachine _gameStateMachine;
        
        [Inject]
        public EntryPoint(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public void Initialize()
        {
            Application.targetFrameRate = 30;
            _gameStateMachine.Enter<LoadingState>();
        }
    }
}