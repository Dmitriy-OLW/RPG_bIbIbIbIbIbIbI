using UnityEngine;
using System.Collections.Generic;

namespace Enemy.State
{
    public class AIFleeState : AIBaseState
    {
        private const float MIN_FLEE_DISTANCE = 10f;
        private const float MAX_FLEE_DISTANCE = 30f;
        private const float MIN_WAIT_TIME = 5f;
        private const float MAX_WAIT_TIME = 10f;
        private const float HEAL_PERCENT_PER_SECOND = 0.1f; // 10% в секунду
        
        private Vector3 _fleeDestination;
        private bool _hasReachedDestination;
        private float _waitTimer;
        private bool _isWaiting;
        private bool _hasFledOnce; // Чтобы не возвращаться в бегство повторно
        
        public AIFleeState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            _hasReachedDestination = false;
            _isWaiting = false;
            _waitTimer = 0f;
            
            // Выбираем случайную точку из патрульных точек
            _fleeDestination = GetRandomFleePoint();
            
            // Если нет патрульных точек, убегаем в направлении от цели
            if (_fleeDestination == Vector3.zero && _stateMachine.Vision.HasTarget)
            {
                Vector3 directionFromTarget = (_stateMachine.transform.position - 
                    _stateMachine.Vision.CurrentTarget.position).normalized;
                    
                float fleeDistance = Random.Range(MIN_FLEE_DISTANCE, MAX_FLEE_DISTANCE);
                _fleeDestination = _stateMachine.transform.position + directionFromTarget * fleeDistance;
            }
            else if (_fleeDestination == Vector3.zero)
            {
                // Вообще нет точек и нет цели - возвращаемся в патруль
                _stateMachine.SwitchState(AIStateType.Patrol);
                return;
            }
            
            _stateMachine.Navigation.SetDestination(_fleeDestination);
            _stateMachine.InputMapper.SetShouldRun(true);
        }

        public override void Update()
        {
            // Если обнаружили цель пока бежим - выбираем новую точку и бежим дальше
            if (!_hasReachedDestination && _stateMachine.Vision.HasTarget)
            {
                // Проверяем, не слишком ли близко цель
                float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
                
                if (distanceToTarget < MIN_FLEE_DISTANCE)
                {
                    // Цель слишком близко - выбираем новую точку для бегства
                    Vector3 newFleePoint = GetRandomFleePoint();
                    
                    if (newFleePoint != Vector3.zero)
                    {
                        _fleeDestination = newFleePoint;
                        _stateMachine.Navigation.SetDestination(_fleeDestination);
                    }
                    else
                    {
                        // Нет доступных точек, убегаем от цели
                        Vector3 directionFromTarget = (_stateMachine.transform.position - 
                            _stateMachine.Vision.CurrentTarget.position).normalized;
                            
                        _fleeDestination = _stateMachine.transform.position + directionFromTarget * MIN_FLEE_DISTANCE;
                        _stateMachine.Navigation.SetDestination(_fleeDestination);
                    }
                }
            }
            
            // Проверяем, достигли ли точки назначения
            if (!_hasReachedDestination && _stateMachine.Navigation.HasReachedDestination)
            {
                _hasReachedDestination = true;
                _isWaiting = true;
                _waitTimer = Random.Range(MIN_WAIT_TIME, MAX_WAIT_TIME);
                
                _stateMachine.Navigation.ClearPath();
                _stateMachine.InputMapper.SetShouldRun(false);
                
                // Если обнаружена цель когда дошли - выбираем новую точку и бежим
                if (_stateMachine.Vision.HasTarget)
                {
                    Vector3 newFleePoint = GetRandomFleePoint();
                    
                    if (newFleePoint != Vector3.zero)
                    {
                        _hasReachedDestination = false;
                        _isWaiting = false;
                        _fleeDestination = newFleePoint;
                        _stateMachine.Navigation.SetDestination(_fleeDestination);
                        _stateMachine.InputMapper.SetShouldRun(true);
                    }
                }
            }
            
            // Ожидание на точке
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                
                // Хилл если включена опция
                if (_stateMachine.CanSelfHeal && _stateMachine.HealthController != null)
                {
                    _stateMachine.HealthController.HealPercent(HEAL_PERCENT_PER_SECOND * Time.deltaTime);
                }
                
                // Проверяем, не появилась ли цель
                if (_stateMachine.Vision.HasTarget)
                {
                    // Появилась цель - выбираем новую точку и бежим
                    Vector3 newFleePoint = GetRandomFleePoint();
                    
                    if (newFleePoint != Vector3.zero)
                    {
                        _isWaiting = false;
                        _hasReachedDestination = false;
                        _fleeDestination = newFleePoint;
                        _stateMachine.Navigation.SetDestination(_fleeDestination);
                        _stateMachine.InputMapper.SetShouldRun(true);
                        return;
                    }
                }
                
                // Время ожидания вышло - возвращаемся в патруль
                if (_waitTimer <= 0f)
                {
                    _hasFledOnce = true; // Помечаем что бегство было
                    _stateMachine.SwitchState(AIStateType.Patrol);
                }
            }
        }
        
        private Vector3 GetRandomFleePoint()
        {
            if (_stateMachine.PatrolPoints == null || _stateMachine.PatrolPoints.Length == 0)
                return Vector3.zero;
            
            // Собираем все доступные точки
            List<Transform> availablePoints = new List<Transform>();
            
            foreach (var point in _stateMachine.PatrolPoints)
            {
                if (point != null)
                {
                    availablePoints.Add(point);
                }
            }
            
            if (availablePoints.Count == 0)
                return Vector3.zero;
            
            // Если есть цель, фильтруем точки подальше от неё
            if (_stateMachine.Vision.HasTarget)
            {
                Vector3 targetPos = _stateMachine.Vision.CurrentTarget.position;
                
                // Сортируем точки по расстоянию от цели (дальние предпочтительнее)
                availablePoints.Sort((a, b) => 
                    Vector3.Distance(b.position, targetPos)
                    .CompareTo(Vector3.Distance(a.position, targetPos)));
                
                // Берём случайную из дальних точек (первые 50% или минимум 1)
                int count = Mathf.Max(1, availablePoints.Count / 2);
                int randomIndex = Random.Range(0, count);
                return availablePoints[randomIndex].position;
            }
            
            // Если цели нет - просто случайная точка
            return availablePoints[Random.Range(0, availablePoints.Count)].position;
        }

        public override void Exit()
        {
            _stateMachine.InputMapper.SetShouldRun(false);
            _stateMachine.Navigation.ClearPath();
        }
    }
}