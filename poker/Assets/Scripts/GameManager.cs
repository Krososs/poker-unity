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

    public static string username;
    public static string username_token;
    public static string table_id;
    public static string user_id;

    public static bool player; 

    private string state_adress;
    private string sit_adress;

    public static GameManager Instance;

    public GameState State;

    public static event Action<GameState> OnGameStateChanged;

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

    void Update() {

        //PrintGameState();
       
        
    }

    void Start(){

        Debug.Log("Witam gracza");
        //username="Aktualny";
        Debug.Log(username);
        Debug.Log("O numerze id");
        Debug.Log(user_id);
        Debug.Log("Przy stole");
        Debug.Log(table_id);

        //PrintGameState();

        //SetUserData();
        all_bet();
    }


    public void PrintGameState(){
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

    void ProcessSitRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);

    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        Debug.Log(node["result"]); 
        //Debug.Log(node["result"]["players"]);
        //Debug.Log(node["result"]["spectators"]);

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
    }

    void SetUserData(){

        for (int i =0; i<user_nickname.Length; i++){
            GameObject _nickname = Instantiate(nickname, new Vector3(0,0,0), Quaternion.identity);
            GameObject _chips = Instantiate(chips, new Vector3(0,0,0), Quaternion.identity);

            _nickname.transform.SetParent(user_nickname[i].transform,false);
            _chips.transform.SetParent(user_chips[i].transform,false);

            if (i == 6)
                _nickname.GetComponent<Text>().text = username;               
            else
                _nickname.GetComponent<Text>().text = "DamolPL";
                

            _chips.GetComponent<Text>().text = "99999";            
        }
               
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

   public void SetUsername(){
       string recivedUsername = LoginMenu.username;
       username=recivedUsername;
       Debug.Log(recivedUsername);

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

    IEnumerator GetRequest(string uri){
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();

        if(www.result != UnityWebRequest.Result.Success){
           Debug.LogError("Something went wrong " + www.error);
           
        }
        else
        {
            ProcessServerRespone(www.downloadHandler.text);
            //ProcessSitRespone(www.downloadHandler.text);
            // switch(reqType){
            //     case 0:
            //         ProcessServerRespone(www.downloadHandler.text);
            //         break;
            //     case 1:
            //         ProcessSitRespone(www.downloadHandler.text);
            //         break;
            // }
            
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
}



public enum GameState {
    Start_game,
    Draw_test,
    Add_money
}
