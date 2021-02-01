using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public  SmallCube[,] grid = new SmallCube[10, 20];

    public GameObject orangeBlock;

    private int lowestClear;

    List<SmallCube> deleteCubes = new List<SmallCube>();

    void SpawnNewBlock()
    {
        Instantiate(orangeBlock);
    }

    public void CheckLines()
    {
        lowestClear = 19;
        for (int i = 0; i < 20; i++)
        {
            CheckOneLine(i);
        }


        StartCoroutine(DeleteLines());
        
    }

    void CheckOneLine(int i)
    {
        for (int a = 0; a < 10; a++)
        {
            if (grid[a, i] == null) return;
        }
        for (int a = 0; a < 10; a++)
        {
            deleteCubes.Add(grid[a, i]);
            grid[a, i] = null;
        }

        if (lowestClear > i) lowestClear = i;
    }

    void MoveLines()
    {
        for (int i = lowestClear + 1; i < 20; i++)
        {
            for (int a = 0; a < 10; a++)
            {
                if(grid[a,i] != null)
                {
                    grid[a, i - 1] = grid[a, i];
                    grid[a, i] = null;
                    grid[a, i - 1].transform.position += Vector3.down;
                }
                
            }
        }

        

        SpawnNewBlock();
    }

    IEnumerator DeleteLines()
    {
        foreach (var cube in deleteCubes)
        {
            
            //do shader stuff here, for now this will do
            //"!#"!#"!#
            //!!!!!
            Destroy(cube.gameObject);
        }
        


        yield return null;

        MoveLines();
    }
}
