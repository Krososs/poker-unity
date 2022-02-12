using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Linq;


public class RegisterMenu : MonoBehaviour
{

    [Serializable]   
    public class NewUser
    {
         public string username;
         public string email;
         public string password;
         public string language="en";        
    }
    
    public static string server_adress;
    private string username;
    private string email;
    private string password;
    private string r_password;

    public GameObject error;
    public GameObject panel;



    public void setUsername(string s){
        username=s;
    }

    public void setEmail(string s){
        email=s;
    }

    public void setPassword(string s){
        password=s;
    }

    public void setRPassword(string s){
        r_password=s;
    }

    public void Back(){
        SceneManager.LoadScene(0);
    }


    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        if(node["valid"]==true){
             SceneManager.LoadScene(0);
        }
       

    }

    public void Click(){
       
        if(password!=r_password){
            GameObject _error = Instantiate(error, new Vector3(0,0,0), Quaternion.identity);
            _error.transform.SetParent(panel.transform,false);
            _error.GetComponent<Text>().text = "Passwords do not match";
            Debug.Log("Hasła nie są identyczne");

        }
        else{
            var hash = new Hash128();
            hash.Append(password);
            string HashedPass=hash.ToString();

            NewUser user = new NewUser();
            user.username=username;
            user.password=HashedPass;
            user.email=email;

            string adress=server_adress+"/register";
            Debug.Log(adress);
            string n = JsonUtility.ToJson(user);
            StartCoroutine(GetRequest(adress,n)); 

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
