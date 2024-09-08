using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameManager GM;
    public Enemy enemyPrefab;
    public Enemy pEnemyPrefab;
    public Enemy eEnemyPrefab;
    public GameObject dropParent;

    List<Enemy> enemyList = new List<Enemy>();

    // Update is called once per frame
    void Update()
    {
        ClearCheck();
    }
    
    /// <summary>
    /// Spawn enemies based on arguments provided.
    /// </summary>
    /// <param name="spawnPoints">List of coords where enemies can spawn</param>
    /// <param name="spawnDir">List of directions from which to leave the spawn points.</param>
    public void SpawnEnemies(List<Vector3> spawnPoints, List<int> spawnDir)
    {
        for (int i = 0; i < spawnPoints.Count; ++i)
        {
            Enemy enemy;

            // spawns enemy type based on how many levels have been passed
            // earlier levels have easier enemies to fight
            float chance = Random.value;
            if(chance < (GM.levelManager.currentLevel - 1) * 0.05f)
                enemy = Instantiate(eEnemyPrefab, spawnPoints[i], Quaternion.identity, transform);
            else if (chance < (GM.levelManager.currentLevel - 1) * 0.1f)
                enemy = Instantiate(pEnemyPrefab, spawnPoints[i], Quaternion.identity, transform);
            else
                enemy = Instantiate(enemyPrefab, spawnPoints[i], Quaternion.identity, transform);

            // set the enemy variables
            enemy.gameObject.transform.rotation = Quaternion.Euler(0, 0, spawnDir[i]);
            enemy.dropParent = dropParent;
            enemy.dropArtHolder = GM.dropsArtHolder;
            enemy.magazine = GM.magazine;
            enemy.GetComponent<GenericTank>().sound = GM.explosion;
            enemyList.Add(enemy);
        }
    }

    /// <summary>
    /// Checks if all enemies have been cleared from the level.
    /// </summary>
    void ClearCheck()
    {
        // no more children
        if (transform.childCount < 1)
        {
            // clear leftover bullets
            foreach (Transform child in GM.magazine.transform)
                Destroy(child.gameObject);

            // tell GM that the level is cleared
            GM.wonLevel = true;
        }
    }

    /// <summary>
    /// Clear enemy list.
    /// </summary>
    public void Clear()
    {
        // clear the list of enemies to make way for the new ones
        enemyList.Clear();
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Adds a score to the current score.
    /// </summary>
    /// <param name="value">Score to add.</param>
    public void AddScore(int value)
    {
        if(GM.score)
            GM.score.UpdateScore(value);
    }
}
