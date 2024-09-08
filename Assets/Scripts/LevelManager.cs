using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public GameManager GM;
    public GameObject mainWalls;
    public GameObject wallPrefab;
    public GameObject plantPrefab;
    public Text level;
    [HideInInspector] public int currentLevel;
    [HideInInspector] public bool active = false;
    public Text timeText;
    public float levelTimer = 1f;

    int levelWidth;
    bool[,] maze;
    Vector3 playerStartPos;
    List<Vector3> spawnPoint;
    List<int> spawnExit;
    readonly int[] dirList = { 0, 180, 90, 270};

    // Update is called once per frame
    void Update()
    {
        // count the timer down as the level is active
        if(active)
        {
            levelTimer -= Time.deltaTime;
            timeText.text = ((int)Math.Floor(levelTimer)).ToString();

            // lose if time has run out
            if (levelTimer < 0.1f)
                GM.Lose();
        }
    }

    /// <summary>
    /// Initiate the maze generation at level 1
    /// </summary>
    public void InitLevelManager()
    {
        currentLevel = 1;
        GenerateMaze();
    }

    /// <summary>
    /// Generate the maze
    /// </summary>
    void GenerateMaze()
    {
        // set the text, timer, spawnpoints and spawnpoint exit directions
        level.text = "LEVEL " + currentLevel;
        levelTimer = (currentLevel - 1) * 10 + 30.5f;
        spawnPoint = new List<Vector3>();
        spawnExit = new List<int>();

        // smallest level should be 5x5
        levelWidth = 5 + (currentLevel - 1) * 2;

        // outer walls
        int halfOuterSize = (levelWidth + 1) / 2;
        for (int x = -halfOuterSize; x <= halfOuterSize; ++x)
            for(int y = -halfOuterSize; y <= halfOuterSize; ++y)
                if(x == -halfOuterSize || x == halfOuterSize || y == -halfOuterSize || y == halfOuterSize)
                    Instantiate(wallPrefab,new Vector3(x, y, 0), Quaternion.identity, mainWalls.transform);

        // initiate player starting position
        Vector3 playerPos = InitPlayerPos();
        // create the maze in grid form
        InitMaze((int)playerPos.x, (int)playerPos.y);
        // instantiate the maze itself
        SpawnMaze(levelWidth/2);
    }
    
    /// <summary>
    /// Put the player at the edge of the maze and return this position
    /// </summary>
    /// <returns></returns>
    Vector3 InitPlayerPos()
    {
        int playerStartX = UnityEngine.Random.value > 0.5 ? levelWidth - 1 : 0;
        int playerStartY = UnityEngine.Random.value > 0.5 ? levelWidth - 1 : 0;
        playerStartPos = new Vector3(playerStartX, playerStartY, 0);
        GM.player.GetComponent<Player>().MovePlayer(playerStartX - levelWidth / 2, playerStartY - levelWidth / 2);
        return playerStartPos;
    }

    /// <summary>
    /// Create the 2D bool array and start creating paths.
    /// </summary>
    /// <param name="playerStartX">X coordinate of this starting position</param>
    /// <param name="playerStartY">Y coordinate of this starting position</param>
    void InitMaze(int playerStartX, int playerStartY)
    {
        // create a new 2D maze
        maze = new bool[levelWidth, levelWidth];

        // set everything to true, since true = wall
        for (int x = 0; x < levelWidth; ++x)
            for (int y = 0; y < levelWidth; ++y)
                maze[x, y] = true;

        // make paths recursively
        MakePathFrom(playerStartX, playerStartY);
    }

    /// <summary>
    /// Recursive function to create paths from a single point
    /// </summary>
    /// <param name="startX">Current point X coordinate</param>
    /// <param name="startY">Current point Y coordinate</param>
    void MakePathFrom(int startX, int startY)
    {
        // set this cell to false
        maze[startX, startY] = false;

        // random direction
        List<Vector2Int> directions = new List<Vector2Int> {
            new Vector2Int(0, 2), 
            new Vector2Int(0, -2),
            new Vector2Int(-2, 0),
            new Vector2Int(2, 0),
        };
        Shuffle(directions);

        // move in a random direction as long as it is within bounds
        foreach (var dir in directions)
        {
            int nx = startX + dir.x;
            int ny = startY + dir.y;

            if (CheckBounds(nx, ny) && maze[nx, ny])
            {
                maze[startX + dir.x / 2, startY + dir.y / 2] = false;
                MakePathFrom(nx, ny);
            }
        }
    }

    /// <summary>
    /// Returns true if the point is within the grid, else it returns false
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns></returns>
    bool CheckBounds(int x, int y)
    {
        return x >= 0 && x < levelWidth && y >= 0 && y < levelWidth;
    }

    /// <summary>
    /// Generic method for shuffling to be used for the directions
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="list">List to be shuffled</param>
    void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Instantiate the individual parts of the maze
    /// </summary>
    /// <param name="halfLevelWidth"></param>
    void SpawnMaze(float halfLevelWidth)
    {
        for (int x = 0; x < levelWidth; ++x)
            for (int y = 0; y < levelWidth; ++y)
            {
                // for walls
                if (maze[x, y])
                {
                    // random chance of plants spawning
                    bool isPlant = UnityEngine.Random.value < 0.01f;

                    // check if the cardinal directions have walls on opposite sides,
                    // prevents plants from appearing in the corners
                    bool horizontal = (!CheckBounds(x - 1, y) || maze[x - 1, y]) && (!CheckBounds(x + 1, y) || maze[x + 1, y]);
                    bool vertical = (!CheckBounds(x, y - 1) || maze[x, y - 1]) && (!CheckBounds(x, y + 1) || maze[x, y + 1]);
                    if (isPlant && (horizontal || vertical))
                        Instantiate(plantPrefab, new Vector3(x - halfLevelWidth, y - halfLevelWidth, 0), Quaternion.identity, mainWalls.transform);
                    else
                        Instantiate(wallPrefab, new Vector3(x - halfLevelWidth, y - halfLevelWidth, 0), Quaternion.identity, mainWalls.transform);
                }
                else
                {
                    int howManyWalls = 0;
                    // up down left right
                    bool[] exitDir = { true, true, true, true };

                    // if tile is at the edge
                    if(x == 0 || x == levelWidth - 1 || y == 0 || y == levelWidth - 1)
                    {
                        // left edge
                        if(x == 0)
                        {
                            // set exit to false if its a wall and add to wall
                            if (CheckBounds(x, y + 1) && maze[x, y + 1]) { howManyWalls++; exitDir[0] = false; }
                            if (CheckBounds(x, y - 1) && maze[x, y - 1]) { howManyWalls++; exitDir[1] = false; }
                            if (CheckBounds(x + 1, y) && maze[x + 1, y]) { howManyWalls++; exitDir[3] = false; }

                            // since its already at the left edge
                            // left is not an exit
                            exitDir[2] = false;
                            howManyWalls++;
                        }
                        // right edge
                        if (x == levelWidth - 1)
                        {
                            // set exit to false if its a wall and add to wall
                            if (CheckBounds(x, y + 1) && maze[x, y + 1]) { howManyWalls++; exitDir[0] = false; }
                            if (CheckBounds(x, y - 1) && maze[x, y - 1]) { howManyWalls++; exitDir[1] = false; }
                            if (CheckBounds(x - 1, y) && maze[x - 1, y]) { howManyWalls++; exitDir[2] = false; }

                            // since its already at the right edge
                            // right is not an exit
                            exitDir[3] = false;
                            howManyWalls++;
                        }
                        // bottom edge
                        if (y == 0)
                        {
                            // set exit to false if its a wall and add to wall
                            if (CheckBounds(x, y + 1) && maze[x, y + 1]) { howManyWalls++; exitDir[0] = false; }
                            if (CheckBounds(x - 1, y) && maze[x - 1, y]) { howManyWalls++; exitDir[2] = false; }
                            if (CheckBounds(x + 1, y) && maze[x + 1, y]) { howManyWalls++; exitDir[3] = false; }

                            // since its already at the bottom edge
                            // bottom is not an exit
                            exitDir[1] = false;
                            howManyWalls++;
                        }
                        // top edge
                        if (y == levelWidth - 1)
                        {
                            // set exit to false if its a wall and add to wall
                            if (CheckBounds(x, y - 1) && maze[x, y - 1]) { howManyWalls++; exitDir[1] = false; }
                            if (CheckBounds(x - 1, y) && maze[x - 1, y]) { howManyWalls++; exitDir[2] = false; }
                            if (CheckBounds(x + 1, y) && maze[x + 1, y]) { howManyWalls++; exitDir[3] = false; }

                            // since its already at the left edge
                            // left is not an exit
                            exitDir[0] = false;
                            howManyWalls++;
                        }
                    }
                    else
                    {
                        if (CheckBounds(x, y + 1) && maze[x, y + 1]) { howManyWalls++; exitDir[0] = false; }
                        if (CheckBounds(x, y - 1) && maze[x, y - 1]) { howManyWalls++; exitDir[1] = false; }
                        if (CheckBounds(x - 1, y) && maze[x - 1, y]) { howManyWalls++; exitDir[2] = false; }
                        if (CheckBounds(x + 1, y) && maze[x + 1, y]) { howManyWalls++; exitDir[3] = false; }
                    }

                    // if this space is empty and is surrounded by 3 walls, its a dead end
                    // can spawn enemy here
                    if (howManyWalls >= 3)
                    {
                        //Debug.Log("Dead end found at: " + new Vector3(x, y, 0));
                        spawnPoint.Add(new Vector3(x, y, 0));
                        // find the exit direction and add it to the list
                        for (int i = 0; i < exitDir.Length; i++)
                            if (exitDir[i])
                            {
                                spawnExit.Add(dirList[i]); 
                                break;
                            }
                    }
                }
            }

        PopulateMaze();
    }

    /// <summary>
    /// Spawn the stuff that's supposed to be inside the maze.
    /// </summary>
    void PopulateMaze()
    {
        // remove the player spawn point from the list of potential spawn points
        // then only keep the number of spawn points = current level
        // send the list of spawn points to enemy manager after adjusting to world coords
        int index = spawnPoint.IndexOf(playerStartPos);
        spawnPoint.RemoveAt(index);
        GM.player.transform.eulerAngles = new Vector3(0, 0, spawnExit[index]);
        spawnExit.RemoveAt(index);
        if (spawnPoint.Count == 0) GenerateMaze();
        List<Vector3> newList = new List<Vector3>();
        int minimum = Mathf.Min(spawnPoint.Count, currentLevel);
        for (int i = 0; i < minimum; ++i)
            newList.Add(MazeToWorld(spawnPoint[i], levelWidth / 2));
        spawnPoint = newList;
        spawnExit.RemoveRange(minimum, spawnExit.Count - minimum);
        GM.enemyManager.SpawnEnemies(spawnPoint, spawnExit);
    }

    /// <summary>
    /// Converts grid coordinates to world coordinates
    /// </summary>
    /// <param name="pos">Vector3 position</param>
    /// <param name="halfLevelWidth">Half of the level width</param>
    /// <returns></returns>
    Vector3 MazeToWorld(Vector3 pos, float halfLevelWidth)
    {
        return new Vector3(pos.x - halfLevelWidth, pos.y - halfLevelWidth, 0);
    }

    /// <summary>
    /// Clear everything in the level, including walls, spawn points and exits as well as enemies.
    /// </summary>
    public void ClearLevel()
    {
        foreach (Transform child in mainWalls.transform)
            Destroy(child.gameObject);
        spawnPoint.Clear();
        spawnExit.Clear();
        GM.enemyManager.Clear();
    }

    /// <summary>
    /// Go to the next level.
    /// </summary>
    public void GoNext()
    {
        ClearLevel();
        // add bonus score according to time remaining
        GM.LevelChanged((int)Math.Floor(levelTimer) * 10);
        currentLevel++;
        GenerateMaze();
    }
}
