using UnityEngine;
using UnityEngine.UI;
using Weapons;

namespace RPGbI.UI
{
    public class ChargeBarUI : MonoBehaviour
    {
        [SerializeField] private RangeWeapon _rangeWeapon;
        [SerializeField] private Image _fillImage;

        private void Update()
        {
            if (_rangeWeapon != null && _fillImage != null)
            {
                _fillImage.fillAmount = _rangeWeapon.CurrentChargeProgress;
            }
        }
    }
}