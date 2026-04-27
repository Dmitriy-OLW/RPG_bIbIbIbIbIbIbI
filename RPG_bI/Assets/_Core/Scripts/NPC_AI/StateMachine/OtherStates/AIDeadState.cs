using UnityEngine;
using System.Collections;

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
            
            _stateMachine.OnDespawn();
        }

        public override void Update()
        {
            
        }
    }
}