using UnityEngine;

namespace Enemy.State
{
    public abstract class AIBaseState
    {
        protected AIStateMachine _stateMachine;
        protected float _stateEnterTime;

        protected AIBaseState(AIStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public virtual void Enter() 
        { 
            _stateEnterTime = Time.time;
        }
        
        public virtual void Update() { }
        public virtual void Exit() { }
        
        protected float StateDuration => Time.time - _stateEnterTime;
    }
}