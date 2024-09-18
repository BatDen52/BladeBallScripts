using UnityEngine;

namespace _Project
{
    public abstract class EntityState<T> where T : Entity<T>
    {
        protected float TimeSinceEntered { get; private set; }

        public void OnEnter(T entity)
        {
            TimeSinceEntered = 0;
            Enter(entity);
        }
        public void OnTick(T entity)
        {
            Tick(entity);
            TimeSinceEntered += Time.deltaTime;
        }
        public abstract void Enter(T entity);
        public abstract void Exit(T entity);
        public abstract void Tick(T entity);
    }
}