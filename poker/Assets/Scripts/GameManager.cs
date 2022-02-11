using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using SimpleJSON;
using UnityEngine.SceneManagement;
using System.Text;
using NativeWebSocket;


public class GameManager : MonoBehaviour
{   
    public GameObject[] user_bets; //poke z chipami + ilością postawionych
    public GameObject[] user_nickname; //pole z nickname + ilość żetonów
    public GameObject[] user_chips;
    public GameObject panel;
    public GameObject small_bet;
    public GameObject medium_bet;
    public GameObject big_bet;
    public GameObject text;
    public GameObject nickname;
    public GameObject chips;
    public GameObject specators_amount;
    public GameObject specators_panel;


    

    public static string username;
    public static string username_token;
    public static string table_id;
    public static string user_id;


    public static bool player; 

    private string state_adress;
    private string sit_adress;

    public static GameManager Instance;

    public GameState State;
    WebSocket websocket;

    public static event Action<GameState> OnGameStateChanged;
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void Awake() {
        
        Instance=this;
        state_adress="http://localhost:3010/game/"+table_id+"/state?token="+username_token;
        sit_adress="http://localhost:3010/game/"+table_id+"/sit_down?token="+username_token;

        if(player){
            GameObject _nickname = Instantiate(nickname, new Vector3(0,0,0), Quaternion.identity);
            _nickname.transform.SetParent(user_nickname[6].transform,false);
            _nickname.GetComponent<Text>().text = username;

        }
             
    }

     void Start(){

        SetupWebSocket();
        Debug.Log("Witam gracza");
        Debug.Log(username);
        Debug.Log("O numerze id");
        Debug.Log(user_id);
        Debug.Log("Przy stole");
        Debug.Log(table_id);

        //PrintGameState();
        //SetUserData();
        //all_bet();
    }

    void Update() {

         #if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
        #endif
      
    }
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// WEB

    async void SetupWebSocket(){
         websocket = new WebSocket("ws://localhost:3010");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            //GetState
            StartCoroutine(GetRequest(state_adress));
            
            Debug.Log("OnMessage!");           
            //getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);
        };

    // waiting for messages
        await websocket.Connect();
    }

    void ProcessSitRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);

    }

    void ProcessStateRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        Debug.Log(node["result"]); 
        Debug.Log("Players");
        int i=0;
        foreach( KeyValuePair<string, JSONNode> entry in node["result"]["players"])
        {
            
            Debug.Log(entry.Key);       
            Debug.Log(entry.Value);
            
            GameObject _nickname = Instantiate(nickname, new Vector3(0,0,0), Quaternion.identity);
            GameObject _chips = Instantiate(chips, new Vector3(0,0,0), Quaternion.identity);

            if(entry.Key==user_id){
                //_nickname.transform.SetParent(user_nickname[6].transform,false);
                _chips.transform.SetParent(user_chips[6].transform,false);
                _chips.GetComponent<Text>().text ="";                
                _chips.GetComponent<Text>().text = entry.Value["wallet"];
                

            }else if(i!=6){
                _nickname.transform.SetParent(user_nickname[i].transform,false);
                _chips.transform.SetParent(user_chips[i].transform,false);
                _nickname.GetComponent<Text>().text = entry.Value["username"];
                _chips.GetComponent<Text>().text = entry.Value["wallet"];             
            }
            Debug.Log(entry.Value["username"]);
            Debug.Log(i);
            i+=1;                     
        }
        i=0;   
        foreach( KeyValuePair<string, JSONNode> entry in node["result"]["spectators"])
        {
            
            Debug.Log("specator");
            Debug.Log(entry.Key);       
            Debug.Log(entry.Value);
            i+=1;          
            // GameObject _nickname = Instantiate(nickname, new Vector3(0,0,0), Quaternion.identity);
            // GameObject _chips = Instantiate(chips, new Vector3(0,0,0), Quaternion.identity);

            // if(entry.Key==user_id){
            //     //_nickname.transform.SetParent(user_nickname[6].transform,false);
            //     _chips.transform.SetParent(user_chips[6].transform,false);
            //     _chips.GetComponent<Text>().text ="";                
            //     _chips.GetComponent<Text>().text = entry.Value["wallet"];
                

            // }else if(i!=6){
            //     _nickname.transform.SetParent(user_nickname[i].transform,false);
            //     _chips.transform.SetParent(user_chips[i].transform,false);
            //     _nickname.GetComponent<Text>().text = entry.Value["username"];
            //     _chips.GetComponent<Text>().text = entry.Value["wallet"];             
            // }
            // Debug.Log(entry.Value["username"]);
            // Debug.Log(i);
            // i+=1;                     
        }

        Debug.Log("Specators in game: " +i.ToString());
       // GameObject sPanel = Instantiate(specators_panel, new Vector3(0,0,0), Quaternion.identity);
        GameObject spectators = Instantiate(specators_amount, new Vector3(0,0,0), Quaternion.identity);
        spectators.transform.SetParent(specators_panel.transform,false);
        spectators.GetComponent<Text>().text ="";
        string amount = i.ToString();
        spectators.GetComponent<Text>().text = amount; 
    }


    IEnumerator GetRequest(string uri){
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();

        if(www.result != UnityWebRequest.Result.Success){
           Debug.LogError("Something went wrong " + www.error);
           
        }
        else
        {                    
            ProcessStateRespone(www.downloadHandler.text);        
        }

    }

    IEnumerator PostRequest(string uri){        
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
            ProcessSitRespone(www.downloadHandler.text);
        }

    }

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//GAME


    public void HandleState(){
        string adress="http://localhost:3010/game/3f50a38b923f48ebad12f17e11e96373/state?token=2f76a09f0ce64fb2acd7b3159bb8a7d8";
        StartCoroutine(GetRequest(state_adress));
    }


    public void Sit(){
        Debug.Log(sit_adress);
        StartCoroutine(PostRequest(sit_adress));
        
    }

    public void GetUp(){

    }

    public void LeaveTable(){

    }

  
    

    // void SetUserData(){

    //     for (int i =0; i<user_nickname.Length; i++){
    //         GameObject _nickname = Instantiate(nickname, new Vector3(0,0,0), Quaternion.identity);
    //         GameObject _chips = Instantiate(chips, new Vector3(0,0,0), Quaternion.identity);

    //         _nickname.transform.SetParent(user_nickname[i].transform,false);
    //         _chips.transform.SetParent(user_chips[i].transform,false);

    //         if (i == 6)
    //             _nickname.GetComponent<Text>().text = username;               
    //         else
    //             _nickname.GetComponent<Text>().text = "DamolPL";
                

    //         _chips.GetComponent<Text>().text = "99999";            
    //     }
               
    // }
   
   public void UpdateGameState(GameState newState){
       State=newState;

       switch(newState){

           case GameState.Draw_test:
                break;
            case GameState.Add_money:
                break;
            case GameState.Start_game:
                break;
            default:
                Debug.Log("Popsułeś");
                break;
        }

        OnGameStateChanged?.Invoke(newState);
   }

    //funkcja testowa 
    void all_bet(){

        for (int i =0; i<user_bets.Length; i++){
            GameObject bet = Instantiate(big_bet, new Vector3(0,0,0), Quaternion.identity);
            GameObject txt = Instantiate(text, new Vector3(0,0,0), Quaternion.identity);
            bet.transform.SetParent(user_bets[i].transform,false);
            txt.transform.SetParent(user_bets[i].transform,false);
            txt.GetComponent<Text>().text = "14567";
            //Debug.Log("Dodaje");
        }

    }

    
}

public enum GameState {
    Start_game,
    Draw_test,
    Add_money
}
