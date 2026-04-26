using UnityEngine;
using Enemy.Navigation;
using Enemy.Strategies;

namespace Enemy.State
{
    public class AIMeleeAttackState : AIBaseAttackState
    {
        // Убираем MELEE_HOLD_DISTANCE, используем данные из StrategyData
        private const float MIN_DISTANCE_TO_TARGET = 1.5f; // Минимальная дистанция чтобы не врезаться

        public AIMeleeAttackState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
            
            if (!_stateMachine.Vision.HasTarget)
                return;

            Transform target = _stateMachine.Vision.CurrentTarget;
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            
            var strategyData = _stateMachine.GetCurrentStrategy();
            
            if (strategyData == null) return;
            
            _attackTimer -= Time.deltaTime;

            // Проверяем выход из зоны атаки
            if (distanceToTarget > strategyData.AttackRange)
            {
                StrategyData otherStrategy = _stateMachine.IsUsingPrimaryAttack 
                    ? _stateMachine.GetSecondaryStrategy() 
                    : _stateMachine.GetPrimaryStrategy();
                
                if (otherStrategy != null && distanceToTarget <= otherStrategy.AttackRange)
                {
                    _stateMachine.CheckAttackSwitchDuring();
                    return;
                }
                
                // Вышли из всех зон атаки - возвращаемся в агрессию
                _stateMachine.SwitchState(AIStateType.Aggression);
                return;
            }
            
            UpdateMeleeBehavior(target, strategyData);
        }

        private void UpdateMeleeBehavior(Transform target, StrategyData strategyData)
        {
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            bool shouldAttack = distanceToTarget <= strategyData.AttackRange;
            
            // ВСЕГДА движемся к цели в ближнем бою, используя PreferredDistance из стратегии
            float preferredDistance = strategyData.PreferredDistance;
            
            if (distanceToTarget > preferredDistance)
            {
                // Бежим к цели если дальше предпочтительной дистанции
                _stateMachine.Navigation.SetDestination(target.position);
                _stateMachine.InputMapper.SetShouldRun(true);
            }
            else if (distanceToTarget < MIN_DISTANCE_TO_TARGET)
            {
                // Слишком близко - немного отходим
                Vector3 directionAway = (_stateMachine.transform.position - target.position).normalized;
                Vector3 backPosition = _stateMachine.transform.position + directionAway * preferredDistance;
                _stateMachine.Navigation.SetDestination(backPosition);
                _stateMachine.InputMapper.SetShouldRun(false);
            }
            else
            {
                // На оптимальной дистанции - стоим и бьём
                _stateMachine.Navigation.ClearPath();
                _stateMachine.InputMapper.SetShouldRun(false);
            }
            
            // Поворачиваемся к цели всегда
            Vector3 directionToTarget = (target.position - _stateMachine.transform.position).normalized;
            directionToTarget.y = 0;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                _stateMachine.transform.rotation = Quaternion.Slerp(
                    _stateMachine.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * 10f
                );
            }
            
            if (shouldAttack && _attackTimer <= 0f)
            {
                if (_stateMachine.IsUsingPrimaryAttack)
                {
                    _stateMachine.InputMapper.InputReader.PerformPrimaryAttack();
                }
                else
                {
                    _stateMachine.InputMapper.InputReader.PerformSecondaryAttack();
                }
                
                _stateMachine.OnAttackPerformed();
                
                _attackTimer = strategyData.AttackCooldown;
            }
        }
    }
}