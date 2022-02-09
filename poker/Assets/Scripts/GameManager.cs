using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
 using UnityEngine.UI;
using System.Linq;
using SimpleJSON;

public class GameManager : MonoBehaviour
{
    /*
    public GameObject user1_data;
    public GameObject user2_data;
    public GameObject user3_data;
    public GameObject user4_data;
    public GameObject user5_data;
    public GameObject user6_data;
    public GameObject user7_data;
    public GameObject user8_data;*/

    public GameObject[] user_bets; //poke z chipami + ilością postawionych
    public GameObject[] user_nickname; //pole z nickname + ilość żetonów
    public GameObject[] user_chips;
    //public GameObject[] user_chips;


    public GameObject panel;


    public GameObject small_bet;
    public GameObject medium_bet;
    public GameObject big_bet;
    public GameObject text;
    public GameObject nickname;
    public GameObject chips;

    public static string username;


    public static GameManager Instance;

    public GameState State;

    public static event Action<GameState> OnGameStateChanged;

    void Awake() {
        Instance=this;
        
    }

    void Update() {
        Debug.Log(username);
        
    }

    void Start(){
        start_game();
        SetUserData();
        all_bet();
        UpdateGameState(GameState.Add_money);
    }
   
   public void UpdateGameState(GameState newState){
       State=newState;

       switch(newState){

           case GameState.Draw_test:
                Handle_Draw_test();
                break;
            case GameState.Add_money:
                break;
            case GameState.Start_game:
                start_game();
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

   void Handle_Draw_test(){
        Debug.Log("I am drawing cards");
    }

    public void start_game(){

        //pobieranie tokena
        string token = "26999ed8d2d64a70a2ff55865a571d97";
        string adress="http://localhost:3010/auth?token=";
        StartCoroutine(GetRequest(adress+token));       

    }
    //funkcja testowa 
    void all_bet(){

        for (int i =0; i<user_bets.Length; i++){
            GameObject bet = Instantiate(big_bet, new Vector3(0,0,0), Quaternion.identity);
            GameObject txt = Instantiate(text, new Vector3(0,0,0), Quaternion.identity);
            bet.transform.SetParent(user_bets[i].transform,false);
            txt.transform.SetParent(user_bets[i].transform,false);
            txt.GetComponent<Text>().text = "14567";
            Debug.Log("Dodaje");

            

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
    

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);

    }

    IEnumerator GetRequest(string uri){
        UnityWebRequest www = UnityWebRequest.Get(uri);
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



public enum GameState {
    Start_game,
    Draw_test,
    Add_money
}
