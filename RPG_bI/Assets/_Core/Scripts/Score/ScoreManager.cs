namespace Score
{
    public static class ScoreManager
    {
        private static int _currentScore;
        
        public static event System.Action<int> OnScoreChanged;
        
        public static int CurrentScore => _currentScore;
        
        public static void AddScore(int points)
        {
            _currentScore += points;
            OnScoreChanged?.Invoke(_currentScore);
        }
        
        public static void ResetScore()
        {
            _currentScore = 0;
            OnScoreChanged?.Invoke(_currentScore);
        }
    }
}