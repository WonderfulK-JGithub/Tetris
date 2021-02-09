using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    //banans bredd, 1 int = 1 unit
    public int stageWidth;

    //vilken sprite den representerar i preview
    public Sprite representSprite;

    //vilken punkt tetrominon ska rotera runt
    public Vector3 rotatePoint;

    //kolla funktion Smartrotation
    Vector3[] smartRotationVectors = new Vector3[4];


    //hur snabbt den faller, skaffar värde från gridmaneger
    float fallTime;

    //variabel som räknar tid
    float curTime;

    //hur mycket extra den ska åka snabbare när man trycker ner
    public float speedAmp;


    //variabel som kollar om man går åt vänster
    bool goLeft = false;

    //en separat variabel som också mäter tid
    float xTime = 0;

    //hur mycket xTime ska starta med när värdet återställs
    public float xTimebuffer;

    //hur snabbt man kan förlytta sig
    public float xSpeed;

    //Referenser till audioManager och gridManager, som kommer refereras till ganska ofta
    GridManager gridManager;
    AudioManager audioManager;

    //prefab för shadowObjektet (skuggan som visar vart tetromino kommer landa
    public GameObject shadowPrefab;

    //varabel som refererar till den skapade shadown
    GameObject shadow;

   
    void Start()
    {
        

        gridManager = FindObjectOfType<GridManager>();
        audioManager = FindObjectOfType<AudioManager>();


        //skapar shadow objektet
        shadow = Instantiate(shadowPrefab);
        UppdateShadow();

        //Importerar fallspeed från gridmanager
        fallTime = gridManager.tetrisSpeed;

        //Om den inte kan röra sig i start betyder det Game over
        if(!AllowMove())
        {
            gridManager.GameOver();
            Destroy(gameObject);
        }

        //kolla funktionen SmartRotation
        smartRotationVectors[0] = new Vector3(0, 1, 0);
        smartRotationVectors[1] = new Vector3(0, -1, 0);
        smartRotationVectors[2] = new Vector3(1, 0, 0);
        smartRotationVectors[3] = new Vector3(-1, 0, 0);

        
    }

    
    void Update()
    {
        //Ser till att man inte kan röra sig när spelet är pausat
        if(!gridManager.pause)
        {
            //left and right movement
            #region
            
            //anledningen till detta sätt att röra sig horizontalt är för att man 
            // ge effekten av att först röra sig ett steg och sen om man håller in ett tag rör man sig snabbt


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

                    if (xTime <= 0)
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
            if (Input.GetKeyDown(KeyCode.Z))
            {

                transform.RotateAround(transform.TransformPoint(rotatePoint), Vector3.forward, 90);

                if (!AllowMove())
                {
                    if (!SmartRotation())
                    {
                        transform.RotateAround(transform.TransformPoint(rotatePoint), Vector3.forward, -90);
                        audioManager.source.PlayOneShot(audioManager.errorClip);
                    }
                    else audioManager.source.PlayOneShot(audioManager.rotateClip);
                }
                else audioManager.source.PlayOneShot(audioManager.rotateClip);
                UppdateShadow();

            }
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.UpArrow))
            {

                transform.RotateAround(transform.TransformPoint(rotatePoint), Vector3.forward, -90);

                if (!AllowMove())
                {
                    if (!SmartRotation())
                    {
                        transform.RotateAround(transform.TransformPoint(rotatePoint), Vector3.forward, 90);
                        audioManager.source.PlayOneShot(audioManager.errorClip);
                    }
                    else audioManager.source.PlayOneShot(audioManager.rotateClip);
                }
                else audioManager.source.PlayOneShot(audioManager.rotateClip);
                UppdateShadow();

            }
            #endregion

            /* Rotation, horizontal rörelse och vertical rörelse kollar alltid efter funktionen AllowMove()
             * om AllowMove() är false vid rotation eller horizontal rörelse åker den bara tillbaka där den var innan
             * om AllowMove() är false vid vertical rörelse, se nedan, åker den tillbaka och man förlorar kontroll över tetrominon och en ny skapas
            */

            //lägger till tid
            curTime += 60 * Time.deltaTime;

            //kollar om det är dags att åka ner ett steg
            if (curTime >= (Input.GetKey(KeyCode.DownArrow) ? fallTime / speedAmp : fallTime))
            {
                transform.position += Vector3.down;
                if (!AllowMove())
                {
                    transform.position += Vector3.up;

                    //lägger till kuberna på GridManagerns grid
                    AddToGrid();
                    //säger åt gridmanagern att börja läsa av griden
                    gridManager.CheckLines();

                    //Gör barnan, alltså de fyra kuberna, föräldrarlösa (det känns fel att skriva sånt på svenska)
                    transform.DetachChildren();

                    //förstör objektet (behövs inte längre)
                    Destroy(gameObject);

                }

                //återställer tiden
                curTime = 0;
            }

            //KOllar om man trycker space, space knappen ska göra att blocket åker ner direkt och att man får extra poäng
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Landing();
            }
        }
        
    }

    //Gör det enklare att rotera
    // Tex om man är vid en vägg och inte kan rotera ska denna funktion kolla om man istället kan röra sig horizontalt ett steg och sedan rotera
    // Detta finns i moderna tetris spel och jag tycker det är jobbigt att spela utan
    bool SmartRotation()
    {
        //sparar positionen den hade innan rotationen
        Vector3 ogPos = transform.position;

        //en foreach loop som går igenom smartRotationVectors arrayen
        //Den ger tetrominon den positionen och kollar om rotationen är okej då
        foreach (var vec3 in smartRotationVectors)
        {
            transform.position = ogPos + vec3;
            if (AllowMove()) return true;
        }

        //om det kommer hit betyder det att det inte finns någon stans man kan rotera
        transform.position = ogPos;
        return false;
    }

    //dessa två förklarar sig själva
    void GoLeft()
    {
        transform.position += Vector3.left;
        if (!AllowMove()) transform.position += Vector3.right;

        UppdateShadow();
    }
    void GoRight()
    {
        transform.position += Vector3.right;
        if (!AllowMove()) transform.position += Vector3.left;

        UppdateShadow();
    }

    //Sker när space är nedtryckt
    void Landing()
    {
        //spela ljudeffekt
        audioManager.source.PlayOneShot(audioManager.bopClip);
        transform.position += Vector3.down;
        if(AllowMove())
        {
            transform.position += Vector3.up;
            Vector3 ogPos = transform.position;
            bool canFall = true;

            //canFall blir false så fort man har kolliderat med en kub
            while (canFall)
            {
                transform.position += Vector3.down;
                canFall = AllowMove();

                //Lägg till poäng
                gridManager.score++;
                gridManager.UpdateScore();
            }

            transform.position += Vector3.up;


            AddToGrid();
            gridManager.CheckLines();

            transform.DetachChildren();
            Destroy(gameObject);
            
        }
        else
        {
            //om man kommer hit betyder det att man tryckte på space precis innan blocket hade ramlat ner, vilket betyder att man inte ska få extra poäng

            transform.position += Vector3.up;


            AddToGrid();
            gridManager.CheckLines();

            transform.DetachChildren();
            Destroy(gameObject);
            
        }

        
    }

    //kollar om draget är tillåtet, dvs om den är utanför planen eller i någon annan kub
    bool AllowMove()
    {
        //går igenom alla 4 barn
        foreach (Transform child in transform)
        { 
            //kollar om kuben är utanför planen
            if (child.transform.position.x < 0f || child.transform.position.x > stageWidth || child.transform.position.y < 0f || child.transform.position.y > 20)
            {
                return false;
            }
            
        }
        foreach (Transform child in transform)
        {
            //kollar om kubens position redan har en index i gridmanagerns grid (alltså om det redan finns en kub i den rutan)
            int roundX = Mathf.RoundToInt(child.position.x - 0.5f);
            int roundY = Mathf.RoundToInt(child.position.y - 0.5f);
            if (gridManager.grid[roundX, roundY] != null)
            {
                return false;
            }
        }

        //kommer man hit betyder det att draget är tillåtet
        return true;
    }

    //sker när tetrominon har landat
    void AddToGrid()
    {
        //lägger till alla barnen i gridden
        foreach (Transform child in transform)
        {
            int roundX = Mathf.RoundToInt(child.position.x - 0.5f);
            int roundY = Mathf.RoundToInt(child.position.y - 0.5f);

            gridManager.grid[roundX, roundY] = child.GetComponent<SmallCube>();
        }
    }

    //updaterar skuggans position
    //använder sig av liknande logic för Landing funktionen
    void UppdateShadow()
    {
        shadow.transform.rotation = transform.rotation;

        int i = 0;
        Vector3 ogPos = transform.position;
        Vector3 newPos;
        bool canFall = true;


        while (canFall)
        {
            transform.position += Vector3.down;
            canFall = AllowMove();
            i++;
        }
        
        transform.position += Vector3.up;
        newPos = transform.position;
        transform.position = ogPos;
        shadow.transform.position = newPos;
    }

    //ser till att skuggan förstörs när tetrominon förstörs
    private void OnDestroy()
    {
        Destroy(shadow);
    }
}
