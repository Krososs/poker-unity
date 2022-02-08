using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using SimpleJSON;
using System.Text;

/*
[Serializable]
public class Data{

    public string playerUsername;
    public string playerPassword;
}*/



public class LoginButton : MonoBehaviour
{

    [Serializable]   
    public class MyClass
    {
         public string username;
         public string password;
        
    }

       
    private string username;
    private string password;
   
    public void Click(){
       //name = usernameField.GetComponent<Text>().text;

        Debug.Log(username);
        Debug.Log(password);


        MyClass myObject = new MyClass();
        myObject.username=username;
        myObject.password=password;

        string n = JsonUtility.ToJson(myObject);
        Debug.Log(n);

        


        string adress="http://localhost:3010/login";

        StartCoroutine(GetRequest(adress,n)); 

       

    }



    public void setPassword(string s){
        password=s;
        Debug.Log(password);
    }

    public void setUSername(string s){
        username=s;
        Debug.Log(username);
    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);

    }

    IEnumerator GetRequest(string uri, string n){

        byte[] bytes = Encoding.ASCII.GetBytes(n);
        //string n="";

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
            Debug.Log(www.downloadHandler.text);
            ProcessServerRespone(www.downloadHandler.text);
        }

    }
    
    
}
