using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System.Text;
using System.Linq;

public class table : MonoBehaviour
{

    
    private string user_token;
    private string table_id;
    public static string table_port;

    public static string server_adress="vps.damol.pl:4000";

    public void JoinTable(){
        GameObject  child= this.transform.GetChild (0).gameObject;
        GameObject  port= this.transform.GetChild (2).gameObject;

        table_port = port.GetComponent<Text>().text;
        table_id = child.GetComponent<Text>().text;

        user_token=MainMenu.token;
         

        string adress= server_adress+"/game/"+table_id+"/join?token="+user_token;
        StartCoroutine(GetRequest(adress));
      
    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        if(node["valid"]){
            GameManager.user_token=user_token;
            GameManager.table_id=table_id;
            GameManager.player=false;
            GameManager.port=table_port;
            SceneManager.LoadScene(6);

        }      
    }

    IEnumerator GetRequest(string uri){        
        string n ="eeeeeeee";
        byte[] bytes = Encoding.ASCII.GetBytes(n);
        UnityWebRequest www = UnityWebRequest.Post(uri,n);
        UploadHandler uploader = new UploadHandlerRaw(bytes);

        www.uploadHandler = uploader;
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
