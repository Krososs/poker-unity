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
        string text = child.GetComponent<Text>().text;
        string user_token =MainMenu.token;
        Debug.Log(text);
        Debug.Log(user_token);
        
    }
    
    

}
