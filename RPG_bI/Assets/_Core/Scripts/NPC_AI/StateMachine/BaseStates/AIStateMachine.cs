using System.Collections.Generic;
using UnityEngine;
using Enemy.Navigation;
using Enemy.Strategies;
using Character.Targeting;
using Health;
using Pooling;
using System;
using Weapons;
using Enemy.Weapons;
using Random = UnityEngine.Random;

namespace Enemy.State
{
    public enum AIStateType
    {
        Patrol,
        Rest,
        Aggression,
        Attack,
        Search,
        Flee,
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
        [SerializeField] private EnemyWeaponController _enemyWeaponController;
        
        [Header("AI Settings")]
        [SerializeField] private Transform[] _patrolPoints;
        [SerializeField] private bool _activateWithOutPool = false;
        [SerializeField] private float _fleeHealthThreshold = 0.3f;
        [SerializeField] private float _restChance = 0.1f;
        [SerializeField] private float _restCooldown = 10f;
        
        private Dictionary<AIStateType, AIBaseState> _states;
        private AIBaseState _currentState;
        private AIStateType _currentStateType;
        private float _restTimer = 0f;
        
        // Attack switching variables
        private bool _isUsingPrimaryAttack = true;
        private float _attackSwitchChanceOnEnter = 0.2f;
        private float _attackSwitchChanceDuring = 0.1f;
        private float _attackSwitchCheckTimer = 0f;
        private float _attackSwitchCheckInterval = 2f;

        public event Action<IPoolable> OnReturnToPool;

        public AIInputMapper InputMapper => _inputMapper;
        public AINavigationController Navigation => _navigation;
        public AIVisionController Vision => _vision;
        public Targeting Targeting => _targeting;
        public EnemyWeaponController WeaponController => _enemyWeaponController;
        public Transform[] PatrolPoints => _patrolPoints;
        public AIStateType CurrentStateType => _currentStateType;
        public float FleeHealthThreshold => _fleeHealthThreshold;
        public float RestChance => _restChance;
        public float RestCooldown => _restCooldown;
        
        private void Awake()
        {
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

        private void InitializeStates()
        {
            _states = new Dictionary<AIStateType, AIBaseState>
            {
                { AIStateType.Patrol, new AIPatrolState(this) },
                { AIStateType.Rest, new AIRestState(this) },
                { AIStateType.Aggression, new AIAggressionState(this) },
                { AIStateType.Attack, CreateAttackState() },
                { AIStateType.Search, new AISearchState(this) },
                { AIStateType.Flee, new AIFleeState(this) },
                { AIStateType.Dead, new AIDeadState(this) }
            };

            if (_activateWithOutPool)
                OnSpawn(transform.parent.position);
        }

        private AIBaseState CreateAttackState()
        {
            if (_enemyWeaponController != null)
            {
                var weaponType = _isUsingPrimaryAttack 
                    ? _enemyWeaponController.GetWeaponType(WeaponStateActive.PrimaryActive)
                    : _enemyWeaponController.GetWeaponType(WeaponStateActive.SecondaryActive);
                
                switch (weaponType)
                {
                    case EnemyType.Ranged:
                        return new AIRangedAttackState(this);
                    case EnemyType.Melee:
                    default:
                        return new AIMeleeAttackState(this);
                }
            }
            
            return new AIMeleeAttackState(this);
        }
        
        private void DetermineAttackType()
        {
            if (_enemyWeaponController == null) return;
            
            bool shouldSwitchToSecondary = false;
            
            if (_enemyWeaponController.PreferredAttack == AttackPriority.Primary)
            {
                // 20% chance to use secondary when entering attack
                if (_currentStateType != AIStateType.Attack)
                {
                    shouldSwitchToSecondary = Random.value < _attackSwitchChanceOnEnter;
                }
                // 10% chance to switch during attack
                else
                {
                    _attackSwitchCheckTimer += Time.deltaTime;
                    if (_attackSwitchCheckTimer >= _attackSwitchCheckInterval)
                    {
                        _attackSwitchCheckTimer = 0f;
                        shouldSwitchToSecondary = Random.value < _attackSwitchChanceDuring;
                    }
                }
            }
            else
            {
                // If secondary is preferred, always use secondary
                shouldSwitchToSecondary = true;
            }
            
            _isUsingPrimaryAttack = !shouldSwitchToSecondary;
            
            // Recreate attack state with new weapon type
            _states[AIStateType.Attack] = CreateAttackState();
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
            
            // Update rest timer
            if (_currentStateType != AIStateType.Rest)
            {
                _restTimer += Time.deltaTime;
            }
            
            // Check for flee condition
            CheckFleeCondition();
        }
        
        private void CheckFleeCondition()
        {
            if (_currentStateType == AIStateType.Dead || _currentStateType == AIStateType.Flee)
                return;
                
            if (_healthController != null && _healthController.HealthPercentage <= _fleeHealthThreshold)
            {
                if (_vision.HasTarget)
                {
                    SwitchState(AIStateType.Flee);
                }
            }
        }

        public void SwitchState(AIStateType newStateType)
        {
            if (!_states.ContainsKey(newStateType))
                return;

            _currentState?.Exit();
            _currentStateType = newStateType;
            
            // Determine attack type when switching to attack
            if (newStateType == AIStateType.Attack)
            {
                DetermineAttackType();
            }
            
            _currentState = _states[newStateType];
            _currentState.Enter();
        }
        
        public StrategyData GetCurrentStrategy()
        {
            if (_enemyWeaponController != null)
            {
                return _isUsingPrimaryAttack 
                    ? _enemyWeaponController.GetStrategy(true)
                    : _enemyWeaponController.GetStrategy(false);
            }
            return null;
        }
        
        public bool IsUsingPrimaryAttack => _isUsingPrimaryAttack;

        private void OnDeath()
        {
            SwitchState(AIStateType.Dead);
        }
        
        private void OnTargetDetected(Transform target)
        {
            if (_currentStateType != AIStateType.Dead && _currentStateType != AIStateType.Flee)
            {
                SwitchState(AIStateType.Aggression);
            }
        }
        
        public bool CanRest()
        {
            return _restTimer >= _restCooldown && Random.value < _restChance;
        }
        
        public void ResetRestTimer()
        {
            _restTimer = 0f;
        }
        
        public void SetPatrolPoints(Transform[] patrolPoints)
        {
            _patrolPoints = patrolPoints;
        }
        
        public void OnSpawn(Vector3 spawnPosition, float healthOverride = -1f)
        {
            gameObject.transform.parent.gameObject.SetActive(true);
            
            if (healthOverride >= 0)
                _healthController.ResetHealth(healthOverride);
            else
                _healthController.ResetHealth();
            
            SwitchState(AIStateType.Patrol);
        }
        
        public void OnDespawn()
        {
            gameObject.transform.parent.gameObject.SetActive(false);
            OnReturnToPool?.Invoke(this);
        }
    }
}