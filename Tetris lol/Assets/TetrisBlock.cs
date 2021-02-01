using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    public int stageWidth;

    public Vector3 rotatePoint;

    //vertical
    public float fallTime;
    float curTime;
    public float speedAmp;

    //horizontal
    bool goLeft = false;
    float xTime = 0;
    public float xTimebuffer;
    public float xSpeed;

    GridManager gridManager;

    // Start is called before the first frame update
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //left and right movement
        #region
        /*
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left;
            if (!AllowMove()) transform.position += Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += Vector3.right;
            if (!AllowMove()) transform.position += Vector3.left;
        }
        */
        xTime -= Time.deltaTime * 1; 
        if (goLeft)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                goLeft = false;
                GoRight();
                xTime = xTimebuffer;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
               
                if(xTime <= 0)
                {
                    xTime = xSpeed;
                    GoLeft();
                }
            }
            else xTime = 0f;

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                goLeft = true;
                GoLeft();
                xTime = xTimebuffer;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                
                if (xTime <= 0)
                {
                    xTime = xSpeed;
                    GoRight();
                }
            }
            else xTime = 0f;
        }

        #endregion
        //rotation
        #region 
        if (Input.GetKeyDown(KeyCode.X))
        {
            
            transform.RotateAround(transform.TransformPoint(rotatePoint), Vector3.forward, 90);

            if (!AllowMove()) transform.RotateAround(transform.TransformPoint(rotatePoint), Vector3.forward, -90);

        }
        if (Input.GetKeyDown(KeyCode.Z))
        {

            transform.RotateAround(transform.TransformPoint(rotatePoint), Vector3.forward, -90);

            if (!AllowMove()) transform.RotateAround(transform.TransformPoint(rotatePoint), Vector3.forward, 90);

        }
        #endregion

        curTime += 1 * Time.deltaTime;

        if (curTime >= (Input.GetKey(KeyCode.DownArrow) ? fallTime / speedAmp : fallTime))
        {
            transform.position += Vector3.down;
            if (!AllowMove())
            {
                if (!AllowMove()) transform.position += Vector3.up;
                this.enabled = false;
                
                AddToGrid();
                gridManager.CheckLines();
            }
            curTime = 0;
        }
    }

    void GoLeft()
    {
        transform.position += Vector3.left;
        if (!AllowMove()) transform.position += Vector3.right;
    }
    void GoRight()
    {
        transform.position += Vector3.right;
        if (!AllowMove()) transform.position += Vector3.left;
    }

    bool AllowMove()
    {
        foreach (Transform child in transform)
        { 
            if (child.transform.position.x < 0f || child.transform.position.x > stageWidth || child.transform.position.y < 0f)
            {
                return false;
            }
            
        }
        foreach (Transform child in transform)
        {
            int roundX = Mathf.RoundToInt(child.position.x - 0.5f);
            int roundY = Mathf.RoundToInt(child.position.y - 0.5f);
            if (gridManager.grid[roundX, roundY] != null)
            {
                return false;
            }
        }
        return true;
    }

    void AddToGrid()
    {
        foreach (Transform child in transform)
        {
            int roundX = Mathf.RoundToInt(child.position.x - 0.5f);
            int roundY = Mathf.RoundToInt(child.position.y - 0.5f);

            gridManager.grid[roundX, roundY] = child.GetComponent<SmallCube>();
        }
    }
}
