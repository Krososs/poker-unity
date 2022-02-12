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
        Debug.Log("Token w menu:" + token);

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
        GameManager.user_token=token;
        GameManager.player=true;
        string adress="http://localhost:3010/game/create?token="+token;
        StartCoroutine(PutRequest(adress));
    }

    public void GameScene(){
        Create_game();
        //SceneManager.LoadScene(6);
    }

    public void Logout(){
        string adress="http://localhost:3010/logout?token="+token;
        StartCoroutine(GetRequest(adress));
        SceneManager.LoadScene(0);

    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        GameManager.table_id=node["result"];
        Debug.Log("Tworzę stół");
        Debug.Log(node["result"]);
        SceneManager.LoadScene(6);   
    }

    void ProcessLogout(string rawRespone){
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

    IEnumerator GetRequest(string uri){
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();

        if(www.result != UnityWebRequest.Result.Success){
           Debug.LogError("Something went wrong " + www.error);
           
           
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            ProcessLogout(www.downloadHandler.text);
        }

    }
}

