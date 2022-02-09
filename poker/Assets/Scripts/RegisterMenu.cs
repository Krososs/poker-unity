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


public class RegisterMenu : MonoBehaviour
{

    [Serializable]   
    public class NewUser
    {
         public string username;
         public string email;
         public string password;        
    }
    

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

    public void Click(){

        if(password!=r_password){
            GameObject _error = Instantiate(error, new Vector3(0,0,0), Quaternion.identity);
            _error.transform.SetParent(panel.transform,false);
            _error.GetComponent<Text>().text = "Passwords do not match";
            

            Debug.Log("Hasła nie są identyczne");

        }
        else{
             NewUser user = new NewUser();
            user.username=username;
            user.password=password;
            user.email=email;

            Debug.Log(user.username);
            Debug.Log(user.email);
            Debug.Log(user.password);

        }

       
        
    }

  
}
