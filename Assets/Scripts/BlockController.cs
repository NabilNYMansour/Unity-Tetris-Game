using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    Grid_ grid;
    public float downwardWaitTime = 0.5f;
    public float fastSpeed;
    public float SlowSpeed;
    float downwardFasterQuotient = 1;
    IEnumerator downwards;
    [NonSerialized]
    public bool landed;

    void MoveBlock(Vector2 towards)
    {
        gameObject.transform.localPosition = new Vector3((int)gameObject.transform.localPosition.x + towards.x, (int)gameObject.transform.localPosition.y + towards.y);
    }
    //---------------------------------------Downward_Movement---------------------------------------//
    IEnumerator MoveDownwards()
    {
        while (true)
        {
            MoveBlock(Vector2.down);
            yield return new WaitForSeconds((downwardWaitTime / downwardFasterQuotient) - grid.Accelerator);
        }
    }

    void TurnChildrenToBlocks()
    {
        foreach (Transform child in gameObject.transform)
        {
            child.tag = "Block";
        }
    }

    bool CheckCollision()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.localPosition.y <= 0)
            {
                Vector2 relativePosition = transform.localPosition + child.transform.localPosition;
                try
                {
                    if (grid.GridTracker[(int) relativePosition.x,(int)relativePosition.y])
                    {
                        gameObject.transform.localPosition += Vector3.up;
                        return true;
                    }
                }
                catch
                {
                    // Error caught.
                }
            }
        }

        return false;
    }

    bool HitGround()
    {
        int lowest = 10;
        foreach (Transform child in gameObject.transform)
        {
            if (child.transform.localPosition.y <= lowest)
            {
                lowest = (int)child.transform.localPosition.y;
            }
        }
        if (gameObject.transform.localPosition.y + lowest <= -1)
        {
            gameObject.transform.localPosition += Vector3.up;
            return true;
        }
        return false;
    }

    //-----------------------------------------------------------------------------------------------//

    //--------------------------------------------Rotation-------------------------------------------//
    int rotationCount;
    void Rotate()
    {
        if (rotationCount == 4)
        {
            rotationCount = 0;
        }
        Vector2 originalLocation = Vector2.zero;
        foreach (Transform child in gameObject.transform)
        {
            foreach (OriginalLocalLocation o in originals)
            {
                if (o.name == child.name)
                {
                    originalLocation = new Vector2(o.x, o.y);
                }
            }
            switch (rotationCount)
            {
                case 0: // 0 degrees.
                    child.transform.localPosition = new Vector3(originalLocation.x, originalLocation.y);
                    break;
                case 1: // 90 degrees.
                    child.transform.localPosition = new Vector3(-originalLocation.y, originalLocation.x);
                    break;
                case 2: // 180 degrees.
                    child.transform.localPosition = new Vector3(-originalLocation.x, -originalLocation.y);
                    break;
                case 3: // 270 degrees.
                    child.transform.localPosition = new Vector3(originalLocation.y, -originalLocation.x);
                    break;
            }
        }
    }

    void FixRotation(int direction)
    {
        int toCheckPos = 0; // By default, checking for left.
        int farthest = 0; // By default, the smallest possible farthest block is at local position 0.
        if (direction == 1) // But if right:
        {
            toCheckPos = 9;
        }
        foreach (Transform child in gameObject.transform)
        {
            if (Mathf.Abs(child.transform.localPosition.x) >= Mathf.Abs(farthest))
            {
                farthest = (int)child.transform.localPosition.x;
            }
        }
        // Fixing position:
        int relativeX = (int)gameObject.transform.localPosition.x + farthest;
        if (direction == -1)
        {
            if (relativeX < toCheckPos)
            {
                gameObject.transform.localPosition += Vector3.right * Mathf.Abs(farthest);
            }
        }
        else
        {
            if (relativeX > toCheckPos)
            {
                gameObject.transform.localPosition += Vector3.left * Mathf.Abs(farthest);
            }
        }
    }

    struct OriginalLocalLocation
    {
        public int x;
        public int y;
        public string name;

        public OriginalLocalLocation(int _x, int _y, string _name)
        {
            x = _x;
            y = _y;
            name = _name;
        }
    }

    List<OriginalLocalLocation> originals;
    void SetOriginalLocationOfBlocks()
    {
        foreach (Transform child in gameObject.transform)
        {
            originals.Add(new OriginalLocalLocation((int)child.transform.localPosition.x, (int)child.transform.localPosition.y, child.name));
        }
    }
    //-----------------------------------------------------------------------------------------------//

    //-------------------------------------------Strafing--------------------------------------------//
    bool StrafCollisionCheck(int direction) // direction = 1 for right, = -1 for left.
    {
        foreach (Transform child in gameObject.transform)
        {
            Vector2 relativePosition = transform.localPosition + child.transform.localPosition;
            if ((int)relativePosition.x - 1 >= 0 && (int)relativePosition.x + 1 <= 9) // Checking for both left and right to avoid any error.
            {
                if (grid.GridTracker[(int)relativePosition.x + direction, (int)relativePosition.y])
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool StrafEdgeCheck(int direction)
    {
        int toCheckPos = 0; // By default, checking for left.
        if (direction == 1) // But if right:
        {
            toCheckPos = 9;
        }
        foreach (Transform child in gameObject.transform)
        {
            Vector2 relativePosition = transform.localPosition + child.transform.localPosition;
            if ((int)relativePosition.x == toCheckPos)
            {
                return true;
            }
        }
        return false;
    }
    //-----------------------------------------------------------------------------------------------//

    void Start()
    {
        rotationCount = 0;
        originals = new List<OriginalLocalLocation>();
        landed = false;
        grid = gameObject.transform.parent.GetComponent<Grid_>();
        downwards = MoveDownwards();
        SetOriginalLocationOfBlocks();
        StartCoroutine(downwards);
    }

    void Update()
    {
        // Controls:
        if (!landed)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                rotationCount++;
                Rotate();
                FixRotation(-1);
                FixRotation(1);
            }
            if (Input.GetKeyDown(KeyCode.A)) // Move Left
            {
                if (!StrafCollisionCheck(-1) && !StrafEdgeCheck(-1))
                {
                    MoveBlock(Vector2.left);
                }
            }
            if (Input.GetKeyDown(KeyCode.D)) // Move Right
            {
                if (!StrafCollisionCheck(1) && !StrafEdgeCheck(1))
                {
                    MoveBlock(Vector2.right);
                }
            }
            if (Input.GetKey(KeyCode.S)) // Move Down faster
            {
                downwardFasterQuotient = fastSpeed;
            }
            else
            {
                downwardFasterQuotient = SlowSpeed;
            }

            // Checking for hit:
            if (CheckCollision() || HitGround())
            {
                StopCoroutine(downwards);
                TurnChildrenToBlocks();
                landed = true;
                grid.Score += 1;
            }
        }
    }
}
