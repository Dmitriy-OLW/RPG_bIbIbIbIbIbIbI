using UnityEngine;
using Weapons;
using Character.InputController;

public class WeaponController : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private MeleeWeapon _meleeWeapon;
    [SerializeField] private RangeWeapon _rangeWeapon;
    
    private Animator _animator;
    private InputReader _inputReader;
    
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
            _inputReader.onMeleeAttack += OnMeleeAttack;
            _inputReader.onRangeAttack += OnRangeAttack;
        }
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.onMeleeAttack -= OnMeleeAttack;
            _inputReader.onRangeAttack -= OnRangeAttack;
        }
    }
    
    public void OnMeleeDamageFrame()
    {
        _meleeWeapon.Attack();
    }
        
    public void OnRangeFireFrame()
    {
        _rangeWeapon.Attack();
    }

    private void OnMeleeAttack()
    {
        _animator.SetTrigger(PrimaryAttack);
    }

    private void OnRangeAttack()
    {
        _animator.SetTrigger(SecondaryAttack);
    }

    private void SetRandomAnimation()
    {
        bool randomAttackValue = Random.value > 0.5f;
        _animator.SetBool(RandomAttack, randomAttackValue);
    }

}