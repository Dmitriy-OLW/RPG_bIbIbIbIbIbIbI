using System.Collections.Generic;
using UnityEngine;
using Enemy.Navigation;
using Enemy.Strategies;
using Character.Targeting;
using Health;
using Pooling;
using System;

namespace Enemy.State
{
    public enum AIStateType
    {
        Patrol,
        Aggression,
        Attack,
        Dead
    }
    
    public class AIStateMachine : MonoBehaviour, IPoolable
    {
        [Header("Components")]
        [SerializeField] private AIInputMapper _inputMapper;
        [SerializeField] private AINavigationController _navigation;
        [SerializeField] private AIVisionController _vision;
        [SerializeField] private Targeting _targeting;
        [SerializeField] private HealthController _healthController;
        
        [Header("AI Settings")]
        [SerializeField] private EnemyType _enemyType;
        
        [SerializeField] private Transform[] _patrolPoints;

        [SerializeField] private bool _activateWithOutPool = false;
        
        private IEnemyBehaviorStrategy _behaviorStrategy;
        private Dictionary<AIStateType, AIBaseState> _states;
        private AIBaseState _currentState;
        private AIStateType _currentStateType;

        public event Action<IPoolable> OnReturnToPool;

        public AIInputMapper InputMapper => _inputMapper;
        public AINavigationController Navigation => _navigation;
        public AIVisionController Vision => _vision;
        public Targeting Targeting => _targeting;
        public IEnemyBehaviorStrategy BehaviorStrategy => _behaviorStrategy;
        public Transform[] PatrolPoints => _patrolPoints;
        public AIStateType CurrentStateType => _currentStateType;
        public EnemyType EnemyType => _enemyType;
        
        private void Awake()
        {
            InitializeBehaviorStrategy();
            InitializeStates();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            if (_healthController != null)
                _healthController.OnDeath -= OnDeath;
            
            if (_vision != null)
                _vision.OnTargetDetected -= OnTargetDetected;
        }

        private void InitializeBehaviorStrategy()
        {
            _behaviorStrategy = _enemyType switch
            {
                EnemyType.Melee => new MeleeBehaviorStrategy(),
                EnemyType.Ranged => new RangedBehaviorStrategy(),
                _ => new MeleeBehaviorStrategy()
            };
        }

        private void InitializeStates()
        {
            _states = new Dictionary<AIStateType, AIBaseState>
            {
                { AIStateType.Patrol, new AIPatrolState(this) },
                { AIStateType.Aggression, new AIAggressionState(this) },
                { AIStateType.Attack, new AIAttackState(this) },
                { AIStateType.Dead, new AIDeadState(this) }
            };

            if (_activateWithOutPool)
                OnSpawn(transform.parent.position);
        }

        private void SubscribeToEvents()
        {
            if (_healthController != null)
                _healthController.OnDeath += OnDeath;
            
            if (_vision != null)
                _vision.OnTargetDetected += OnTargetDetected;
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
        }

        private void OnDeath()
        {
            SwitchState(AIStateType.Dead);
        }
        
        private void OnTargetDetected(Transform target)
        {
            if (_currentStateType != AIStateType.Dead)
            {
                SwitchState(AIStateType.Aggression);
            }
        }
        
        public void SetEnemyType(EnemyType newType)
        {
            _enemyType = newType;
            InitializeBehaviorStrategy();
        }
        
        public void SetPatrolPoints(Transform[] patrolPoints)
        {
            _patrolPoints = patrolPoints;
        }
        
        public void OnSpawn(Vector3 spawnPosition, float healthOverride = -1f)
        {
            if (_navigation != null)
            {
                _navigation.Warp(spawnPosition);
            }
            
            if (healthOverride >= 0)
                _healthController.ResetHealth(healthOverride);
            else
                _healthController.ResetHealth();
            
            SwitchState(AIStateType.Patrol);
            
            gameObject.transform.parent.gameObject.SetActive(true);
        }
        
        public void OnDespawn()
        {
            gameObject.transform.parent.gameObject.SetActive(false);
            OnReturnToPool?.Invoke(this);
        }
    }
}