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

    public static int token;
    public void JoinTable(){
        GameObject  child= this.transform.GetChild (0).gameObject;
        string table_id = child.GetComponent<Text>().text;
        //string user_token =MainMenu.token;
        string user_token="d5ecf435402745b4ac09c49948219514";
        Debug.Log(table_id);
        Debug.Log(user_token);

        string adress="http://localhost:3010/game/"+table_id+"/join?token="+user_token;
        StartCoroutine(GetRequest(adress));
        


        //c37a782515bd438c86c12bf39ab7ec8c/join?token=46d889e170e24cb3ac1baec65eac5b14"
        
    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
    }



    IEnumerator GetRequest(string uri){
        uri="http://localhost:3010/game/8be4bfd9a75b40b297621150f2bf3f26/join?token=d5ecf435402745b4ac09c49948219514";
        //"http://localhost:3010/game/7cd9d5f6b30c4e0da389e9e9be8d2a62/join?token=d5ecf435402745b4ac09c49948219514"
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
