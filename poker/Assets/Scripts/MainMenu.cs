using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System.Text;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    public static string token;

    void Start(){
        TableListMenu.token=token;

    }

    public void RankingScene(){
        SceneManager.LoadScene(4);
    }
    public void TableListScene(){
        SceneManager.LoadScene(5);
    }
    public void ExitGame(){
        Application.Quit();
    }

    void Create_game(){
        string adress="http://localhost:3010/game/create?token="+token;
        StartCoroutine(PutRequest(adress));


    }

    public void GameScene(){
        Create_game();
        //SceneManager.LoadScene(6);

    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);   
    }

   IEnumerator PutRequest(string uri){
        string n ="n";
        byte[] bytes = Encoding.ASCII.GetBytes(n);
        UnityWebRequest www = UnityWebRequest.Put(uri,bytes);     
        yield return www.SendWebRequest();

        if(www.result != UnityWebRequest.Result.Success){
           Debug.LogError("Something went wrong " + www.error);         
        }
        else
        {
            ProcessServerRespone(www.downloadHandler.text);
        }

    }
}

