using UnityEngine;
using TMPro;

namespace Score
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private string _prefix = "";
        
        private ScoreController _controller;
        
        private void Start()
        {
            _controller = new ScoreController(this);
        }
        
        private void OnDestroy()
        {
            _controller?.Dispose();
        }
        
        public void UpdateDisplay(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = _prefix + score.ToString();
            }
        }
    }
}