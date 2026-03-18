using UnityEngine;
using Character;

namespace CharacterCamera
{
    public class SampleObjectLockOn : MonoBehaviour
    {
        [SerializeField] private Material _highlightMat;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Transform _highlightOrb;

        private MeshRenderer _meshRenderer;

        private void Start()
        {
            _meshRenderer = _highlightOrb.GetComponent<MeshRenderer>();
            
        }
        
        private void OnTriggerEnter(Collider otherCollider)
        {
            PlayerHandler playerAnimationController = otherCollider.GetComponent<PlayerHandler>();
            
            if (playerAnimationController != null)
            {
                playerAnimationController.PlayerTargeting.AddTargetCandidate(gameObject);
            }
        }

        private void OnTriggerExit(Collider otherCollider)
        {
            PlayerHandler playerAnimationController = otherCollider.GetComponent<PlayerHandler>();
            
            if (playerAnimationController != null)
            {
                playerAnimationController.PlayerTargeting.RemoveTarget(gameObject);
                Highlight(false, false);
            }
        }

        public void Highlight(bool enable, bool targetLock)
        {
            Material currentMaterial = targetLock ? _targetMat : _highlightMat;

            if (_highlightOrb != null)
            {
                _highlightOrb.gameObject.SetActive(enable);
                if (enable)
                {
                    _meshRenderer.material = currentMaterial;
                }
            }
        }
    }
}
