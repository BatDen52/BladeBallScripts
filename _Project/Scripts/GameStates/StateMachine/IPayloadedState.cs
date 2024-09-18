namespace _Project.StateMachine
{
    public interface IPayloadedState<TPayload> : IBaseState
    {
        void Enter(TPayload payload);
    }
}