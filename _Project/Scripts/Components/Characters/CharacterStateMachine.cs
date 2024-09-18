namespace _Project
{
    public class CharacterStateMachine : EntityStateMachine<Character>
    {
        protected override void InitStates()
        {
            States[typeof(IdleCharacterState)] = new IdleCharacterState();
            States[typeof(WalkCharacterState)] = new WalkCharacterState();
            States[typeof(FallCharacterState)] = new FallCharacterState();
            States[typeof(DashCharacterState)] = new DashCharacterState();
            States[typeof(ThunderDashCharacterState)] = new ThunderDashCharacterState();
            
            Enter<IdleCharacterState>();
        }
    }
}