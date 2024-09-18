namespace _Project.StateMachine
{
    public interface IBaseState
    {
        IGameStateMachine GameStateMachine { get; set; }
        void Exit();
    }
}