﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;



/*Maneging the grid
 * also maneging the UI
 * and sceneTransitioning
 * and file management
 */



public class GridManager : MonoBehaviour
{
    public float tetrisSpeed;

    public AudioManager audioManager;

    public  SmallCube[,] grid = new SmallCube[10, 20];
    
    GameObject activeBlock;
    GameObject currentBlock;
    GameObject holdBlock;
    bool allowSwitching = true;

    public int score = 0;
    public int level;
    int linesCleared = 0;
    public float dissolveSpeed;

    public int highScore;
    public Text scoreText;
    public Text levelText;
    
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

    public GameObject gameOverScreen;
    public Image overlayImage;
    public Text thisScoreText;
    public Text highscoreText;

    public bool pause;
    bool allowPause = true;
    public Text pauseText;

    private void Start()
    {
        int random = Random.Range(0, 7);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[0].GetComponent<TetrisBlock>().representSprite);

        random = Random.Range(0, 7);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[1].GetComponent<TetrisBlock>().representSprite);

        random = Random.Range(0, 7);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[2].GetComponent<TetrisBlock>().representSprite);

        SpawnNewBlock();

        Load();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(allowSwitching)
            {
                SaveBlock();
            }
            else
            {
                audioManager.source.PlayOneShot(audioManager.errorClip);
            }
        }

        if(Input.GetKeyDown(KeyCode.Return) && allowPause)
        {
            pause = !pause;
            overlayImage.enabled = !overlayImage.enabled;
            pauseText.enabled = !pauseText.enabled;
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
        allowSwitching = false;

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

        allowSwitching = true;
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
        if (deleteCubes.Count > 0)
        {
            audioManager.source.PlayOneShot(audioManager.lineClearClip);

            float time = 1;

            while (time > 0)
            {
                time -= dissolveSpeed;
                if (time < 0) time = 0;
                foreach (var cube in deleteCubes)
                {
                    cube.rend.material.SetFloat("_time", time);
                }
                yield return null;
            }

            foreach (var cube in deleteCubes)
            {
                Destroy(cube.gameObject);
            }

            deleteCubes.Clear();
        }

        MoveLines();

        switch(lineMoves.Count)
        {
            case 1:
                score += 40;
                break;
            case 2:
                score += 100;
                break;
            case 3:
                score += 300;
                break;
            case 4:
                score += 1200;
                break;
        }
        if (lineMoves.Count != 0) UpdateScore();
        

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
        CheckLevel();
        SpawnNewBlock();
    }

    public void CheckLevel()
    {
        linesCleared += lineMoves.Count;
        if (linesCleared >= 10)
        {
            
            level++;
            linesCleared -= 10;
            UpdateLevel();

            if (level < 9)
            {
                tetrisSpeed -= 5;
            }
            else if(level == 9)
            {
                tetrisSpeed = 6;
            }
            else if(level > 13)
            {
                tetrisSpeed = 5;
            }
            else if(level > 16)
            {
                tetrisSpeed = 4;
            }
            else if(level > 19)
            {
                tetrisSpeed = 3;
            }
            else if(level > 29)
            {
                tetrisSpeed = 2;
            }
            else
            {
                tetrisSpeed = 1;
            }
        }
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

    public void UpdateScore()
    {
        scoreText.text = score.ToString();
    }

    void UpdateLevel()
    {
        levelText.text = level.ToString();
    }


    //ENDGAME
    public void GameOver()
    {
        if(score > highScore)
        {
            Save();
        }
        Load();

        allowSwitching = false;
        gameOverScreen.SetActive(true);
        overlayImage.enabled = true;

        thisScoreText.text += score.ToString();
        highscoreText.text += highScore.ToString();
    }

    public void Save()
    {
        SaveData nyData = new SaveData();

        nyData.bestScore = score;

        string nyJson = JsonUtility.ToJson(nyData);

        print(nyJson);

        File.WriteAllText(Application.persistentDataPath + "/save.txt", nyJson);
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/save.txt"))
        {
            string gammalJson = File.ReadAllText(Application.persistentDataPath + "/save.txt");

            SaveData gammalData = JsonUtility.FromJson<SaveData>(gammalJson);

            highScore = gammalData.bestScore;

            print(highScore);
        }
        else highScore = 0;
       
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
