using UnityEngine;
using Weapons;
using Character.InputController;

public class WeaponController : MonoBehaviour
{
    public enum WeaponStateActive
    {
        Deactive,
        PrimaryActive,
        SecondaryActive
    }
    
    [Header("Weapons")]
    [SerializeField] private WeaponBase _primaryWeapon;
    [SerializeField] private WeaponBase _secondaryWeapon;
    
    private Animator _animator;
    private InputReader _inputReader;
    private WeaponStateActive _currentWeaponState = WeaponStateActive.Deactive;

    private static readonly int PrimaryAttack = Animator.StringToHash("PrimaryAttack");
    private static readonly int SecondaryAttack = Animator.StringToHash("SecondaryAttack");
    private static readonly int RandomAttack = Animator.StringToHash("RandomAttack");

    private void Awake()
    {
        _inputReader = GetComponent<InputReader>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.onPrimaryAttack += OnPrimaryAttack;
            _inputReader.onSecondaryAttack += OnSecondaryAttack;
        }
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.onPrimaryAttack -= OnPrimaryAttack;
            _inputReader.onSecondaryAttack -= OnSecondaryAttack;
        }
    }
    
    public void OnDamageFrame()
    {
        if(_currentWeaponState == WeaponStateActive.PrimaryActive)
            _primaryWeapon.Attack();
        else if(_currentWeaponState == WeaponStateActive.SecondaryActive)
            _secondaryWeapon.Attack();
        
        _currentWeaponState = WeaponStateActive.Deactive;
    }

    private void OnPrimaryAttack()
    {
        if (_currentWeaponState == WeaponStateActive.Deactive)
        {
            _animator.SetTrigger(PrimaryAttack);
            _currentWeaponState = WeaponStateActive.PrimaryActive;
        }
    }

    private void OnSecondaryAttack()
    {
        if (_currentWeaponState == WeaponStateActive.Deactive)
        {
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