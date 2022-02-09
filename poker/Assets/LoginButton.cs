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


public class LoginButton : MonoBehaviour
{

    [Serializable]   
    public class MyClass
    {
         public string username;
         public string password;
        
    }

       
    public static string username;
    private string password;

    public GameObject error;
    public GameObject panel;
   
    public void Click(){
      
        MyClass myObject = new MyClass();
        myObject.username=username;
        myObject.password=password;

        string n = JsonUtility.ToJson(myObject);
        //Debug.Log(n);
        string adress="http://localhost:3010/login";

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
        Debug.Log(node["valid"]);

        if(node["valid"]==true){
            Debug.Log("Zalogowano");
            GameManager.username=username;
            SceneManager.LoadScene(3);
        }
        else{

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
