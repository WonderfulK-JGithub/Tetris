using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



/*Maneging the grid
 * also maneging the UI
 * and sceneTransitioning
 */



public class GridManager : MonoBehaviour
{
    public AudioManager audioManager;

    public  SmallCube[,] grid = new SmallCube[10, 20];

    GameObject activeBlock;
    GameObject currentBlock;
    GameObject holdBlock;
    bool AllowSwitching = true;
    
    /*
    public GameObject orangeBlock;
    public GameObject blueBlock;
    public GameObject redBlock;
    public GameObject greenBlock;
    public GameObject purpleBlock;
    public GameObject lightBlueBlock;
    public GameObject yellowBlock;
    */
    public GameObject[] cubeArray;

    List<GameObject> nextBlocks = new List<GameObject>();
    List<Sprite> nextSprites = new List<Sprite>();

    List<SmallCube> deleteCubes = new List<SmallCube>();
    List<int> lineMoves = new List<int>();

    public SpriteRenderer sprite0;
    public SpriteRenderer sprite1;
    public SpriteRenderer sprite2;

    public SpriteRenderer holdSprite;
    private void Start()
    {
        int random = Random.Range(0, 1);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[0].GetComponent<TetrisBlock>().representSprite);

        random = Random.Range(0, 1);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[1].GetComponent<TetrisBlock>().representSprite);

        random = Random.Range(0, 1);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[2].GetComponent<TetrisBlock>().representSprite);

        SpawnNewBlock();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(AllowSwitching)
            {
                SaveBlock();
            }
            else
            {
                audioManager.source.PlayOneShot(audioManager.errorClip);
            }
        }
    }
    //ingame Stuff
    #region

    void SaveBlock()
    {
        GameObject heldBlock = holdBlock;
        holdBlock = currentBlock;
        Destroy(activeBlock);
        if(heldBlock != null)
        {
            currentBlock = heldBlock;
            activeBlock = Instantiate(currentBlock);
        }
        else
        {
            currentBlock = nextBlocks[0];
            activeBlock = Instantiate(nextBlocks[0]);

            nextBlocks.RemoveAt(0);

            int random = Random.Range(0, 7);
            nextBlocks.Add(cubeArray[random]);

            UpdatePreview();
        }
        AllowSwitching = false;

        UpdateHold();
    }

    void SpawnNewBlock()
    {
        currentBlock = nextBlocks[0];
        activeBlock = Instantiate(nextBlocks[0]);

        nextBlocks.RemoveAt(0);

        int random = Random.Range(0, 7);
        nextBlocks.Add(cubeArray[random]);

        UpdatePreview();

        AllowSwitching = true;
    }
    public void CheckLines()
    {
       
        for (int i = 19; i > -1; i--)
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

        
        
        lineMoves.Add(i);
    }
    IEnumerator DeleteLines()
    {
        if (deleteCubes.Count > 0) audioManager.source.PlayOneShot(audioManager.lineClearClip);

        foreach (var cube in deleteCubes)
        {
            
            //do shader stuff here, for now this will do
            //"!#"!#"!#
            //!!!!!
            Destroy(cube.gameObject);
        }

        yield return null;

        deleteCubes.Clear();

        MoveLines();

        lineMoves.Clear();
    }
    void MoveLines()
    {

        foreach (var integer in lineMoves)
        {

            for (int i = integer + 1; i < 20; i++)
            {
                for (int a = 0; a < 10; a++)
                {
                    if (grid[a, i] != null)
                    {
                        grid[a, i - 1] = grid[a, i];
                        grid[a, i] = null;
                        grid[a, i - 1].transform.position += Vector3.down;
                    }

                }
            }
        }
        SpawnNewBlock();
    }
    #endregion



    //UI stuff

    void UpdatePreview()
    {
        nextSprites.RemoveAt(0);
        nextSprites.Add(nextBlocks[2].GetComponent<TetrisBlock>().representSprite);

        sprite0.sprite = nextSprites[0];
        sprite1.sprite = nextSprites[1];
        sprite2.sprite = nextSprites[2];
    }

    void UpdateHold()
    {
        holdSprite.sprite = holdBlock.GetComponent<TetrisBlock>().representSprite;
    }
}
