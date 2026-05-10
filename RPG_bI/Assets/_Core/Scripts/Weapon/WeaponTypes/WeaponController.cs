using UnityEngine;
using Weapons;
using Character.InputController;

namespace Weapons
{
    public enum WeaponStateActive
    {
        Deactive,
        PrimaryActive,
        SecondaryActive
    }
    public class WeaponController : MonoBehaviour
    {
        [Header("Weapons")] [SerializeField] protected WeaponBase _primaryWeapon;
        [SerializeField] protected WeaponBase _secondaryWeapon;

        protected Animator _animator;
        protected BaseInputReader _inputReader;
        protected WeaponStateActive _currentWeaponState = WeaponStateActive.Deactive;

        protected static readonly int PrimaryAttack = Animator.StringToHash("PrimaryAttack");
        protected static readonly int SecondaryAttack = Animator.StringToHash("SecondaryAttack");
        protected static readonly int RandomAttack = Animator.StringToHash("RandomAttack");

        private void Awake()
        {
            _inputReader = GetComponent<BaseInputReader>();
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnPrimaryAttackActivated += OnPrimaryAttack;
                _inputReader.OnSecondaryAttackActivated += OnSecondaryAttack;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnPrimaryAttackActivated -= OnPrimaryAttack;
                _inputReader.OnSecondaryAttackActivated -= OnSecondaryAttack;
            }
        }

        // Метод для вызова из анаматора
        public void OnDamageFrame()
        {
            if (_currentWeaponState == WeaponStateActive.PrimaryActive)
                _primaryWeapon.Attack();
            else if (_currentWeaponState == WeaponStateActive.SecondaryActive)
                _secondaryWeapon.Attack();

            _currentWeaponState = WeaponStateActive.Deactive;
        }

        public EnemyType GetWeaponType(WeaponStateActive weaponSlot)
        {
            switch (weaponSlot)
            {
                case WeaponStateActive.PrimaryActive:
                    return _primaryWeapon != null ? _primaryWeapon.EnemyType : EnemyType.Melee;
                case WeaponStateActive.SecondaryActive:
                    return _secondaryWeapon != null ? _secondaryWeapon.EnemyType : EnemyType.Melee;
                default:
                    return EnemyType.Melee;
            }
        }

        public void SetWeapon(WeaponStateActive weaponSlot, WeaponBase weapon)
        {
            switch (weaponSlot)
            {
                case WeaponStateActive.PrimaryActive:
                    _primaryWeapon = weapon;
                    break;
                case WeaponStateActive.SecondaryActive:
                    _secondaryWeapon = weapon;
                    break;
            }
        }

        protected virtual void OnPrimaryAttack()
        {
            if (_currentWeaponState == WeaponStateActive.Deactive)
            {
                SetRandomAnimation();
                _animator.SetTrigger(PrimaryAttack);
                _currentWeaponState = WeaponStateActive.PrimaryActive;
            }
        }

        protected virtual void OnSecondaryAttack()
        {
            if (_currentWeaponState == WeaponStateActive.Deactive)
            {
                SetRandomAnimation();
                _animator.SetTrigger(SecondaryAttack);
                _currentWeaponState = WeaponStateActive.SecondaryActive;
            }
        }

        private void SetRandomAnimation()
        {
            bool randomAttackValue = Random.value > 0.5f;
            _animator.SetBool(RandomAttack, randomAttackValue);
        }
    }
}