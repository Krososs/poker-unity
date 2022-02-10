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
    public void JoinTable(){
        GameObject  child= this.transform.GetChild (0).gameObject;

         table_id = child.GetComponent<Text>().text;
         user_token=MainMenu.token;

        string adress="http://localhost:3010/game/"+table_id+"/join?token="+user_token;
        StartCoroutine(GetRequest(adress));
      
    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        if(node["valid"]){
            Debug.Log("Użytkownik: "+user_token);
            Debug.Log("Dołącza do stołu: " +table_id);
            GameManager.username_token=user_token;
            GameManager.table_id=table_id;
            SceneManager.LoadScene(6);

        }else{
            Debug.Log(node);
        }
       
    }



    IEnumerator GetRequest(string uri){        
        string n ="eeeeeeee";
        byte[] bytes = Encoding.ASCII.GetBytes(n);
        UnityWebRequest www = UnityWebRequest.Post(uri,n);
        UploadHandler uploader = new UploadHandlerRaw(bytes);

        
        //uploader.contentType = "application/json";

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
