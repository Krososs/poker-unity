using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void RankingScene(){
        SceneManager.LoadScene(4);
    }
    public void TableListScene(){
        SceneManager.LoadScene(5);
    }
    public void ExitGame(){
        Application.Quit();
    }
    public void GameScene(){
        SceneManager.LoadScene(6);

    }

}
