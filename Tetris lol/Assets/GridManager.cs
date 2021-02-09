using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;


//låtsas som detta bara är svenska
/*Maneging the grid
 * also maneging the UI
 * and sceneTransitioning
 * and file management
 */



public class GridManager : MonoBehaviour
{
    //hur snabbt tetrominona ska falla
    public float tetrisSpeed;

    //referense till AudioManagern
    public AudioManager audioManager;

    //THE GRID, som håller koll på alla små kuber
    //Kuberna är inte sparade som GameObjects, utan som SmallCube instances
    //Detta är för att jag ska kunna accessa deras script
    public  SmallCube[,] grid = new SmallCube[10, 20];
    
    //referense till den senaste skapade tetrominon, NOTE inte samma sak som current block
    GameObject activeBlock;

    //Denna variabel sparar prefaben som den senaste skapade tetrominon använde sig av, NOTE inte samma sak som active block
    GameObject currentBlock;

    //Prefab på vilket block man har sparat
    GameObject holdBlock;

    //bool som kontrollerar om man kan spara ett block eller inte
    bool allowSwitching = true;

    //score och level säger sig självt
    public int score = 0;
    public int level;

    //hur många lines man clearat, återsätts vid levelup
    int linesCleared = 0;

    //hur snabbt kuberna ska försvinna när linen blir clearad
    public float dissolveSpeed;

    //säger sig självt
    public int highScore;

    //referens till text
    public Text scoreText;
    public Text levelText;
    
    //Array med alla tetromino prefabs
    public GameObject[] cubeArray;

    //lista för alla block i previewn, och en lista för deras sprites
    List<GameObject> nextBlocks = new List<GameObject>();
    List<Sprite> nextSprites = new List<Sprite>();

    //lista för vilka cubes som ska tas bort
    List<SmallCube> deleteCubes = new List<SmallCube>();

    //lista för vilka lines som har tagits bort
    List<int> lineMoves = new List<int>();

    //referense till spriterenderers för previews
    public SpriteRenderer sprite0;
    public SpriteRenderer sprite1;
    public SpriteRenderer sprite2;

    //samma som ovan fast för hold/ soarad tetromino
    public SpriteRenderer holdSprite;

    //referense till gameoverscreen och till en image som ska göra allt utom gameoverscreenen lite mörkare
    public GameObject gameOverScreen;
    public Image overlayImage;

    //referense till text för nuvarande score och highscore
    public Text thisScoreText;
    public Text highscoreText;

    //variabel för om spelet är pausat och en för om man får pausa
    public bool pause;
    bool allowPause = true;

    //referense till text som visas när spelet är pausat
    public Text pauseText;

    private void Start()
    {
        //lägger till 3 slumpmässiga objekt i previewn, objekt och sprites
        int random = Random.Range(0, 7);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[0].GetComponent<TetrisBlock>().representSprite);

        random = Random.Range(0, 7);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[1].GetComponent<TetrisBlock>().representSprite);

        random = Random.Range(0, 7);
        nextBlocks.Add(cubeArray[random]);
        nextSprites.Add(nextBlocks[2].GetComponent<TetrisBlock>().representSprite);

        //skapar nytt block
        SpawnNewBlock();

        //hämtar highscore från JSON fil
        Load();
    }

    private void Update()
    {
        //kollar om man tryckt på c och om man får spara ett block
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(allowSwitching)
            {
                SaveBlock();
            }
            else
            {
                //spelar ljudeffekt
                audioManager.source.PlayOneShot(audioManager.errorClip);
            }
        }

        //kollar om man tryckt på enter och om man får pausa
        if(Input.GetKeyDown(KeyCode.Return) && allowPause)
        {
            pause = !pause;

            //gör overlayimagen och pausetexten enabled/ disabled
            overlayImage.enabled = !overlayImage.enabled;
            pauseText.enabled = !pauseText.enabled;
        }
    }

    //ingame Stuff
    #region

    //när man sparar ett block
    void SaveBlock()
    {
        //referense till vilket block man hade sparat
        GameObject heldBlock = holdBlock;

        //sparar det blocket man kontrollerade senast (man sparar dens prefab)
        holdBlock = currentBlock;

        //förstör blocket man kontrollerade senast
        Destroy(activeBlock);

        //kollar om man hade sparat något innan, ibörjan har man ju inget block sparat
        if(heldBlock != null)
        {
            //sparar det nya blockets prefab
            currentBlock = heldBlock;

            //skapar det nya blocket
            activeBlock = Instantiate(currentBlock);
        }
        else
        {
            //gör samma sak som i Spawn block
            currentBlock = nextBlocks[0];
            activeBlock = Instantiate(nextBlocks[0]);

            nextBlocks.RemoveAt(0);

            int random = Random.Range(0, 7);
            nextBlocks.Add(cubeArray[random]);

            UpdatePreview();
        }

        //ser till att man inte får spara nästa gång, så att man inte bara kan trycka C hela tiden
        allowSwitching = false;

        //uppdatera UI
        UpdateHold();
    }

    //när ett nytt block skapas
    void SpawnNewBlock()
    {
        //sparar prefaben för den nya tetrominon
        currentBlock = nextBlocks[0];

        //skapar den nya tetrominon
        activeBlock = Instantiate(nextBlocks[0]);

        //tar bort den första i nextBlocks listan
        nextBlocks.RemoveAt(0);

        //lägger till en ny slumpmässig tetromino i nextBlocks listan
        int random = Random.Range(0, 7);
        nextBlocks.Add(cubeArray[random]);

        //uppdatera UI
        UpdatePreview();

        //ser till att man får spara
        allowSwitching = true;
    }

    //kallas när tetrominon har landat
    public void CheckLines()
    {
       //går igenom alla lines på planen
        for (int i = 19; i > -1; i--)
        {
            CheckOneLine(i);
        }


        StartCoroutine(DeleteLines());
        
    }

    //kollar en line
    void CheckOneLine(int i)
    {
        //kollar om en gridposition är tom i denna rad
        for (int a = 0; a < 10; a++)
        {
            if (grid[a, i] == null) return;
        }

        //om man kommer hit fanns det ingen tom position vilket betyder att denna line ska tasbort
        for (int a = 0; a < 10; a++)
        {
            //lägger till alla kuber som ska tas bort i deleteCube listan
            deleteCubes.Add(grid[a, i]);

            //tar bort kuberna från griden
            grid[a, i] = null;
        }

        
        //lägger till linens int position i lineMoves listan
        lineMoves.Add(i);
    }

    //tar bort lines
    IEnumerator DeleteLines()
    {
        //kollar om någon kub ska tas bort
        if (deleteCubes.Count > 0)
        {
            //spelar ljudeffekt
            audioManager.source.PlayOneShot(audioManager.lineClearClip);

            //time variabel wow
            float time = 1;


            while (time > 0)
            {
                time -= dissolveSpeed * Time.deltaTime * 60;
                if (time < 0) time = 0;

                //accessar kubernas shader och ändrar variabeln time, time kontrollerar dissolve effekten
                foreach (var cube in deleteCubes)
                {
                    cube.rend.material.SetFloat("_time", time);
                }
                yield return null;
            }

            //ta bort alla kuberna
            foreach (var cube in deleteCubes)
            {
                Destroy(cube.gameObject);
            }

            //cleara hela deleteCubes listan
            deleteCubes.Clear();
        }

        //förflytta lines

        MoveLines();

        //kollar hur många lines man clearade och ger poäng efter det
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
        if (lineMoves.Count != 0) UpdateScore(); //uppdaterar UI, om poäng läggs till
        
        //clearar lineMoves listan
        lineMoves.Clear();
    }

    //förflyttar lines
    void MoveLines()
    {
        //för varje line som har tagits bort ska for loopen användas
        foreach (var integer in lineMoves)
        {
            //for loopen utgår alltid från ett steg ovanför där linen togs bort, eftersom bara kuber ovanför line clearen ska flyttas ner
            for (int i = integer + 1; i < 20; i++)
            {
                for (int a = 0; a < 10; a++)
                {
                    //kollar om rutan som ska förlyttas inte är tom
                    if (grid[a, i] != null)
                    {
                        //ger rutans värde till rutan under
                        grid[a, i - 1] = grid[a, i];

                        //sätter rutans värde till null (tom)
                        grid[a, i] = null;

                        //flyttar på kuben, så att den har samma position i världen som i griden
                        grid[a, i - 1].transform.position += Vector3.down;
                    }

                }
            }
        }
        CheckLevel();
        SpawnNewBlock();
    }

    //kollar efter levelup
    public void CheckLevel()
    {
        //lägger till antalet clearade lines i linesCleared
        linesCleared += lineMoves.Count;

        //kollar om linesCleared är mer än eller lika med 10
        if (linesCleared >= 10)
        {
            //ökar level med 1
            level++;

            //tar bort 10 från linesCleared
            linesCleared -= 10;

            //uppdaterar UI
            UpdateLevel();

            //massa if statements som baserat på level gör att tetromino faller ner snabbare (mindre tetrisSpeed betyder att den åker ner snabbare)
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



  
    //uppdatera previewn
    void UpdatePreview()
    {
        //tar bort den första i nextSprites listan
        nextSprites.RemoveAt(0);

        //lägger till den tredje i nextBlocks i nextSprites
        nextSprites.Add(nextBlocks[2].GetComponent<TetrisBlock>().representSprite);

        //uppdaterar sprites
        sprite0.sprite = nextSprites[0];
        sprite1.sprite = nextSprites[1];
        sprite2.sprite = nextSprites[2];
    }

    //uppdatera holdspriten
    void UpdateHold()
    {
        holdSprite.sprite = holdBlock.GetComponent<TetrisBlock>().representSprite;
    }

    //uppdatera score och level text
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
        //kollar om score är mer än highscore
        if(score > highScore)
        {
            //sparar highScore
            Save();
        }
        //laddar highScore från JSON
        Load();

        //sätter allowSwitching och allowPause till false
        allowSwitching = false;
        allowPause = false;

        //eneblar gameOverScreen och overlayImage
        gameOverScreen.SetActive(true);
        overlayImage.enabled = true;

        //uppdaterar text i gameOverScreen
        thisScoreText.text += score.ToString();
        highscoreText.text += highScore.ToString();
    }

    //Sparar highScore i JSON
    public void Save()
    {
        //skapar ny instance av savedata 
        SaveData nyData = new SaveData();

        //sätter datans highscore till scoren man fick
        nyData.bestScore = score;

        //skapar en string
        string nyJson = JsonUtility.ToJson(nyData);

        print(nyJson);

        //skriver stringen till en fil
        File.WriteAllText(Application.persistentDataPath + "/save.txt", nyJson);
    }

    //laddar highScore i JSON
    public void Load()
    {
        //kollar om filen finns
        if (File.Exists(Application.persistentDataPath + "/save.txt"))
        {
            //skapar en string med info från filen
            string gammalJson = File.ReadAllText(Application.persistentDataPath + "/save.txt");

            //gör en ny SaveData som får info från stringen
            SaveData gammalData = JsonUtility.FromJson<SaveData>(gammalJson);

            //sätter highScore variabeln till SaveDatans highscore
            highScore = gammalData.bestScore;

            print(highScore);
        }
        else highScore = 0; //om filen inte finns är highscore 0
       
    }

    //knapp funktioner, laddar olika scener (0 = titlescreen)
    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
