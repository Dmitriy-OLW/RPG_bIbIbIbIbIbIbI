using Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Testing
{
    public class TestPlayer : MonoBehaviour
    {
        [SerializeField] private WeaponBase _weapon;

        public void OnLook(InputAction.CallbackContext context)
        {
            _weapon.Attack();
        }
    }
}