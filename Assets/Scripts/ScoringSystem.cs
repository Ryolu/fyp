using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoringSystem : MonoBehaviour
{
    public GameManager GM;
    public string highScores;
    public List<int> highScoreSplit;
    public int currentScore = 0;
    public Text highScoreText;
    public Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        // get highscore if it exists
        if (PlayerPrefs.HasKey("highscores"))
        {
            highScores = PlayerPrefs.GetString("highscores");
            highScoreSplit = highScores.Split(',').Select(int.Parse).ToList();
        }
        // otherwise, create a new empty list
        else
            highScoreSplit = new List<int> { 0, 0, 0, 0, 0 };

        // set the score UI stuff
        highScoreText.text = $"<{highScoreSplit[0]}>";
        scoreText.text = $"<{currentScore}>";
    }

    /// <summary>
    /// Update the current score by adding a value
    /// Visually updates the highscore as well if the current score exceeds the highest score achieved.
    /// </summary>
    /// <param name="value">Value to add to current score.</param>
    public void UpdateScore(int value)
    {
        // adds new value to the current 
        currentScore += value;

        // displays the current score as the high score if exceeded
        if (currentScore > highScoreSplit[0])
        {
            highScoreText.text = $"<{currentScore}>";
            scoreText.text = $"<{currentScore}>";
        }
        // otherwise, maintain status quo
        else
        {
            highScoreText.text = $"<{highScoreSplit[0]}>";
            scoreText.text = $"<{currentScore}>";
        }
    }

    /// <summary>
    /// Savse the current list of high scores into the playerprefs
    /// </summary>
    public void SaveScore()
    {
        // if the current score is higher than the lowest score in the list of highscores
        if (currentScore > highScoreSplit[highScoreSplit.Count - 1])
        {
            // then it will be added into the list of highscore for storage later
            highScoreSplit.Add(currentScore);
            highScoreSplit.Sort((a, b) => b.CompareTo(a));
            highScoreSplit.RemoveAt(highScoreSplit.Count - 1);
        }

        // concat the list of high scores and store the information as a string
        highScores = string.Join(",", highScoreSplit);
        PlayerPrefs.SetString("highscores", highScores);
    }
}
