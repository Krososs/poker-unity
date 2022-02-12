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
//GAME OBJECTS   
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

    public Button user_button1;
    public Button user_button2;
    public Button user_button3;
    public Button up_down_button;


    public static string username;
    public static string user_token;
    public static string table_id;
    public static string user_id;

    public static bool player; 
//PRIVATE

//ADDRESSES
    private string state_adress;
    private string sit_adress;
    private string get_up_adress;
    private string status_adress;
    private string leave_adress;

//IN_GAME_VARIABLES
    private string status="NOT_READY";
    private bool is_sittng=false;
    private int user_chips_amount;
    

    public static GameManager Instance;

    enum PostRequestType{
        SIT,
        GET_UP,
        STATUS,
        LEAVE
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
        state_adress="http://localhost:3010/game/"+table_id+"/state?token="+user_token;
        sit_adress="http://localhost:3010/game/"+table_id+"/sit_down?token="+user_token;
        get_up_adress="http://localhost:3010/game/"+table_id+"/get_up?token="+user_token;
        status_adress="http://localhost:3010/game/"+table_id+"/player_status?token="+user_token+"&status=";
        leave_adress="http://localhost:3010/game/"+table_id+"/leave?token="+user_token;        
                
    }

     void Start(){

        SetupWebSocket();
        InitiateObjects();
        Debug.Log("Witam gracza");
        Debug.Log(username);
        Debug.Log("O numerze id");
        Debug.Log(user_id);
        Debug.Log("Przy stole");
        Debug.Log(table_id);

        if(!player){
            ManageButtons(false);         
        }else{
            is_sittng=true;
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = "Get up";
            user_nickname[6].GetComponentInChildren<Text>().text=username;
            user_chips[6].GetComponentInChildren<Text>().text="0";
            
        }
        GetState();
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

        if(node["valid"]){
            ManageButtons(true);
            //player=true;                
            user_chips[6].GetComponentInChildren<Text>().text="0";
            user_nickname[6].GetComponentInChildren<Text>().text=username;
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = "Get up";

        }

    }
     void ProcessGetUpRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        if(node["valid"]){
            ManageButtons(false);
            //player=false;
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = "Sit";
            user_nickname[6].GetComponentInChildren<Text>().text = "";
            user_chips[6].GetComponentInChildren<Text>().text = "";
        }
   
    }

    void ProcessLeaveRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        if(node["valid"]){
            SceneManager.LoadScene(3);
        }

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
                case PostRequestType.GET_UP:
                    ProcessGetUpRespone(www.downloadHandler.text);
                    break;
                case PostRequestType.LEAVE:
                    ProcessLeaveRespone(www.downloadHandler.text);
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

    private void InitiateObjects(){
        //initiate user panel data
        for(int i=0; i<8; i++){
            GameObject _nickname = Instantiate(nickname, new Vector3(0,0,0), Quaternion.identity);
            GameObject _chips = Instantiate(chips, new Vector3(0,0,0), Quaternion.identity);
            _nickname.transform.SetParent(user_nickname[i].transform,false);
            _chips.transform.SetParent(user_chips[i].transform,false);
        }
        GameObject spectators = Instantiate(specators_amount, new Vector3(0,0,0), Quaternion.identity);
        spectators.transform.SetParent(specators_panel.transform,false);
    }

    void ManageButtons(bool o){
        status_button.interactable=o;
        user_button1.interactable=o;
        user_button2.interactable=o;
        user_button3.interactable=o;
    }

    public void GetState(){
        StartCoroutine(GetRequest(state_adress));
    }

    public void SendStatus(){
        string adress=status_adress;
        if(status=="NOT_READY"){
            status="READY";
            GameObject.Find("StatusButton").GetComponentInChildren<Text>().text = "Not ready";
        }
        else{ 
            status="NOT_READY";
            GameObject.Find("StatusButton").GetComponentInChildren<Text>().text = "Ready";
        }
        adress+=status;
        Debug.Log("Status");
        Debug.Log(adress);
        StartCoroutine(PostRequest(adress, PostRequestType.STATUS));

    }

    void HandleState(JSONNode state){
        int i=0;
        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["players"])
        {           
            Debug.Log(entry.Key);       
            Debug.Log(entry.Value);
            if(entry.Key==user_id){                
                user_chips[6].GetComponentInChildren<Text>().text=entry.Value["wallet"];               
                
            }else if(i!=6){
                user_nickname[i].GetComponentInChildren<Text>().text=entry.Value["username"];               
                user_chips[i].GetComponentInChildren<Text>().text=entry.Value["wallet"]; 
            }else{
                user_nickname[i+1].GetComponentInChildren<Text>().text=entry.Value["username"];            
                user_chips[i+1].GetComponentInChildren<Text>().text=entry.Value["wallet"];           
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
         }

        Debug.Log("Specators in game: " +i.ToString());
        specators_panel.GetComponentInChildren<Text>().text = i.ToString();
            
    }


    public void Sit(){
        if(is_sittng==true){
            is_sittng=false;       
            StartCoroutine(PostRequest(get_up_adress, PostRequestType.GET_UP));
            Debug.Log("Wstaje");
        }else{
            is_sittng=true;
            Debug.Log("Siadam");
            Debug.Log(sit_adress);
            StartCoroutine(PostRequest(sit_adress, PostRequestType.SIT));         
        }
              
    }

    public void LeaveTable(){
        StartCoroutine(PostRequest(leave_adress,PostRequestType.LEAVE));
        Debug.Log(leave_adress);
    }

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
