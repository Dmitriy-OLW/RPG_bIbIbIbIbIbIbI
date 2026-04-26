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
        
        private const float RANDOM_SWITCH_CHANCE_ON_ENTER = 0.2f;
        private const float RANDOM_SWITCH_CHANCE_DURING = 0.1f;
        private const float RANDOM_SWITCH_CHECK_INTERVAL = 2f;
        
        private float _randomSwitchTimer;
        
        public AttackTransitionLogic(IEnemyWeaponProvider weaponProvider, AIVisionController vision)
        {
            _weaponProvider = weaponProvider;
            _vision = vision;
            _randomSwitchTimer = 0f;
        }
        
        public void UpdateTimer(float deltaTime)
        {
            _randomSwitchTimer += deltaTime;
        }
        
        public bool DetermineAttackOnEnter(bool currentlyUsingPrimary)
        {
            if (_weaponProvider == null || !_vision.HasTarget)
                return currentlyUsingPrimary;
            
            AttackPriority preferredAttack = _weaponProvider.PreferredAttack;
            
            EnemyType primaryType = _weaponProvider.GetWeaponType(true);
            EnemyType secondaryType = _weaponProvider.GetWeaponType(false);
            
            float primaryAttackRange = GetAttackRange(true);
            float secondaryAttackRange = GetAttackRange(false);
            
            float distanceToTarget = _vision.DistanceToTarget;
            
            bool ownIsPrimary = (preferredAttack == AttackPriority.Primary);
            float ownAttackRange = ownIsPrimary ? primaryAttackRange : secondaryAttackRange;
            float otherAttackRange = ownIsPrimary ? secondaryAttackRange : primaryAttackRange;
            
            bool canUseOwn = distanceToTarget <= ownAttackRange;
            bool canUseOther = distanceToTarget <= otherAttackRange;

            if (ownAttackRange < otherAttackRange)
            {
                if (canUseOwn)
                {
                    return ownIsPrimary;
                }
                else if (canUseOther)
                {
                    return !ownIsPrimary;
                }
                else
                {
                    return ownIsPrimary;
                }
            }
            else if (ownAttackRange > otherAttackRange)
            {
                if (canUseOther)
                {
                    return !ownIsPrimary;
                }
                else
                {
                    return ownIsPrimary;
                }
            }
            else
            {
                return ownIsPrimary;
            }
        }

        public bool ShouldSwitchDuringAttack(bool currentlyUsingPrimary)
        {
            if (_weaponProvider == null || !_vision.HasTarget)
                return false;
            
            AttackPriority preferredAttack = _weaponProvider.PreferredAttack;
            
            float primaryAttackRange = GetAttackRange(true);
            float secondaryAttackRange = GetAttackRange(false);
            
            float distanceToTarget = _vision.DistanceToTarget;
            
            bool ownIsPrimary = (preferredAttack == AttackPriority.Primary);
            float ownAttackRange = ownIsPrimary ? primaryAttackRange : secondaryAttackRange;
            float otherAttackRange = ownIsPrimary ? secondaryAttackRange : primaryAttackRange;
            
            bool currentlyOwn = (currentlyUsingPrimary == ownIsPrimary);
            bool canUseOwn = distanceToTarget <= ownAttackRange;
            bool canUseOther = distanceToTarget <= otherAttackRange;
            bool canUseCurrent = currentlyUsingPrimary ? 
                (distanceToTarget <= primaryAttackRange) : 
                (distanceToTarget <= secondaryAttackRange);

            bool mustSwitch = CheckMandatorySwitch(
                ownAttackRange, otherAttackRange, 
                ownIsPrimary, canUseOwn, canUseOther, 
                currentlyUsingPrimary, canUseCurrent, distanceToTarget);
            
            if (mustSwitch)
                return true;
            
            if (canUseOwn && canUseOther)
            {
                return CheckRandomSwitch();
            }
            
            return false;
        }
        
        private bool CheckMandatorySwitch(
            float ownAttackRange, float otherAttackRange,
            bool ownIsPrimary, bool canUseOwn, bool canUseOther,
            bool currentlyUsingPrimary, bool canUseCurrent, float distanceToTarget)
        {
            // Сценарий A: Своя атака имеет МЕНЬШИЙ радиус
            if (ownAttackRange < otherAttackRange)
            {
                // Если используем чужую (дальнюю), но игрок вошел в радиус своей (ближней) - переключаемся на свою
                if (!currentlyUsingPrimary == ownIsPrimary && canUseOwn)
                    return true;
                
                // Если используем свою (ближнюю), но игрок вышел из радиуса своей и остался в радиусе чужой - переключаемся на чужую
                if (currentlyUsingPrimary == ownIsPrimary && !canUseOwn && canUseOther)
                    return true;
            }
            // Сценарий B: Своя атака имеет БОЛЬШИЙ радиус
            else if (ownAttackRange > otherAttackRange)
            {
                // Если используем свою (дальнюю), но игрок вошел в радиус чужой (ближней) - переключаемся на чужую
                if (currentlyUsingPrimary == ownIsPrimary && canUseOther)
                    return true;
                
                // Если используем чужую (ближнюю), но игрок вышел из радиуса чужой - возвращаемся на свою
                if (!currentlyUsingPrimary == ownIsPrimary && !canUseOther)
                    return true;
            }
            
            return false;
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
    }
}