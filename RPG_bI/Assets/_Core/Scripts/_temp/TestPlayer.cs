using Health;
using UnityEngine;

namespace Testing
{
    public class TestPlayer : MonoBehaviour
    {
        [SerializeField] private Weapons.WeaponBase _weapon;
        [SerializeField] private HealthController _healthController;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _weapon.Attack();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                _healthController.Heal(20f);
            }
        }
    }
}