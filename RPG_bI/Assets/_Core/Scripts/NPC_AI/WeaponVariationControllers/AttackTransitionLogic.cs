using UnityEngine;
using Weapons;
using Enemy.Strategies;
using Enemy.Weapons;
using Enemy.Navigation;

namespace Enemy.State
{
    public class AttackTransitionLogic
    {
        private readonly IEnemyWeaponProvider _weaponProvider;
        private readonly AIVisionController _vision;
        
        // Константы для случайного переключения
        private const float RANDOM_SWITCH_CHANCE_DURING = 0.1f;
        private const float RANDOM_SWITCH_CHECK_INTERVAL = 2f;
        
        // Таймер для обязательного сближения
        private const float CLOSE_IN_TIMEOUT = 10f;
        
        private float _randomSwitchTimer;
        private float _closeInTimer;
        private bool _isClosingIn;
        
        // Отслеживание "запасной" атаки
        private bool _usingFallbackAttack; // Используем чужую (дальнюю) атаку из-за таймаута
        private bool _fallbackAttackUsed;  // Запасная атака была выполнена
        
        public AttackTransitionLogic(IEnemyWeaponProvider weaponProvider, AIVisionController vision)
        {
            _weaponProvider = weaponProvider;
            _vision = vision;
            _randomSwitchTimer = 0f;
            _closeInTimer = 0f;
            _isClosingIn = false;
            _usingFallbackAttack = false;
            _fallbackAttackUsed = false;
        }
        
        public void UpdateTimer(float deltaTime)
        {
            _randomSwitchTimer += deltaTime;
            
            if (_isClosingIn)
            {
                _closeInTimer += deltaTime;
            }
        }
        
        /// <summary>
        /// Вызывается когда враг выполнил атаку (из WeaponController.OnDamageFrame)
        /// </summary>
        public void OnAttackPerformed(bool isPrimaryAttack)
        {
            if (_usingFallbackAttack && !_fallbackAttackUsed)
            {
                // Это была первая атака из запасного оружия
                _fallbackAttackUsed = true;
            }
        }
        
        /// <summary>
        /// Проверяет, нужно ли сбросить таймер и вернуться к своей атаке после выстрела из запасной
        /// </summary>
        public bool ShouldResetAfterFallbackAttack()
        {
            if (_usingFallbackAttack && _fallbackAttackUsed)
            {
                // Сбрасываем флаги и таймер для новой попытки сближения
                _usingFallbackAttack = false;
                _fallbackAttackUsed = false;
                _isClosingIn = true;
                _closeInTimer = 0f;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Определяет, какую атаку использовать при входе в состояние атаки
        /// </summary>
        public bool DetermineAttackOnEnter(bool currentlyUsingPrimary)
        {
            if (_weaponProvider == null || !_vision.HasTarget)
                return currentlyUsingPrimary;
            
            if (_weaponProvider.OnlyPreferredAttack)
            {
                return _weaponProvider.PreferredAttack == AttackPriority.Primary;
            }
            
            AttackPriority preferredAttack = _weaponProvider.PreferredAttack;
            
            float primaryAttackRange = GetAttackRange(true);
            float secondaryAttackRange = GetAttackRange(false);
            float distanceToTarget = _vision.DistanceToTarget;
            
            bool ownIsPrimary = (preferredAttack == AttackPriority.Primary);
            float ownAttackRange = ownIsPrimary ? primaryAttackRange : secondaryAttackRange;
            float otherAttackRange = ownIsPrimary ? secondaryAttackRange : primaryAttackRange;
            
            bool canUseOwn = distanceToTarget <= ownAttackRange;
            
            // Сброс состояния запасной атаки при входе
            _usingFallbackAttack = false;
            _fallbackAttackUsed = false;
            
            // Если своя атака имеет МЕНЬШИЙ радиус (предпочитаем ближний бой)
            if (ownAttackRange < otherAttackRange)
            {
                if (canUseOwn)
                {
                    _isClosingIn = false;
                    _closeInTimer = 0f;
                    return ownIsPrimary;
                }
                else
                {
                    _isClosingIn = true;
                    _closeInTimer = 0f;
                    return ownIsPrimary;
                }
            }
            // Если своя атака имеет БОЛЬШИЙ радиус (предпочитаем дальний бой)
            else if (ownAttackRange > otherAttackRange)
            {
                _isClosingIn = false;
                _closeInTimer = 0f;
                
                bool canUseOther = distanceToTarget <= otherAttackRange;
                
                if (canUseOther)
                {
                    return !ownIsPrimary;
                }
                else
                {
                    return ownIsPrimary;
                }
            }
            // Равные радиусы
            else
            {
                _isClosingIn = false;
                _closeInTimer = 0f;
                return ownIsPrimary;
            }
        }
        
        /// <summary>
        /// Определяет, нужно ли переключить атаку во время боя
        /// </summary>
        public bool ShouldSwitchDuringAttack(bool currentlyUsingPrimary)
        {
            if (_weaponProvider == null || !_vision.HasTarget)
                return false;
            
            if (_weaponProvider.OnlyPreferredAttack)
            {
                return false;
            }
            
            AttackPriority preferredAttack = _weaponProvider.PreferredAttack;
            
            float primaryAttackRange = GetAttackRange(true);
            float secondaryAttackRange = GetAttackRange(false);
            float distanceToTarget = _vision.DistanceToTarget;
            
            bool ownIsPrimary = (preferredAttack == AttackPriority.Primary);
            float ownAttackRange = ownIsPrimary ? primaryAttackRange : secondaryAttackRange;
            float otherAttackRange = ownIsPrimary ? secondaryAttackRange : primaryAttackRange;
            
            bool canUseOwn = distanceToTarget <= ownAttackRange;
            bool canUseOther = distanceToTarget <= otherAttackRange;
            
            // Сценарий: своя атака имеет МЕНЬШИЙ радиус (предпочитаем ближний бой)
            if (ownAttackRange < otherAttackRange)
            {
                // Если мы в запасной атаке и уже выстрелили - возвращаемся к своей
                if (_usingFallbackAttack && _fallbackAttackUsed)
                {
                    // Сбрасываем и начинаем новое сближение
                    _usingFallbackAttack = false;
                    _fallbackAttackUsed = false;
                    _isClosingIn = true;
                    _closeInTimer = 0f;
                    
                    // Переключаемся на свою атаку
                    if (currentlyUsingPrimary != ownIsPrimary)
                    {
                        return true;
                    }
                    return false;
                }
                
                if (_isClosingIn)
                {
                    if (canUseOwn)
                    {
                        _isClosingIn = false;
                        _closeInTimer = 0f;
                        
                        if (currentlyUsingPrimary != ownIsPrimary)
                        {
                            return true;
                        }
                        return false;
                    }
                    
                    if (_closeInTimer >= CLOSE_IN_TIMEOUT)
                    {
                        // Время вышло - переключаемся на чужую (дальнюю) атаку
                        _isClosingIn = false;
                        _closeInTimer = 0f;
                        _usingFallbackAttack = true;
                        _fallbackAttackUsed = false;
                        
                        if (canUseOther)
                        {
                            return currentlyUsingPrimary == ownIsPrimary;
                        }
                        return false;
                    }
                    
                    return false;
                }
                else
                {
                    if (canUseOwn)
                    {
                        if (currentlyUsingPrimary != ownIsPrimary)
                        {
                            _usingFallbackAttack = false;
                            _fallbackAttackUsed = false;
                            return true;
                        }
                    }
                    
                    if (!canUseOther && currentlyUsingPrimary != ownIsPrimary)
                    {
                        _isClosingIn = true;
                        _closeInTimer = 0f;
                        _usingFallbackAttack = false;
                        _fallbackAttackUsed = false;
                        return true;
                    }
                    
                    if (canUseOwn && canUseOther && currentlyUsingPrimary != ownIsPrimary)
                    {
                        return CheckRandomSwitch();
                    }
                    
                    return false;
                }
            }
            // Сценарий: своя атака имеет БОЛЬШИЙ радиус (предпочитаем дальний бой)
            else if (ownAttackRange > otherAttackRange)
            {
                if (currentlyUsingPrimary == ownIsPrimary && canUseOther)
                {
                    return true;
                }
                
                if (currentlyUsingPrimary != ownIsPrimary && !canUseOther)
                {
                    return true;
                }
                
                if (canUseOwn && canUseOther)
                {
                    return CheckRandomSwitch();
                }
                
                return false;
            }
            // Равные радиусы
            else
            {
                if (canUseOwn && canUseOther)
                {
                    return CheckRandomSwitch();
                }
                return false;
            }
        }
        
        private bool CheckRandomSwitch()
        {
            if (_randomSwitchTimer >= RANDOM_SWITCH_CHECK_INTERVAL)
            {
                _randomSwitchTimer = 0f;
                return Random.value < RANDOM_SWITCH_CHANCE_DURING;
            }
            return false;
        }
        
        private float GetAttackRange(bool isPrimary)
        {
            StrategyData strategy = _weaponProvider.GetStrategy(isPrimary);
            return strategy != null ? strategy.AttackRange : 0f;
        }
        
        public AIBaseState CreateAttackState(AIStateMachine stateMachine, bool usePrimary)
        {
            EnemyType weaponType = _weaponProvider.GetWeaponType(usePrimary);
            
            switch (weaponType)
            {
                case EnemyType.Ranged:
                    return new AIRangedAttackState(stateMachine);
                case EnemyType.Melee:
                default:
                    return new AIMeleeAttackState(stateMachine);
            }
        }
        
        public void ResetRandomTimer()
        {
            _randomSwitchTimer = 0f;
        }
        
        public void ResetCloseInTimer()
        {
            _closeInTimer = 0f;
            _isClosingIn = false;
            _usingFallbackAttack = false;
            _fallbackAttackUsed = false;
        }
    }
}