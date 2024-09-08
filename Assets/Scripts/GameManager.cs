using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public LevelManager levelManager;
    public ScoringSystem score;
    public EnemyManager enemyManager;
    public Camera mainCamera;

    public GameObject player;
    public GameObject passOverlay;
    public GameObject failOverlay;
    public GameObject hsList;
    public GameObject tuteOverlay;
    public GameObject hud;
    public GameObject magazine;
    public GameObject dropsArtHolder;

    public Tutorial tutorial;

    public AudioSource shootSound;
    public AudioSource explosion;
    public AudioSource select;
    public AudioSource won;
    public AudioSource lost;

    [HideInInspector] public bool wonLevel = false;

    bool playerDead = false;
    bool gameStart = false;
    bool wonPlayed = false;
    bool lostPlayed = false;

    // Start is called before the first frame update
    void Start()
    {
        // create the player offscreen first
        player = Instantiate(player, new Vector3(100, 100, 100), Quaternion.identity);
        player.GetComponent<Player>().GM = this;
        
        // initialise the level generation
        levelManager.InitLevelManager();
    }

    // Update is called once per frame
    void Update()
    {
        // start the game when the player has finished reading tutorial
        if (!gameStart)
            if (Input.GetKeyDown("z") && tutorial.state == STATE.POWERUPS)
            {
                select.Play();
                tuteOverlay.SetActive(false);
                hud.SetActive(true); 
                player.GetComponent<Player>().enabled = true;
                player.GetComponent<Player>().fired = true;
                levelManager.active = true;
                gameStart = true;
            }

        if (wonLevel)
        {
            // pauses the timer
            levelManager.active = false;

            // only do stuff after explosion is done
            if(!explosion.isPlaying)
            {
                // open the overlay and disable player firing and movement
                passOverlay.SetActive(true);
                player.GetComponent<Player>().enabled = false;

                if(!wonPlayed)
                {
                    won.Play();
                    wonPlayed = true;
                }

                // go to the next level
                if (Input.GetKeyDown("z") && !won.isPlaying)
                    GoNext();
            }
        }

        if (playerDead && !explosion.isPlaying)
        {
            // open the lose screen
            failOverlay.SetActive(true);

            // play lose audio once
            if (!lostPlayed)
            {
                lost.Play();
                lostPlayed = true;
            }

            // can reset when audio is done
            if (Input.GetKeyDown("z") && !lost.isPlaying)
            {
                select.Play();
                lostPlayed = false;
                ResetGame();
            }
        }
    }

    /// <summary>
    /// Update camera, player variables and score.
    /// Clean up leftover bullets.
    /// </summary>
    /// <param name="timerScore">Score to add based on timer.</param>
    public void LevelChanged(int timerScore)
    {
        // increases camera viewsize to fit the whole maze
        if (levelManager.currentLevel >= 2)
            mainCamera.orthographicSize++;

        player.GetComponent<Player>().OnLevelNext();
        score.UpdateScore(timerScore);

        // destroy all leftover bullets
        foreach (Transform child in magazine.transform)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Kills the player, then makes a new one. Updates scoring and pauses level timer before cleaning up leftover bullets.
    /// </summary>
    public void Lose()
    {
        // kill off the player
        player.GetComponent<GenericTank>().Die();
        playerDead = true;

        // instantiate the player off screen first
        player = Instantiate(player, new Vector3(100, 100, 100), Quaternion.identity);
        player.GetComponent<Player>().enabled = false;
        player.GetComponent<CircleCollider2D>().enabled = true;

        // run code within updatescore and save
        score.UpdateScore(0);
        score.SaveScore();
        // pause level timer
        levelManager.active = false;

        // destroy all leftover bullets
        foreach (Transform child in magazine.transform)
            Destroy(child.gameObject);

        // this bool is used to ensure that if there are multiple of the same score,
        // only the first instance will be displayed as the current score
        bool foundCurrent = false;
        for (int i = 0; i < score.highScoreSplit.Count; i++)
        {
            hsList.transform.GetChild(i).GetComponent<Text>().text = $"{score.highScoreSplit[i]}";
            if(score.highScoreSplit[i] == score.currentScore && !foundCurrent)
            {
                foundCurrent = true;
                hsList.transform.GetChild(i).GetComponent<HighScore>().glow = true;
            }
        }
    }
    
    /// <summary>
    /// Reset everything.
    /// </summary>
    void ResetGame()
    {
        // allow player movement again
        player.GetComponent<Player>().enabled = true;
        player.GetComponent<Player>().fired = true;

        // reset and update the score
        score.currentScore = 0;
        score.UpdateScore(0);

        // reset camera
        mainCamera.orthographicSize = 5.5f;

        // reset the level back to 1
        levelManager.ClearLevel();
        levelManager.InitLevelManager();

        // reset some variables
        failOverlay.SetActive(false);
        playerDead = false;
        levelManager.active = true;

        // reset highscore glow
        foreach (Transform child in hsList.transform)
            child.GetComponent<HighScore>().White();
    }

    /// <summary>
    /// Resets some variables and goes to the next level.
    /// </summary>
    void GoNext()
    {
        select.Play();
        
        // re-enable player
        player.GetComponent<Player>().enabled = true;
        player.GetComponent<Player>().fired = true;

        // reset bools
        wonLevel = false;
        wonPlayed = false;
        levelManager.active = true;

        // generate next level
        levelManager.GoNext();

        // disable overlay
        passOverlay.SetActive(false);
    }
}
