using UnityEngine;
using System.Collections;
using Pooling;

namespace Enemy.State
{
    public class AIDeadState : AIBaseState
    {
        private float _destroyDelay = 15f;

        public AIDeadState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _stateMachine.Navigation.Stop();

            _stateMachine.StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(_destroyDelay);
            
            if (_stateMachine is IPoolable poolable)
            {
                _stateMachine.OnDespawn();
            }
        }

        public override void Update()
        {
            
        }
    }
}