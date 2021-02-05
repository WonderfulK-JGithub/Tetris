using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class HighscoreThingManagerStuff : MonoBehaviour
{
    Text text;
    int highScore;
    // Start is called before the first frame update
    void Start()
    {
        if (File.Exists(Application.persistentDataPath + "/save.txt"))
        {
            string gammalJson = File.ReadAllText(Application.persistentDataPath + "/save.txt");

            SaveData gammalData = JsonUtility.FromJson<SaveData>(gammalJson);

            highScore = gammalData.bestScore;

            print(highScore);
        }
        else highScore = 0;

        text = GetComponent<Text>();
        text.text += highScore.ToString();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
