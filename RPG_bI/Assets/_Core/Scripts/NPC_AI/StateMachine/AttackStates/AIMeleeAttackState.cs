using UnityEngine;
using Enemy.Navigation;
using Enemy.Strategies;

namespace Enemy.State
{
    public class AIMeleeAttackState : AIBaseAttackState
    {
        private bool _hasReachedPreferredDistance; // Флаг что достигли оптимальной дистанции
        private const float STOP_THRESHOLD = 0.3f; // Допустимая погрешность чтобы не дёргаться

        public AIMeleeAttackState(AIStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _hasReachedPreferredDistance = false;
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
                
                // Вышли из зоны атаки - возвращаемся в агрессию
                _stateMachine.SwitchState(AIStateType.Aggression);
                return;
            }
            
            UpdateMeleeBehavior(target, strategyData);
        }

        private void UpdateMeleeBehavior(Transform target, StrategyData strategyData)
        {
            float distanceToTarget = _stateMachine.Vision.DistanceToTarget;
            float preferredDistance = strategyData.PreferredDistance;
            bool shouldAttack = distanceToTarget <= strategyData.AttackRange;
            
            // Определяем, достигли ли мы предпочтительной дистанции
            if (distanceToTarget <= preferredDistance + STOP_THRESHOLD)
            {
                // Достигли или даже ближе чем нужно — останавливаемся
                if (!_hasReachedPreferredDistance)
                {
                    _hasReachedPreferredDistance = true;
                    _stateMachine.Navigation.ClearPath();
                    _stateMachine.InputMapper.SetShouldRun(false);
                }
            }
            else
            {
                // Цель дальше предпочтительной дистанции — бежим к ней
                _hasReachedPreferredDistance = false;
                _stateMachine.Navigation.SetDestination(target.position);
                _stateMachine.InputMapper.SetShouldRun(true);
            }
            
            // Если игрок отошёл пока мы стояли — снова бежим
            if (_hasReachedPreferredDistance && distanceToTarget > preferredDistance + STOP_THRESHOLD + 0.5f)
            {
                _hasReachedPreferredDistance = false;
                _stateMachine.Navigation.SetDestination(target.position);
                _stateMachine.InputMapper.SetShouldRun(true);
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

        public override void Exit()
        {
            base.Exit();
            _hasReachedPreferredDistance = false;
        }
    }
}