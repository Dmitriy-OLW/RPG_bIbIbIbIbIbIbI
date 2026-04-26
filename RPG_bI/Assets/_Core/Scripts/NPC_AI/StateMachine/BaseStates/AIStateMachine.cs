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
        
        [Header("AI Settings")]
        [SerializeField] private Transform[] _patrolPoints;
        [SerializeField] private bool _activateWithOutPool = false;
        [SerializeField] private float _fleeHealthThreshold = 0.3f;
        [SerializeField] private float _restChance = 0.1f;
        [SerializeField] private float _restCooldown = 10f;
        [Header("Flee Settings")]
        [SerializeField] private bool _canFlee = true;
        [SerializeField] private bool _canSelfHeal = false;
        [SerializeField] private bool _allowRepeatFlee = false;
        
        private Dictionary<AIStateType, AIBaseState> _states;
        private AIBaseState _currentState;
        private AIStateType _currentStateType;
        private float _restTimer = 0f;
        
        private bool _hasFledOnce = false;

        private IEnemyWeaponProvider _weaponProvider;
        private AttackTransitionLogic _attackTransitionLogic;

        private bool _isUsingPrimaryAttack = true;

        public event Action<IPoolable> OnReturnToPool;

        public AIInputMapper InputMapper => _inputMapper;
        public AINavigationController Navigation => _navigation;
        public AIVisionController Vision => _vision;
        public Targeting Targeting => _targeting;
        public Transform[] PatrolPoints => _patrolPoints;
        public AIStateType CurrentStateType => _currentStateType;
        public float FleeHealthThreshold => _fleeHealthThreshold;
        public float RestChance => _restChance;
        public float RestCooldown => _restCooldown;
        public IEnemyWeaponProvider WeaponProvider => _weaponProvider;
        public bool IsUsingPrimaryAttack => _isUsingPrimaryAttack;
        public bool CanSelfHeal => _canSelfHeal;
        public HealthController HealthController => _healthController;
        public bool HasFledOnce => _hasFledOnce;
        public bool AllowRepeatFlee => _allowRepeatFlee;
        public bool CanFlee => _canFlee;
        
        private void Awake()
        {
            _weaponProvider = GetComponent<IEnemyWeaponProvider>();
            
            if (_weaponProvider != null)
            {
                _attackTransitionLogic = new AttackTransitionLogic(_weaponProvider, _vision);
            }
            
            InitializeStates();
            SubscribeToEvents();
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
            if (_attackTransitionLogic != null)
            {
                return _attackTransitionLogic.CreateAttackState(this, _isUsingPrimaryAttack);
            }
            
            return new AIMeleeAttackState(this);
        }
        
        private void DetermineAttackTypeOnEnter()
        {
            if (_attackTransitionLogic == null) return;
            
            _attackTransitionLogic.ResetRandomTimer();
            _isUsingPrimaryAttack = _attackTransitionLogic.DetermineAttackOnEnter(_isUsingPrimaryAttack);
            
            _states[AIStateType.Attack] = CreateAttackState();
        }
        
        public void CheckAttackSwitchDuring()
        {
            if (_attackTransitionLogic == null) return;
            
            bool shouldSwitch = _attackTransitionLogic.ShouldSwitchDuringAttack(_isUsingPrimaryAttack);
            
            if (shouldSwitch)
            {
                _isUsingPrimaryAttack = !_isUsingPrimaryAttack;
                
                var newAttackState = CreateAttackState();
                _states[AIStateType.Attack] = newAttackState;
                
                _currentState?.Exit();
                _currentState = newAttackState;
                _currentState.Enter();
            }
        }

        private void SubscribeToEvents()
        {
            if (_healthController != null)
            {
                _healthController.OnDeath += OnDeath;
                _healthController.OnHit += OnHit;
            }
            
            if (_vision != null)
            {
                _vision.OnTargetDetected += OnTargetDetected;
                _vision.OnTargetLost += OnTargetLost;
            }
        }

        private void OnDestroy()
        {
            if (_healthController != null)
            {
                _healthController.OnDeath -= OnDeath;
                _healthController.OnHit -= OnHit;
            }
            
            if (_vision != null)
            {
                _vision.OnTargetDetected -= OnTargetDetected;
                _vision.OnTargetLost -= OnTargetLost;
            }
        }
        
        private void Update()
        {
            _currentState?.Update();
            
            if (_currentStateType == AIStateType.Attack)
            {
                _attackTransitionLogic?.UpdateTimer(Time.deltaTime);
            }
            
            if (_currentStateType != AIStateType.Rest)
            {
                _restTimer += Time.deltaTime;
            }
            
            CheckFleeCondition();
        }
        
        private void CheckFleeCondition()
        {
            if (_currentStateType == AIStateType.Dead || _currentStateType == AIStateType.Flee)
                return;

            if (!_canFlee)
                return;
            
            if (_hasFledOnce)
                return;
        
            if (_healthController != null && _healthController.HealthPercentage <= _fleeHealthThreshold)
            {
                _hasFledOnce = true; 
                SwitchState(AIStateType.Flee);
            }
        }

        public void SwitchState(AIStateType newStateType)
        {
            if (!_states.ContainsKey(newStateType))
                return;
            
            if (_currentStateType == AIStateType.Attack && newStateType != AIStateType.Attack)
            {
                _attackTransitionLogic?.ResetCloseInTimer();
            }

            _currentState?.Exit();
            _currentStateType = newStateType;
    
            if (newStateType == AIStateType.Attack)
            {
                DetermineAttackTypeOnEnter();
            }
    
            _currentState = _states[newStateType];
            _currentState.Enter();
        }
        
        public void DetermineAttackTypeForAggression()
        {
            if (_attackTransitionLogic == null) return;
            
            _isUsingPrimaryAttack = _attackTransitionLogic.DetermineAttackOnEnter(_isUsingPrimaryAttack);
            _states[AIStateType.Attack] = CreateAttackState();
        }
        
        public StrategyData GetCurrentStrategy()
        {
            if (_weaponProvider != null)
            {
                return _weaponProvider.GetStrategy(_isUsingPrimaryAttack);
            }
            return null;
        }
        
        public StrategyData GetPrimaryStrategy()
        {
            return _weaponProvider?.GetStrategy(true);
        }
        
        public StrategyData GetSecondaryStrategy()
        {
            return _weaponProvider?.GetStrategy(false);
        }

        private void OnDeath()
        {
            SwitchState(AIStateType.Dead);
        }
        
        private void OnHit()
        {
            if (!_canFlee) 
                return;
            
            if (_allowRepeatFlee && 
                _hasFledOnce &&
                _healthController.HealthPercentage <= _fleeHealthThreshold &&
                _currentStateType != AIStateType.Flee && 
                _currentStateType != AIStateType.Dead)
            {
                SwitchState(AIStateType.Flee);
            }
        }

        public void OnAttackPerformed()
        {
            _attackTransitionLogic?.OnAttackPerformed(_isUsingPrimaryAttack);
        }
        
        
        private void OnTargetDetected(Transform target)
        {
            if (_currentStateType != AIStateType.Dead && _currentStateType != AIStateType.Flee)
                SwitchState(AIStateType.Aggression);
        }
        
        private void OnTargetLost()
        {
            if (_currentStateType == AIStateType.Aggression || _currentStateType == AIStateType.Attack)
                SwitchState(AIStateType.Search);
            
        }
        
        public bool CanRest()
        {
            return _restTimer >= _restCooldown && Random.value < _restChance;
        }
        
        public void ResetRestTimer() => _restTimer = 0f;

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
            
            _hasFledOnce = false;

            SwitchState(AIStateType.Patrol);
        }
        
        public void OnDespawn()
        {
            gameObject.transform.parent.gameObject.SetActive(false);
            OnReturnToPool?.Invoke(this);
        }
    }
}