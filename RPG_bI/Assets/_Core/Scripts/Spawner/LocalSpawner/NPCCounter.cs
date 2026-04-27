using UnityEngine;

namespace Utilities
{
    public class ChildCounter : MonoBehaviour
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private GameObject _objectToDisable;
        [SerializeField] private int _maxCount = 25;
        
        private void Update()
        {
            if (_parent == null || _objectToDisable == null)
                return;
            
            int childCount = _parent.childCount;
            
            if (childCount > _maxCount)
            {
                if (_objectToDisable.activeSelf)
                    _objectToDisable.SetActive(false);
            }
        }
    }
}