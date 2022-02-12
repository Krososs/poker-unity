using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using SimpleJSON;
using System.Text;
using UnityEngine.SceneManagement;


public class LoginMenu : MonoBehaviour
{

    [Serializable]   
    public class MyClass
    {
         public string username;
         public string password;
        
    }

       
    public static string username;
    public static string server_adress;
    private string password;

    public GameObject error;
    public GameObject panel;
   
    public void Click(){
        var hash = new Hash128();
        hash.Append(password);
        string HashedPass=hash.ToString();
     
        MyClass myObject = new MyClass();
        myObject.username=username;
        myObject.password=HashedPass;

        string n = JsonUtility.ToJson(myObject);
        //Debug.Log(n);
        string adress= server_adress+"/login";

        StartCoroutine(GetRequest(adress,n)); 
    }

    public void Back(){
        SceneManager.LoadScene(0);
    }

    public void setPassword(string s){
        password=s;
    }

    public void setUSername(string s){
        username=s;      
    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node["result"]["user_id"]);
        Debug.Log(node["valid"]);
        Debug.Log(node["result"]["token"]);

        if(node["valid"]==true){
            Debug.Log("Zalogowano");
            GameManager.username=username;
            GameManager.user_id=node["result"]["user_id"];
            MainMenu.token= node["result"]["token"];
            SceneManager.LoadScene(3);
        }
        else{

            Debug.Log(node);
            GameObject _error = Instantiate(error, new Vector3(0,0,0), Quaternion.identity);
            _error.transform.SetParent(panel.transform,false);
            _error.GetComponent<Text>().text = "Wrong username or password";
        }

    }

    IEnumerator GetRequest(string uri, string n){

        byte[] bytes = Encoding.ASCII.GetBytes(n);
        UnityWebRequest www = UnityWebRequest.Post(uri,"");
        UploadHandler uploader = new UploadHandlerRaw(bytes);

        
        uploader.contentType = "application/json";

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
