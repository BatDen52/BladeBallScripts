using System;
using System.Collections.Generic;
using VContainer;

namespace _Project.StateMachine
{
    public class GameStateMachine : IGameStateMachine
    {
        private Dictionary<Type, IBaseState> _states;
        private IBaseState _activeState;

        [Inject]
        public GameStateMachine(
            LoadingState loadingState, 
            GameplayState gameplayState,
            LobbyState lobbyState,
            TutorialState tutorialState)
        {
            _states = new Dictionary<Type, IBaseState>
            {
                [typeof(LoadingState)] = loadingState.With(state => state.GameStateMachine = this),
                [typeof(GameplayState)] = gameplayState.With(state => state.GameStateMachine = this),
                [typeof(LobbyState)] = lobbyState.With(state => state.GameStateMachine = this),
                [typeof(TutorialState)] = tutorialState.With(state => state.GameStateMachine = this),
            };
        }

        public void Enter<TState>() where TState : class, IState
        {
            IState state = ChangeState<TState>();
            state.Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
        {
            TState state = ChangeState<TState>();
            state.Enter(payload);
        }

        private TState ChangeState<TState>() where TState : class, IBaseState
        {
            _activeState?.Exit();
      
            TState state = GetState<TState>();
            _activeState = state;
      
            return state;
        }

        private TState GetState<TState>() where TState : class, IBaseState => 
            _states[typeof(TState)] as TState;

        private void RegisterState<TState>(TState state) where TState : IBaseState =>
            _states.Add(typeof(TState), state);
    }
}