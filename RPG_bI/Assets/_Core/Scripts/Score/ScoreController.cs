using System;

namespace Score
{
    public class ScoreController
    {
        private ScoreView _view;
        
        public ScoreController(ScoreView view)
        {
            _view = view;
            ScoreManager.ResetScore();
            ScoreManager.OnScoreChanged += UpdateScore;
            UpdateScore(ScoreManager.CurrentScore);
        }
        
        private void UpdateScore(int newScore)
        {
            _view?.UpdateDisplay(newScore);
        }
        
        public void Dispose()
        {
            ScoreManager.OnScoreChanged -= UpdateScore;
        }
    }
}