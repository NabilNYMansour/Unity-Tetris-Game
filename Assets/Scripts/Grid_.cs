using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_ : MonoBehaviour
{
    //----------------------------------------Grid_Controller----------------------------------------//
    public Vector2 gridSize; // The grid will be longer to accommodate spawning blocks
    public bool[,] GridTracker;

    void GridReset()
    {
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                GridTracker[i, j] = false;
            }
        }
    }

    private void BlockCheck()
    {
        GridReset();
        foreach (Transform t in gameObject.transform)
        {
            foreach (Transform Block in t.transform)
            {
                if (Block.tag == "Block")
                {
                    Vector2 relativePosition = t.localPosition + Block.transform.localPosition;
                    GridTracker[(int)relativePosition.x, (int)relativePosition.y] = true;
                }
            }
        }
    }

    public Transform point;
    void PointShower()
    {
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                if (GridTracker[i, j])
                {
                    Transform newPoint = Instantiate(point);
                    newPoint.parent = gameObject.transform;
                    newPoint.transform.localPosition = new Vector3(i, j);
                }
            }
        }
    }
    void PointRemover()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.tag == "Point")
            {
                Destroy(child.gameObject);
            }
        }
    }
    //-----------------------------------------------------------------------------------------------//

    //---------------------------------------Spawn_Controller----------------------------------------//
    public Transform[] BlockList;
    void SpawnBlock()
    {
        System.Random random = new System.Random();
        int randomInt = random.Next(0, BlockList.Length);
        Transform Block = BlockList[randomInt];
        Instantiate(Block, gameObject.transform);
        Block.localPosition = new Vector3(5, 23);
        Block.tag = "CurrentBlock";
    }

    Transform currentBlock;
    void FindCurrentBlock()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.tag == "CurrentBlock")
            {
                currentBlock = child;
            }
        }
    }
    //-----------------------------------------------------------------------------------------------//

    //-------------------------------------Full_Line_Controller--------------------------------------//
    public int Score;
    void FullLineCheck()
    {
        int toRemove = -1;
        bool Check = false;
        for (int y = 0; y < 20; y++) // for each y:
        {
            for (int x = 0; x < 10; x++)
            {
                if (GridTracker[x, y])
                {
                    Check = true;
                }
                else
                {
                    Check = false;
                    break;
                }
                if (Check && x == 9)
                {
                    toRemove = y;
                }
            }
            if (Check)
            {
                break;
            }
        }
        if (Check) // if full line:
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.GetComponent<BlockController>().landed)
                {
                    foreach (Transform Block in child.transform)
                    {
                        if (Block.transform.localPosition.y + child.transform.localPosition.y == toRemove) // Deleting the full line blocks.
                        {
                            Destroy(Block.gameObject);
                        }
                        if (Block.transform.localPosition.y + child.transform.localPosition.y > toRemove) // Dropping down the blocks.
                        {
                            Block.transform.localPosition += Vector3.down;
                        }
                    }
                    // Removing empty shapes:
                    if (child.childCount == 0)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            Score += 10;
        }
    }
    //-----------------------------------------------------------------------------------------------//

    //---------------------------------------Score_Controller----------------------------------------//
    public Transform ScoreDisplay;
    void UpdateScore()
    {
        ScoreDisplay.GetComponent<TextMesh>().text = "Score = " + Score.ToString();
    }

    bool gameStop;
    bool GameOver()
    {
        for (int x = 0; x < 10; x++)
        {
            if (GridTracker[x, 20])
            {
                return true;
            }
        }
        return false;
    }
    //-----------------------------------------------------------------------------------------------//

    public float Accelerator;
    void Start()
    {
        Accelerator = 0;
        Score = 0;
        gameStop = false;
        GridTracker = new bool[(int)gridSize.x, (int)gridSize.y];
        GridReset();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (!gameStop)
        {
            BlockCheck();
            FullLineCheck();
            UpdateScore();
            // Checking if game should end:
            if (GameOver())
            {
                gameStop = true;
                ScoreDisplay.transform.position = new Vector3(-3, 6) + (Vector3.forward * 2);
                ScoreDisplay.GetComponent<TextMesh>().text = $"Game Over!{System.Environment.NewLine}Score = " + Score.ToString();
            }
            // Spawning the block:
            if (currentBlock == null || currentBlock.GetComponent<BlockController>().landed)
            {
                SpawnBlock();
                FindCurrentBlock();
            }
            Accelerator += 0.000001f;
        }
        // For debugging purposes:
        if (Input.GetKeyDown(KeyCode.C)) // To check spots that are occupied within the grid.
        {
            PointShower();
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            PointRemover();
        }
    }
}