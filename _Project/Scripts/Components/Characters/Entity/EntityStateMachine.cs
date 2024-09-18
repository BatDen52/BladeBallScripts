using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project
{
    public abstract class EntityStateMachine<T> : MonoBehaviour where T : Entity<T>
    {
        public EntityState<T> ActiveState { get; private set; }
        public T Entity { get; private set; }
        
        protected Dictionary<Type, EntityState<T>> States = new Dictionary<Type, EntityState<T>>();

        public void Enter<TState>() where TState : EntityState<T>
        {
            ActiveState?.Exit(Entity);
            ActiveState = States[typeof(TState)];
            ActiveState.OnEnter(Entity);
        }

        public void Tick()
        {
            ActiveState?.OnTick(Entity);
        }

        private void Awake()
        {
            InitEntity();
            InitStates();
        }

        protected abstract void InitStates();

        private void InitEntity()
        {
            Entity = GetComponent<T>();
        }
    }
}