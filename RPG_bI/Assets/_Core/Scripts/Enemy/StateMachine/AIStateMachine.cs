using System.Collections.Generic;
using UnityEngine;
using Health;
using Enemy.Data;
using Enemy.Navigation;
using Character.Targeting;

namespace Enemy.State
{
    public class AIStateMachine : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private AIInputMapper _inputMapper;
        [SerializeField] private AINavigationController _navigation;
        [SerializeField] private AIVisionController _vision;
        [SerializeField] private HealthController _healthController;
        [SerializeField] private EnemyType _enemyType;
        [SerializeField] private Transform[] _patrolPoints;
        
        private Dictionary<AIStateType, IAIState> _states;
        private IAIState _currentState;
        private AIStateType _currentStateType;

        public AIInputMapper InputMapper => _inputMapper;
        public AINavigationController Navigation => _navigation;
        public AIVisionController Vision => _vision;
        public EnemyType EnemyType => _enemyType;
        public Transform[] PatrolPoints => _patrolPoints;
        public AIStateType CurrentStateType => _currentStateType;

        private void Awake()
        {
            InitializeStates();
            SubscribeToEvents();
        }

        private void InitializeStates()
        {
            _states = new Dictionary<AIStateType, IAIState>
            {
                { AIStateType.Patrol, new AIPatrolState(this) },
                { AIStateType.Aggression, new AIAggressionState(this) },
                { AIStateType.Attack, new AIAttackState(this) },
                { AIStateType.Search, new AISearchState(this) },
                { AIStateType.Dead, new AIDeadState(this) }
            };
            
            SwitchState(AIStateType.Patrol);
        }

        private void SubscribeToEvents()
        {
            _healthController.OnDeath += HandleDeath;
            _vision.OnTargetDetected += HandleTargetDetected;
            _vision.OnTargetLost += HandleTargetLost;
        }

        private void OnDestroy()
        {
            _healthController.OnDeath -= HandleDeath;
            _vision.OnTargetDetected -= HandleTargetDetected;
            _vision.OnTargetLost -= HandleTargetLost;
        }

        private void Update()
        {
            _currentState?.Update();
        }

        public void SwitchState(AIStateType newStateType)
        {
            if (_states.ContainsKey(newStateType))
            {
                _currentState?.Exit();
                _currentStateType = newStateType;
                _currentState = _states[newStateType];
                _currentState.Enter();
            }
        }

        private void HandleDeath()
        {
            SwitchState(AIStateType.Dead);
        }

        private void HandleTargetDetected(Transform target)
        {
            if (_currentStateType != AIStateType.Dead)
            {
                SwitchState(AIStateType.Aggression);
            }
        }

        private void HandleTargetLost()
        {
            if (_currentStateType != AIStateType.Dead && _currentStateType != AIStateType.Search)
            {
                SwitchState(AIStateType.Search);
            }
        }
    }

    public interface IAIState
    {
        void Enter();
        void Update();
        void Exit();
    }
}