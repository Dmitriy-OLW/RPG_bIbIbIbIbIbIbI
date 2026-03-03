using System.Collections.Generic;
using UnityEngine;
using Enemy.Navigation;

namespace Enemy.State
{
    public enum AIStateType
    {
        Patrol,
        Aggression,
        Attack,
        Search,
        Dead
    }

    public class AIStateMachine : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private AIInputMapper _inputMapper;
        [SerializeField] private AINavigationController _navigation;
        [SerializeField] private Transform[] _patrolPoints;
        
        private Dictionary<AIStateType, AIBaseState> _states;
        private AIBaseState _currentState;
        private AIStateType _currentStateType;

        public AIInputMapper InputMapper => _inputMapper;
        public AINavigationController Navigation => _navigation;
        public Transform[] PatrolPoints => _patrolPoints;
        public AIStateType CurrentStateType => _currentStateType;

        private void Awake()
        {
            InitializeStates();
        }

        private void InitializeStates()
        {
            _states = new Dictionary<AIStateType, AIBaseState>
            {
                { AIStateType.Patrol, new AIPatrolState(this) }
            };
            
            SwitchState(AIStateType.Patrol);
        }

        private void Update()
        {
            _currentState?.Update();
        }

        public void SwitchState(AIStateType newStateType)
        {
            if (!_states.ContainsKey(newStateType))
                return;

            _currentState?.Exit();
            _currentStateType = newStateType;
            _currentState = _states[newStateType];
            _currentState.Enter();
            
            Debug.Log($"[AI] Switched to state: {newStateType}");
        }
    }
}