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
    public GameObject[] user_panels; //players panels
    public GameObject [] cards;
    public GameObject table;
    //public GameObject panel; //user panel

    public GameObject small_bet; //small bet texture
    public GameObject medium_bet; //medium bet texture
    public GameObject big_bet; //big bet texture
    public GameObject invisible_bet; //big bet texture

    public GameObject bet_amount; //user bet amount

    public GameObject nickname; //user nickname
    public GameObject chips; //user amount of chips
    
    public GameObject specators_panel;
    public GameObject specators_amount;

    public GameObject time_to_end_panel;
    public GameObject time_to_end_amount;

    public GameObject phase_panel;
    public GameObject phase_text;

    public GameObject plot_panel;
    public GameObject plot_text;

    public InputField raise_input_field;

    public Button status_button;

    public Button user_button1; //fold button
    public Button user_button2; //call_check_button
    public Button user_button3; //raise_button
    public Button up_down_button;

    Dictionary<int, int> keys = new Dictionary<int, int>();
    
    // public Image small_bet_image;
    // public Image medium_bet_image;
    // public Image big_bet_image;
    // public Image empty_image;

//ADDRESSES
    public static string server_adress="vps.damol.pl:4000";
    private string state_adress;
    private string sit_adress;
    private string get_up_adress;
    private string status_adress;
    private string leave_adress;
    private string call_adress;
    private string all_in_adress;
    private string check_adress;
    private string fold_adress;
    private string raise_adress;

    private bool message=true;


//IN_GAME_VARIABLES
    public static string username;
    public static string user_token;
    public static string table_id;
    public static string user_id;
    public static string port;

    private int raise_amount=1;
    private string raise_value; // variable sent to raise request
    private int biggest_bet=0;
    private int minimum_raise_value=1;
    private string status="NOT_READY";
    private bool is_sittng=false;
    private int user_bet=0;
    private int lot=0;
    private int phase;

    public static bool player;
   
    
    public static GameManager Instance;
    public GameState State;

    enum PostRequestType{
        SIT,
        GET_UP,
        STATUS,
        LEAVE,
        CALL,
        ALL_IN,
        CHECK,
        FOLD,
        RAISE
    }

    WebSocket websocket;

    public static event Action<GameState> OnGameStateChanged;
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//UNITY

    void Awake() {       
         Instance=this;
         server_adress="vps.damol.pl:4000/game/";
         state_adress= server_adress+table_id+"/state?token="+user_token;
         sit_adress= server_adress+table_id+"/sit_down?token="+user_token;
         get_up_adress= server_adress+table_id+"/get_up?token="+user_token;
         status_adress= server_adress+table_id+"/player_status?token="+user_token+"&status=";
         leave_adress= server_adress+table_id+"/leave?token="+user_token;

         call_adress= server_adress+table_id+"/call?token="+user_token;
         all_in_adress= server_adress+table_id+"/all_in?token="+user_token;
         check_adress= server_adress+table_id+"/check?token="+user_token;
         fold_adress= server_adress+table_id+"/fold?token="+user_token;
         raise_adress= server_adress+table_id+"/raise?token="+user_token+"&amount=";
               
    }

     void Start(){

        SetupWebSocket();
        InitiateObjects();
        raise_input_field.text=raise_amount.ToString();
        phase=-1;

        if(!player){
            ManageButtons(false);         
        }else{
            ManageButtons(false);
            is_sittng=true;
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = "Get up";
            user_nickname[6].GetComponentInChildren<Text>().text=username;
            user_chips[6].GetComponentInChildren<Text>().text="0";
            
        }     
        GetState();      
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
         websocket = new WebSocket("ws://vps.damol.pl:"+port);
         
        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            Debug.Log("On port");
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
            if(message){
                GetState();
                Debug.Log("OnMessage!");           
                //getting the message as a string
                var mess = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("OnMessage! " + mess);
                message=false;
            }
        };

    // waiting for messages
        await websocket.Connect();
    }

    void ProcessSitRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);

        if(node["valid"]){
            //ManageButtons(true);
            //player=true;
            status_button.interactable=true;                
            user_chips[6].GetComponentInChildren<Text>().text="0";
            user_nickname[6].GetComponentInChildren<Text>().text=username;
            //user_panels[6].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,1.0f);
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = "Get up";

        }

    }
     void ProcessGetUpRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        if(node["valid"]){
            //ManageButtons(false);
            //player=false;
            status="NOT_READY";
            ManageButtons(false);
            status_button.interactable=false;
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = "Sit";
            GameObject.Find("StatusButton").GetComponentInChildren<Text>().text = "Ready";
            user_panels[6].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,0.55f);
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


    void ProcessCallRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log("CALL RESPONE");
        Debug.Log(node);
              
    }

    void ProcessAllInRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
               
    }
    void ProcessCheckRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log("CHECK RESPONE");
        Debug.Log(node);
              
    }
    void ProcessFoldRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        if(node["valid"]){
            Debug.Log("Pasuję pomyślnie");         
        }
              
    }
    void ProcessRaiseRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        if(node["valid"]){
            Debug.Log("Poprawnie podbijam o"+raise_value);

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
            ProcessStateRespone(www.downloadHandler.text);        
        }

    }

    IEnumerator PostRequest(string uri, PostRequestType type){        
        string n ="eeeeeeee";
        byte[] bytes = Encoding.ASCII.GetBytes(n);
        UnityWebRequest www = UnityWebRequest.Post(uri,n);
        UploadHandler uploader = new UploadHandlerRaw(bytes);

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
                case PostRequestType.CALL:
                    ProcessCallRespone(www.downloadHandler.text);
                    break;
                case PostRequestType.ALL_IN:
                    ProcessAllInRespone(www.downloadHandler.text);
                    break;
                case PostRequestType.CHECK:
                    ProcessCheckRespone(www.downloadHandler.text);
                    break;
                case PostRequestType.FOLD:
                    ProcessFoldRespone(www.downloadHandler.text);
                    break;
                case PostRequestType.RAISE:
                    ProcessRaiseRespone(www.downloadHandler.text);
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

            user_panels[i].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,0.55f);
          
        }
        GameObject spectators = Instantiate(specators_amount, new Vector3(0,0,0), Quaternion.identity);
        spectators.transform.SetParent(specators_panel.transform,false);

        GameObject time = Instantiate(time_to_end_amount, new Vector3(0,0,0), Quaternion.identity);
        time.transform.SetParent(time_to_end_panel.transform,false);

        GameObject text = Instantiate(phase_text, new Vector3(0,0,0), Quaternion.identity);
        text.transform.SetParent(phase_panel.transform,false);

    }

    void ManageButtons(bool o){        
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
            user_panels[6].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,1.00f);
            ManageButtons(true);
        }
        else{ 
            status="NOT_READY";
            GameObject.Find("StatusButton").GetComponentInChildren<Text>().text = "Ready";
            user_panels[6].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,0.55f);
            ManageButtons(false);
        }
        adress+=status;
        Debug.Log("Status");
        Debug.Log(adress);
        StartCoroutine(PostRequest(adress, PostRequestType.STATUS));

    }

    void HandleState(JSONNode state){

        int i=0; //indeks paneli
        int j=0; //indeks kolejności graczy
        int k =0;
        int bet;
        int [] colour = new int[3];
        int [] value = new int[3]; 
        Debug.Log("GAME STATE");
        Debug.Log(state);
     
        if(lot!=state["lot"]){
            lot=state["lot"];
            UpdateLot();
        }
        
        Array.Clear(colour,0,colour.Length);
        Array.Clear(value,0,value.Length);

        if(phase!=state["result"]["game_state"]["current_phase"] && state["result"]["game_state"]["current_phase"]>0 ){

            DestroyTable();
            phase=state["result"]["game_state"]["current_phase"];
            Debug.Log("BOARD");
            Debug.Log(state["result"]["board"]);

            foreach(KeyValuePair<string, JSONNode> card in state["result"]["board"]){
                colour[k]=card.Value["colour"];
                value[k]=card.Value["value"];
                Debug.Log("K: "+k+" Kolor: "+card.Value["colour"]+" Wartość: " +card.Value["value"]);
                k+=1;

            }            
            if (k>=2) ShowTable(colour,value); //w każdej rundzie?
        }
        k=0;
        Array.Clear(colour,0,colour.Length);
        Array.Clear(value,0,value.Length);
        
        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["players"])
        {   
            
            if(entry.Key==user_id){
                keys[j]=6;                
                user_chips[6].GetComponentInChildren<Text>().text=entry.Value["wallet"]; //portfel
                user_bet=entry.Value["current_bet"];
                Debug.Log("Mój bet:" + entry.Value["current_bet"]);
                                                         
            }else if(i!=6){
                keys[j]=i; 
                user_nickname[i].GetComponentInChildren<Text>().text=entry.Value["username"];               
                user_chips[i].GetComponentInChildren<Text>().text=entry.Value["wallet"];
                Debug.Log("Bet gracza"+ entry.Value["username"]+ " "+ entry.Value["current_bet"]);
                i+=1;

            }else{
                keys[j]=i+1;                
                user_nickname[i+1].GetComponentInChildren<Text>().text=entry.Value["username"];            
                user_chips[i+1].GetComponentInChildren<Text>().text=entry.Value["wallet"];
                i+=1;                          
            }

            if(entry.Value["current_bet"]>0  && state["result"]["game_state"]["current_phase"]>0){          
                Bet(j,entry.Value["current_bet"]);
            }

            if(state["result"]["game_state"]["current_phase"]==1)
            {
                if(entry.Key==state["result"]["game_state"]["small_blind_player_id"].ToString() && user_bets[keys[j]].transform.childCount==0 ){
                    BlindBet(j,state["result"]["small_blind_value"],state["result"]["big_blind_value"]);                   
                }
              
                if(entry.Key==user_id &&user_panels[6].transform.childCount==0){ // wyświetlanie kart w panelu gracza
                    foreach(KeyValuePair<string, JSONNode> card in entry.Value["hand"]){

                        colour[k]=card.Value["colour"];
                        value[k]=card.Value["value"];
                        //Debug.Log(card.Value["colour"]);
                        //Debug.Log(card.Value["value"]);
                        k+=1;
                    }
                    DealCards(colour,value);
                }
                
            }

            bet=entry.Value["current_bet"];
            if(bet>=biggest_bet){
                biggest_bet=bet;
                UpdateRaiseValue(biggest_bet);                
            }
                                
            j+=1;
                                 
        }

        for(int x=0; x<keys.Count; x++){
            Debug.Log(keys[x]);
        }
        i=0;
        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["players"]){
            if(entry.Value["status"]==1) user_panels[keys[i]].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,1.0f);
            i+=1;

        } 
        i=0;   
         foreach( KeyValuePair<string, JSONNode> entry in state["result"]["spectators"])
        { 
            i+=1;                                    
        }
        Debug.Log("Active_player_id: " + state["result"]["game_state"]["active_player_id"]);
        Debug.Log("USER_ID: "+ user_id);
        Debug.Log("Biggest_bet " + biggest_bet);
        Debug.Log("User_bet: " + user_bet);

        if(state["result"]["game_state"]["active_player_id"].ToString()==user_id && biggest_bet==user_bet && state["result"]["game_state"]["current_phase"]>1){
            Debug.Log("Wchodzę do ifa");
            GameObject.Find("Call_check_button").GetComponentInChildren<Text>().text = "CHECK";
            user_button2.onClick.RemoveListener(Call);
            user_button2.onClick.AddListener(Check);
        }else{
            GameObject.Find("Call_check_button").GetComponentInChildren<Text>().text = "CALL";
            user_button2.onClick.RemoveListener(Check);
            user_button2.onClick.AddListener(Call);

        }

        //Debug.Log("Specators in game: " +i.ToString());
        specators_panel.GetComponentInChildren<Text>().text = i.ToString();
        time_to_end_panel.GetComponentInChildren<Text>().text = state["result"]["game_state"]["time_to_end"];
        phase_panel.GetComponentInChildren<Text>().text = state["result"]["game_state"]["current_phase"];
        message=true;
            
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

   void BlindBet(int i,int value, int value2){
        Debug.Log("Blind_bet");

        GameObject bet = Instantiate(small_bet, new Vector3(0,0,0), Quaternion.identity);
        GameObject amount = Instantiate(bet_amount, new Vector3(0,0,0), Quaternion.identity);
        GameObject bet2 = Instantiate(small_bet, new Vector3(0,0,0), Quaternion.identity);
        GameObject amount2 = Instantiate(bet_amount, new Vector3(0,0,0), Quaternion.identity);


        Debug.Log("Dodaje small blind dla user_bets[]"+keys[i]);
        Debug.Log("Długość tablicy z kluczami: " +keys.Count);


        bet.transform.SetParent(user_bets[keys[i]].transform,false);
        amount.transform.SetParent(user_bets[keys[i]].transform,false);
        amount.GetComponent<Text>().text= value.ToString();
  
        if((i+1)==8){
            bet2.transform.SetParent(user_bets[0].transform,false);
            amount2.transform.SetParent(user_bets[0].transform,false);
            amount2.GetComponent<Text>().text= value2.ToString();

        }else{  
            bet2.transform.SetParent(user_bets[keys[i+1]].transform,false);
            amount2.transform.SetParent(user_bets[keys[i+1]].transform,false);
            amount2.GetComponent<Text>().text= value2.ToString();
        }
   }

   void Bet(int i, int value){

        Debug.Log("Standard bet");
        GameObject bet = Instantiate(small_bet, new Vector3(0,0,0), Quaternion.identity);
        GameObject amount = Instantiate(bet_amount, new Vector3(0,0,0), Quaternion.identity);

        if(user_bets[keys[i]].transform.childCount==0){//pierwszy bet danego gracza
           bet.transform.SetParent(user_bets[keys[i]].transform,false);
           amount.transform.SetParent(user_bets[keys[i]].transform,false);
           amount.GetComponent<Text>().text= value.ToString();

        }else{
            string current_bet =user_bets[keys[i]].GetComponentInChildren<Text>().text;
            //value+= Int16.Parse(current_bet);
            user_bets[keys[i]].GetComponentInChildren<Text>().text=value.ToString();
        }         
   }
   void UpdateLot(){
       GameObject bet = Instantiate(big_bet, new Vector3(0,0,0), Quaternion.identity);
       GameObject amount = Instantiate(plot_text, new Vector3(0,0,0), Quaternion.identity);

       if(plot_panel.transform.childCount==0){
           bet.transform.SetParent(plot_panel.transform,false);
           amount.transform.SetParent(plot_panel.transform,false);
           amount.GetComponent<Text>().text= lot.ToString();
       }else{
           plot_panel.GetComponentInChildren<Text>().text=lot.ToString();
       }

   }
   
   void HandleRaise(int i, int value){     
        GameObject im;
        if(value > 0 && value <50){
            im = small_bet;
        }
        else if(value>50 && value<100){
            im=medium_bet;
        }
        else{
            im=big_bet;
        }

        GameObject bet = Instantiate(im, new Vector3(0,0,0), Quaternion.identity);
        GameObject amount = Instantiate(bet_amount, new Vector3(0,0,0), Quaternion.identity);
        bet.transform.SetParent(user_bets[i].transform,false);
        amount.transform.SetParent(user_bets[i].transform,false);
        amount.GetComponent<Text>().text= value.ToString();
   }

   public void Fold (){ //pasuj
        StartCoroutine(PostRequest(fold_adress,PostRequestType.FOLD));
   }
   public void Check(){ // czekaj
        StartCoroutine(PostRequest(check_adress,PostRequestType.CHECK));
   }

   public void Call(){  //sprawdź
        StartCoroutine(PostRequest(call_adress,PostRequestType.CALL));
       
   }

   public void Raise(){ //podbij
       int amount = Int16.Parse(raise_input_field.text);

       if(amount>=minimum_raise_value){
           raise_value=amount.ToString();
           StartCoroutine(PostRequest(raise_adress+raise_value,PostRequestType.RAISE));
           Debug.Log("Podbijam o:" +raise_value);

       }else{
           Debug.Log("Niepoprawna wartość raise");
       }  
    //    Debug.Log("Ustawoina wartość RAISE");
    //    Debug.Log(amount);
   }

   public void AllIn(){ //napierdalaj na ostro
        StartCoroutine(PostRequest(all_in_adress,PostRequestType.ALL_IN));

   }

  

   public void DestroyMyCards(){ 
        for(int i=user_panels[6].transform.childCount-1; i>=0; i--){
            DestroyImmediate(user_panels[6].transform.GetChild(i).gameObject);
        }
    }


   void DealCards(int [] colour, int [] value){
        int x= (colour[0]-1) * 13 + (value[0]-2);
        int y= (colour[1]-1) * 13 + (value[1]-2);
        Debug.Log(x);
        Debug.Log(y);
        GameObject playerCard = Instantiate(cards[x], new Vector3(0,0,0), Quaternion.identity);
        GameObject playerCard2 = Instantiate(cards[y], new Vector3(0,0,0), Quaternion.identity);
        playerCard.transform.SetParent(user_panels[6].transform,false);
        playerCard2.transform.SetParent(user_panels[6].transform,false);      
    }

    void DestroyTable(){
         for(int i=table.transform.childCount-1; i>=0; i--){
            DestroyImmediate(table.transform.GetChild(i).gameObject);
        }
    }


    void ShowTable(int [] colour, int [] value){

        int x;
        for(int j=0; j<colour.Length; j++){
            x=(colour[j]-1) * 13 + (value[j]-2);
            GameObject card = Instantiate(cards[x], new Vector3(0,0,0), Quaternion.identity);
            card.transform.SetParent(table.transform,false);
        }

    }

    void UpdateRaiseValue(int biggest_bet){
        if(user_bets[6].transform.childCount>0){
            int my_bet= Int16.Parse(user_bets[6].GetComponentInChildren<Text>().text);
            minimum_raise_value = (biggest_bet -my_bet) + raise_amount; //(biggest_bet - mój_bet) + raise_amount                  
            raise_input_field.text = minimum_raise_value.ToString();
        }

    }

    //funkcja testowa 
    public void all_bet(){
        UpdateLot();

        for (int i =0; i<user_bets.Length; i++){
            GameObject bet = Instantiate(big_bet, new Vector3(0,0,0), Quaternion.identity);
            GameObject amount = Instantiate(bet_amount, new Vector3(0,0,0), Quaternion.identity);

            bet.transform.SetParent(user_bets[i].transform,false);
            amount.transform.SetParent(user_bets[i].transform,false);
            amount.GetComponent<Text>().text= "14567";
            //Debug.Log("Dodaje");
        }

    }

    
}

public enum GameState {
    Start_game,
    Draw_test,
    Add_money
}
