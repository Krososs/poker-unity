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
    public GameObject[] user_bets; //user bet place
    public GameObject[] user_nickname; //user nickname
    public GameObject[] user_chips; // user amount of chips
    public GameObject panel; //user panel

    public GameObject small_bet; //small bet texture
    public GameObject medium_bet; //medium bet texture
    public GameObject big_bet; //big bet texture

    public GameObject bet_amount; //user bet amount

    public GameObject nickname; //user nickname
    public GameObject chips; //user amount of chips
    public GameObject specators_amount;
    public GameObject specators_panel;

    public Button status_button;

    public static string username;
    public static string username_token;
    public static string table_id;
    public static string user_id;

    public static bool player; 

    private string state_adress;
    private string sit_adress;
    private string status_adress;
    private string status="NOT_READY";

    public static GameManager Instance;

    enum PostRequestType{
        SIT,
        STATUS
    }

    public GameState State;
    WebSocket websocket;

    public static event Action<GameState> OnGameStateChanged;
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//UNITY

    void Awake() {
        
        Instance=this;
        state_adress="http://localhost:3010/game/"+table_id+"/state?token="+username_token;
        sit_adress="http://localhost:3010/game/"+table_id+"/sit_down?token="+username_token;
        status_adress="http://localhost:3010/game/"+table_id+"/player_status?token="+username_token+"&status=";
        //'http://localhost:3010/game/5e13d35320dc4e5e86fc8291669928af/player_status?token=f124c5a7332a42d188b851cc623f540d&status=READY'

        if(player){
            GameObject _nickname = Instantiate(nickname, new Vector3(0,0,0), Quaternion.identity);
            _nickname.transform.SetParent(user_nickname[6].transform,false);
            _nickname.GetComponent<Text>().text = username;

        }
             
    }

     void Start(){

        SetupWebSocket();
        GetState();
        Debug.Log("Witam gracza");
        Debug.Log(username);
        Debug.Log("O numerze id");
        Debug.Log(user_id);
        Debug.Log("Przy stole");
        Debug.Log(table_id);

        if(!player){
            DisableButtons();
        }

        

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
            //StartCoroutine(GetRequest(state_adress));
            GetState();

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

        if(node["valid"])
            player=true;

    }
    void ProcessStatusRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);

    }

    void ProcessStateRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        HandleState(node);       
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

    IEnumerator PostRequest(string uri, PostRequestType type){        
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
            switch(type){
                case PostRequestType.SIT:
                    ProcessSitRespone(www.downloadHandler.text);
                    break;
                case PostRequestType.STATUS:
                    ProcessStatusRespone(www.downloadHandler.text);
                    break;
                default:
                    Debug.LogError("Wrong request type!");
                    break;  

            }
            
        }

    }

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//GAME

    public void DisableButtons(){
        status_button.interactable=false;
    }

    public void GetState(){
        StartCoroutine(GetRequest(state_adress));

    }

    public void SendStatus(){

        if(status=="NOT_READY")status="READY";
        else status="READY";
        status_adress+=status;
        Debug.Log("Status");
        Debug.Log(status_adress);
        StartCoroutine(PostRequest(status_adress, PostRequestType.STATUS));

    }


    void HandleState(JSONNode state){

        //string adress="http://localhost:3010/game/3f50a38b923f48ebad12f17e11e96373/state?token=2f76a09f0ce64fb2acd7b3159bb8a7d8";
        //StartCoroutine(GetRequest(state_adress));
        Debug.Log(state["result"]); 
        Debug.Log("Players");
        int i=0;
        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["players"])
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
        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["spectators"])
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
        GameObject spectators = Instantiate(specators_amount, new Vector3(0,0,0), Quaternion.identity);
        spectators.transform.SetParent(specators_panel.transform,false);
        spectators.GetComponent<Text>().text ="";
        string amount = i.ToString();
        spectators.GetComponent<Text>().text = amount; 
    }


    public void Sit(){
        Debug.Log(sit_adress);
        StartCoroutine(PostRequest(sit_adress, PostRequestType.SIT));
        
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

    // //funkcja testowa 
    // void all_bet(){

    //     for (int i =0; i<user_bets.Length; i++){
    //         GameObject bet = Instantiate(big_bet, new Vector3(0,0,0), Quaternion.identity);
    //         GameObject amount = Instantiate(bet_amount, new Vector3(0,0,0), Quaternion.identity);

    //         bet.transform.SetParent(user_bets[i].transform,false);
    //         amount.transform.SetParent(user_bets[i].transform,false);
    //         amount.GetComponent<Text>().bet_amount = "14567";
    //         //Debug.Log("Dodaje");
    //     }

    // }

    
}

public enum GameState {
    Start_game,
    Draw_test,
    Add_money
}
