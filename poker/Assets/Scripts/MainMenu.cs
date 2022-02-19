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
    public static string server_adress="vps.damol.pl:4000";

    void Start(){
        TableListMenu.token=token;
        RankingMenu.token=token;
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
        string adress= server_adress+"/game/create?token="+token;
        StartCoroutine(PutRequest(adress));
    }

    void Create_singleplayer_game(){
        GameManager.user_token=token;
        GameManager.player=true;
        string adress= server_adress+"/game/create/singleplayer/4?token="+token;
        Debug.Log("ADRESS");
        Debug.Log(adress);
        StartCoroutine(PutRequest(adress));
    }

    public void GameScene(){
        Create_game();
    }

    public void Singleplayer(){
        Create_singleplayer_game();

    }

    public void Logout(){
        string adress= server_adress+"/logout?token="+token;
        StartCoroutine(GetRequest(adress));
        SceneManager.LoadScene(0);

    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        string table_id="";
        int port=0;

        foreach(KeyValuePair<string, JSONNode> entry in node["result"]){

            if(entry.Key=="tableId"){
                table_id=entry.Value;
            }else{
                port=entry.Value;
            }
        }    
        GameManager.table_id=table_id;
        GameManager.port=port.ToString();
        SceneManager.LoadScene(6);   
    }

    void ProcessLogout(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
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

