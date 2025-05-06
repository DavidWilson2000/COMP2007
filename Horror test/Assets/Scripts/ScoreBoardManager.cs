using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreboardManager : MonoBehaviour
{
    private const int MaxScores = 10;
    private const string ScoreKeyPrefix = "HighScore";

    [System.Serializable]
    public class ScoreEntry
    {
        public string playerName;
        public int score;

        public ScoreEntry(string name, int score)
        {
            playerName = name;
            this.score = score;
        }
    }

    public List<ScoreEntry> highScores = new List<ScoreEntry>();
    public TextMeshProUGUI scoreboardText;

    void Start()
    {
        LoadScores();
        UpdateScoreboardDisplay(); // Auto-show at start if needed
    }

    public void TryAddScore(int newScore, string newName)
    {
        highScores.Add(new ScoreEntry(newName, newScore));
        highScores.Sort((a, b) => b.score.CompareTo(a.score));

        if (highScores.Count > MaxScores)
            highScores.RemoveAt(highScores.Count - 1);
        Debug.Log($"[ScoreboardManager] Trying to add score: {newName} - {newScore}");
        SaveScores();
    }

    public void LoadScores()
    {
        highScores.Clear();
        for (int i = 0; i < MaxScores; i++)
        {
            string nameKey = ScoreKeyPrefix + i + "_Name";
            string scoreKey = ScoreKeyPrefix + i + "_Score";

            string name = PlayerPrefs.GetString(nameKey, "");
            int score = PlayerPrefs.GetInt(scoreKey, -1);

            if (!string.IsNullOrWhiteSpace(name) && score > -1)
            {
                highScores.Add(new ScoreEntry(name, score));
            }
        }

        highScores.Sort((a, b) => b.score.CompareTo(a.score));
    }

    public void SaveScores()
    {
        for (int i = 0; i < MaxScores && i < highScores.Count; i++)
        {
            PlayerPrefs.SetString(ScoreKeyPrefix + i + "_Name", highScores[i].playerName);
            PlayerPrefs.SetInt(ScoreKeyPrefix + i + "_Score", highScores[i].score);
        }

        PlayerPrefs.Save();
        Debug.Log("✅ Scores saved successfully");

    }

    public void ResetScores()
    {
        for (int i = 0; i < MaxScores; i++)
        {
            PlayerPrefs.DeleteKey(ScoreKeyPrefix + i + "_Name");
            PlayerPrefs.DeleteKey(ScoreKeyPrefix + i + "_Score");
        }

        highScores.Clear();
        UpdateScoreboardDisplay(); // Clear the UI too
    }

    public List<ScoreEntry> GetTopScores()
    {
        return new List<ScoreEntry>(highScores);
    }

    public void UpdateScoreboardDisplay()
    {
        if (scoreboardText == null) return;

        scoreboardText.text = "";
        var topScores = GetTopScores();

        for (int i = 0; i < topScores.Count; i++)
        {
            scoreboardText.text += $"{i + 1}. {topScores[i].playerName} - {topScores[i].score}\n";
        }

        if (topScores.Count == 0)
        {
            scoreboardText.text = "No scores yet.";
        }
    }
}
