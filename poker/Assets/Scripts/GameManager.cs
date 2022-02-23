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
using UnityEngine.Localization.Settings;


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

    public GameObject pool_panel;
    public GameObject pool_text;

    public GameObject winner_panel;

    public GameObject round_winner_panel;
    public GameObject winner_data_panel; //pnel with nickname + prize + card combination
    public GameObject winner_data_text; 

    public GameObject winner_name;
    public GameObject winner_data;

    public InputField raise_input_field;

    public Button status_button;

    public Button user_button1; //fold button
    public Button user_button2; //call_check_button
    public Button user_button3; //raise_button
    public Button up_down_button;
    public Button all_in_button;
    public Button exit_button;
    public Button leave_button;
    Dictionary<int, int> keys = new Dictionary<int, int>();

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
    private bool next_round=true;
    private bool get_state=true;
    private int user_bet=0;
    private int lot=0;
    private int phase;
    private int user_wallet;
    private int players=0;
    public float timeRemaining=0;
    public int timer=3;


    public static bool player;
    public static GameManager Instance;

    public struct RoundWinner
    {
        public string username;
        public List<int> colour;
        public List<int> value;

        public RoundWinner(string _username, List<int> _colour, List<int> _value)
        {   
            colour = new List<int>(_colour);
            value = new List<int>(_value);
            username= _username;    
        }

    }

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
        //all_in_button.interactable=false;
        raise_input_field.text=raise_amount.ToString();
        phase=-1;
        

        if(!player){
            ManageButtons(false);
            status_button.interactable=false;         
        }else{
            ManageButtons(false);
            is_sittng=true;
            leave_button.interactable=false;
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "get_up");
            user_nickname[6].GetComponentInChildren<Text>().text=username;
            user_chips[6].GetComponentInChildren<Text>().text="0";
            
        }     
        GetState();      
    }

    void Update() {

        if(timeRemaining>0){
            int time = (int) Math.Round(timeRemaining);
            timeRemaining-=Time.deltaTime;
            DisplayTime(timeRemaining);
        }

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
        Debug.Log("SIT_DOWN_RESPONE");

        if(node["valid"]){
            //ManageButtons(true);
            //player=true;
            is_sittng=true;
            Debug.Log(sit_adress);
            status_button.interactable=true;
            leave_button.interactable=false;                
            user_chips[6].GetComponentInChildren<Text>().text="0";
            user_nickname[6].GetComponentInChildren<Text>().text=username;
            //user_panels[6].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,1.0f);
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "get_up");

        }

    }
     void ProcessGetUpRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        Debug.Log(node);
        Debug.Log("GET_UP_RESPONE");
        if(node["valid"]){
            is_sittng=false;          
            ManageButtons(false);
            status_button.interactable=false;
            leave_button.interactable=true;
            GameObject.Find("Up/DownButton").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "sit");
            GameObject.Find("StatusButton").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "ready");
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
        Debug.Log("STATUS RESPONE");
        Debug.Log(node);
        if(node["valid"]){
            if(status=="READY"){
                GameObject.Find("StatusButton").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "not_ready");
                user_panels[6].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,1.00f);
                ManageButtons(true);
            }else{
                GameObject.Find("StatusButton").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "ready");
                user_panels[6].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,0.55f);
                ManageButtons(false);
            }

        }else{
            if(status=="READY")status="NOT_READY";
            else status="READY";
        }

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
        all_in_button.interactable=o;
    }

    public void GetState(){
        StartCoroutine(GetRequest(state_adress));
    }

    public void SendStatus(){
        string adress=status_adress;
        if(status=="NOT_READY"){
            status="READY";         
        }
        else{ 
            status="NOT_READY";       
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
        int [] colour = new int[5];
        int [] value = new int[5];
        int _players=0;  
        Debug.Log("GAME STATE");
        Debug.Log(state);
     
        if(lot!=state["result"]["lot"]){
            lot=state["result"]["lot"];
            if(lot>0)UpdateLot(lot);
        }
        
        Array.Clear(colour,0,colour.Length);
        Array.Clear(value,0,value.Length);

        if(phase!=state["result"]["game_state"]["current_phase"] && state["result"]["game_state"]["current_phase"]>0 ){

            if(phase==5 && state["result"]["game_state"]["current_phase"] ==1 ){
                DestroyUserCards();
                ClearRoundWinnerPanel();
            }

            timeRemaining=state["result"]["game_state"]["time_to_end"];
            DestroyTable();
            DestroyUserBets();
            biggest_bet=0;
            phase=state["result"]["game_state"]["current_phase"];
            foreach(KeyValuePair<string, JSONNode> card in state["result"]["board"]){
                colour[k]=card.Value["colour"];
                value[k]=card.Value["value"];
                k+=1;
            }            
            if (k>=2) ShowTable(colour,value,k); //w każdej rundzie?
        }
        if(state["result"]["game_state"]["time_to_end"]==30)
            timeRemaining=state["result"]["game_state"]["time_to_end"];
        k=0;
        Array.Clear(colour,0,colour.Length);
        Array.Clear(value,0,value.Length);
        
        if(pool_panel.transform.childCount>0){
            DestroyPool();
            pool_panel.GetComponentInChildren<Text>().text=lot.ToString();       
        }

        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["players"]){
            _players++;
        }

        if(_players!=players){
            players=_players;

            for(int p=0; p<8; p++){
                user_nickname[p].GetComponentInChildren<Text>().text="";            
                user_chips[p].GetComponentInChildren<Text>().text="";
            }
        }
           
        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["players"])
        {   
            
            if(entry.Key==user_id){
                keys[j]=6;
                user_nickname[6].GetComponentInChildren<Text>().text=entry.Value["username"];                 
                user_chips[6].GetComponentInChildren<Text>().text=entry.Value["wallet"]; //portfel
                user_wallet=entry.Value["wallet"];
                user_bet=entry.Value["current_bet"];
    
                                                         
            }else if(i!=6){
                keys[j]=i; 
                user_nickname[i].GetComponentInChildren<Text>().text=entry.Value["username"];               
                user_chips[i].GetComponentInChildren<Text>().text=entry.Value["wallet"];
                i+=1;

            }else{
                keys[j]=i+1;                
                user_nickname[i+1].GetComponentInChildren<Text>().text=entry.Value["username"];            
                user_chips[i+1].GetComponentInChildren<Text>().text=entry.Value["wallet"];
                i+=1;                          
            }

            if(entry.Value["current_bet"]>0  && state["result"]["game_state"]["current_phase"]>0){          
                Bet(j,entry.Value["current_bet"]);
                UpdatePool(entry.Value["current_bet"]);               
            }
            k=0;
            if(state["result"]["game_state"]["current_phase"]==5 && entry.Key!=user_id && entry.Value["hand_exposed"] && user_panels[keys[j]].transform.childCount==0){
                Array.Clear(colour,0,colour.Length);
                Array.Clear(value,0,value.Length);
                foreach(KeyValuePair<string, JSONNode> card in entry.Value["hand"]){

                    colour[k]=card.Value["colour"];
                    value[k]=card.Value["value"];
                    k+=1;
                }                    
                DealCards(keys[j],colour, value);
            }
            
            k=0;
            if(state["result"]["game_state"]["current_phase"]==1)
            {
                          
                if(entry.Key==user_id &&user_panels[6].transform.childCount==0){ // wyświetlanie kart w panelu gracza
                    foreach(KeyValuePair<string, JSONNode> card in entry.Value["hand"]){

                        colour[k]=card.Value["colour"];
                        value[k]=card.Value["value"];
                        k+=1;
                    }
                    DealCards(6,colour,value);
                }              
            }
        
            if(entry.Value["current_bet"]>=biggest_bet)biggest_bet=entry.Value["current_bet"];                                   
            j+=1;
                                 
        }
        
        if(state["result"]["game_state"]["active_player_id"].ToString()==user_id) ManageButtons(true);
        else ManageButtons(false);

       
        if(state["result"]["game_state"]["current_phase"]==6 && winner_panel.transform.childCount==0){
            List<string> winners = new List<string>(); 
            
            foreach(KeyValuePair<string, JSONNode> id in state["result"]["game_state"]["game_result"]["winners"]){
                Debug.Log(id.Value);

                foreach( KeyValuePair<string, JSONNode> entry in state["result"]["players"]){
                    if(string.Compare(entry.Key, id.Value)==0) winners.Add(entry.Value["username"]);
                }
            }        
            HandleWinner(winners, state["result"]["game_state"]["game_result"]["price"]);
        }       
        i=0;
        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["players"]){
            if(state["result"]["game_state"]["current_phase"]>0 && (string.Compare(entry.Key, state["result"]["game_state"]["active_player_id"])==0) &&state["result"]["game_state"]["current_phase"]!=5 ){ 
                user_panels[keys[i]].GetComponent<Image>().color= new Color(0.6f,0.6f,1.0f,1.0f);
                
            }
            else if (entry.Value["status"]>0) user_panels[keys[i]].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,1.0f);
            i+=1;
        }

        i=0;   
        foreach( KeyValuePair<string, JSONNode> entry in state["result"]["spectators"]){
             i+=1;
             if(entry.Key==user_id) leave_button.interactable=true;
        }
    
        if(state["result"]["game_state"]["active_player_id"].ToString()==user_id && biggest_bet==user_bet && state["result"]["game_state"]["current_phase"]>1){
            GameObject.Find("Call_check_button").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "check");         
        }else{
            GameObject.Find("Call_check_button").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "call");
        }

        if(state["result"]["game_state"]["current_phase"]==5 && next_round){
            next_round=false; 
            for(int c=0; c<keys.Count; c++) user_panels[keys[c]].GetComponent<Image>().color= new Color(1.0f,1.0f,1.0f,0.55f);
            status="NOT_READY";
            GameObject.Find("StatusButton").GetComponentInChildren<Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "ready");
            ManageButtons(false);
            DeleteLot();
            if(round_winner_panel.transform.childCount==0)
                HandleRoundWinner(state);

            foreach( KeyValuePair<string, JSONNode> winner in state["result"]["game_state"]["game_result"]["players"]){
                Debug.Log(winner.Key);
                Debug.Log(winner.Value);
            }
           
        }else next_round=true;
        specators_panel.GetComponentInChildren<Text>().text = i.ToString();
        phase_panel.GetComponentInChildren<Text>().text = state["result"]["game_state"]["current_phase"];
        message=true;
            
    }

    void DisplayTime(float time){
        time+=1;
        
        float second = Mathf.FloorToInt(time % 60);
        int _time = (int) Math.Round(second);
         if((30-timer)==_time && get_state ){
             timer+=4;
             GetState();
             get_state=false;
        }else get_state=true;

        if(_time==0) timer=3;
              
        time_to_end_panel.GetComponentInChildren<Text>().text = second.ToString();
    }

    public void Sit(){
        if(is_sittng==true){
            //if(status=="READY")SendStatus();                
            StartCoroutine(PostRequest(get_up_adress, PostRequestType.GET_UP));        
            
        }else{           
            StartCoroutine(PostRequest(sit_adress, PostRequestType.SIT));                 
        }
              
    }

    public void LeaveTable(){
        StartCoroutine(PostRequest(leave_adress,PostRequestType.LEAVE));
    }

   void Bet(int i, int value){
        GameObject bet = Instantiate(small_bet, new Vector3(0,0,0), Quaternion.identity);
        GameObject amount = Instantiate(bet_amount, new Vector3(0,0,0), Quaternion.identity);

        if(user_bets[keys[i]].transform.childCount==0){//pierwszy bet danego gracza
           bet.transform.SetParent(user_bets[keys[i]].transform,false);
           amount.transform.SetParent(user_bets[keys[i]].transform,false);
           amount.GetComponent<Text>().text= value.ToString();

        }else{
            string current_bet =user_bets[keys[i]].GetComponentInChildren<Text>().text;
            user_bets[keys[i]].GetComponentInChildren<Text>().text=value.ToString();
            DestroyImmediate(bet);
            DestroyImmediate(amount);
        }         
   }
   void UpdateLot(int value){
       GameObject bet = Instantiate(big_bet, new Vector3(0,0,0), Quaternion.identity);
       GameObject amount = Instantiate(plot_text, new Vector3(0,0,0), Quaternion.identity);

       if(plot_panel.transform.childCount==0){
           bet.transform.SetParent(plot_panel.transform,false);
           amount.transform.SetParent(plot_panel.transform,false);                   
           amount.GetComponent<Text>().text= value.ToString();
       }else{
           plot_panel.GetComponentInChildren<Text>().text=value.ToString();
           DestroyImmediate(bet);
           DestroyImmediate(amount);
       }
   }

   void DeleteLot(){
       for(int i=plot_panel.transform.childCount-1; i>=0; i--)
            DestroyImmediate(plot_panel.transform.GetChild(i).gameObject);

   }

   void UpdatePool(int value){
       GameObject amount = Instantiate(pool_text, new Vector3(0,0,0), Quaternion.identity);
       if(pool_panel.transform.childCount==0){
           //Debug.Log("Tworzę pool");
           amount.transform.SetParent(pool_panel.transform,false);
           amount.GetComponent<Text>().text= value.ToString();   
       }else{
           int pool = Int32.Parse(pool_panel.GetComponentInChildren<Text>().text);
           pool+=value;
           pool_panel.GetComponentInChildren<Text>().text= pool.ToString();
           DestroyImmediate(amount);
       }
   }
   public void Fold (){ //pasuj
        StartCoroutine(PostRequest(fold_adress,PostRequestType.FOLD));
   }
   public void Check(){ // czekaj
        StartCoroutine(PostRequest(check_adress,PostRequestType.CHECK));
   }

   public void Call(){
       if(GameObject.Find("Call_check_button").GetComponentInChildren<Text>().text==LocalizationSettings.StringDatabase.GetLocalizedString("UI", "check"))
       StartCoroutine(PostRequest(check_adress,PostRequestType.CHECK));
       else  StartCoroutine(PostRequest(call_adress,PostRequestType.CALL));        
   }

   public void Raise(){ //podbij
       int amount = Int16.Parse(raise_input_field.text);
       if(amount>=minimum_raise_value){
           raise_value=amount.ToString();
           StartCoroutine(PostRequest(raise_adress+raise_value,PostRequestType.RAISE));
       }else{
           Debug.Log("Niepoprawna wartość raise");
       }  

   }

   public void AllIn(){ 
        StartCoroutine(PostRequest(all_in_adress,PostRequestType.ALL_IN));
   }

   public void DestroyUserCards(){
       for(int c=0; c<8; c++){        
            for(int i=user_panels[c].transform.childCount-1; i>=0; i--) 
                DestroyImmediate(user_panels[c].transform.GetChild(i).gameObject);                                         
       }        
   }

    void DealCards(int i, int [] colour, int [] value){
        int x= (colour[0]-1) * 13 + (value[0]-2);
        int y= (colour[1]-1) * 13 + (value[1]-2);
        Debug.Log(x);
        Debug.Log(y);
        GameObject playerCard = Instantiate(cards[x], new Vector3(0,0,0), Quaternion.identity);
        GameObject playerCard2 = Instantiate(cards[y], new Vector3(0,0,0), Quaternion.identity);
        playerCard.transform.SetParent(user_panels[i].transform,false);
        playerCard2.transform.SetParent(user_panels[i].transform,false);      
     }


    void DestroyTable(){
        for(int i=table.transform.childCount-1; i>=0; i--){
            DestroyImmediate(table.transform.GetChild(i).gameObject);
        }
    }

    void DestroyPool(){
        if(pool_panel.transform.childCount>0)
            pool_panel.GetComponentInChildren<Text>().text= "0";

    }

    void DestroyUserBets(){
        for(int i=0; i<8; i++){
            for(int x=user_bets[i].transform.childCount-1; x>=0; x--){
                DestroyImmediate(user_bets[i].transform.GetChild(x).gameObject);
            }
        }
    }


    void ShowTable(int [] colour, int [] value, int k){

        int x;
        for(int j=0; j<k; j++){
            x=(colour[j]-1) * 13 + (value[j]-2);
            GameObject card = Instantiate(cards[x], new Vector3(0,0,0), Quaternion.identity);
            card.transform.SetParent(table.transform,false);
        }

    }

    void HandleWinner( List<string> winners, int wallet){

        GameObject _winners=  Instantiate(winner_name, new Vector3(0,0,0), Quaternion.identity);
        GameObject data =  Instantiate(winner_data, new Vector3(0,0,0), Quaternion.identity);
        Button button =  Instantiate(exit_button, new Vector3(0,0,0), Quaternion.identity);

        winner_panel.GetComponent<Image>().color= new Color(1.0f,0.95f,0.68f,0.53f);

        _winners.GetComponent<Text>().text=LocalizationSettings.StringDatabase.GetLocalizedString("UI", "winners");
        _winners.transform.SetParent(winner_panel.transform,false);

        foreach(string winner in winners){
            GameObject _winner=  Instantiate(winner_name, new Vector3(0,0,0), Quaternion.identity);
            _winner.GetComponent<Text>().text=winner;
            _winner.transform.SetParent(winner_panel.transform,false);
        }

        data.GetComponent<Text>().text=LocalizationSettings.StringDatabase.GetLocalizedString("UI", "prize")+" "+wallet.ToString();
        data.transform.SetParent(winner_panel.transform,false);

        button.transform.SetParent(winner_panel.transform,false);
        button.onClick.AddListener(Exit);
    }

    void HandleRoundWinner(JSONNode state ){
        string _username ="";
        bool is_winner=false;
        List<int> colour;
        List<int> value;
        List<RoundWinner> winners = new List<RoundWinner>();
        
        foreach( KeyValuePair<string, JSONNode> winner in state["result"]["game_state"]["game_result"]["players"]){
            colour= new List<int>();
            value= new List<int>();
            Debug.Log(winner.Key);
            Debug.Log(winner.Value);

            Debug.Log("WINNERS TABLE");
            foreach(KeyValuePair<string, JSONNode> ks in state["result"]["game_state"]["game_result"]["winners"]){
                Debug.Log(ks);
                Debug.Log(ks.Key);
                Debug.Log(ks.Value);
                if(winner.Key.ToString()==ks.Value) is_winner=true;

            }

            if(is_winner){

                foreach(KeyValuePair<string, JSONNode> entry in state["result"]["players"]){
                    if(winner.Key==entry.Key){
                        _username =entry.Value["username"];
                        break;
                    }

                }
                foreach (KeyValuePair<string, JSONNode> card in winner.Value["cards"])
                {
                    colour.Add(card.Value["colour"]);
                    value.Add(card.Value["value"]);               
                }
                RoundWinner _winner = new RoundWinner(_username, colour, value);

                Debug.Log("WINNERS CARDS");
                foreach(int c in _winner.colour){
                    Debug.Log(c);
                }

                winners.Add(_winner);
            }
            is_winner=false;
        }
        HandleRoundWinnerPanel(winners);


    }

    public void HandleRoundWinnerPanel(List<RoundWinner> winners){

        //test variables
        int winners_length= 6;
        int cards_count=5;

        int [] c = new int[5];
        int [] v = new int[5];

        GameObject _winners=  Instantiate(winner_data_text, new Vector3(0,0,0), Quaternion.identity);

        round_winner_panel.GetComponent<Image>().color= new Color(1.0f,0.95f,0.68f,0.53f);

        _winners.GetComponent<Text>().text=LocalizationSettings.StringDatabase.GetLocalizedString("UI", "winners");
        _winners.transform.SetParent(round_winner_panel.transform,false);

        foreach(RoundWinner winner in winners){
            Array.Clear(c,0,c.Length);
            Array.Clear(v,0,v.Length);

            GameObject _winner=  Instantiate(winner_data_text, new Vector3(0,0,0), Quaternion.identity);
            _winner.GetComponent<Text>().text=winner.username;
            _winner.transform.SetParent(round_winner_panel.transform,false);

            GameObject panel=  Instantiate(winner_data_panel, new Vector3(0,0,0), Quaternion.identity);
            //cards
            int i=0;
            Debug.Log("Dodaję kolory od winnera");
            foreach(int z in winner.colour){
                c[i]=z;
                Debug.Log("Kolor:" + c[i]);
                i+=1;
            }
            i=0;
            Debug.Log("Dodaję wartości od winnera");
            Debug.Log("WINNER VALUE COUNT: " +winner.value.Count);
            Debug.Log("WINNER COLOUR COUNT: " +winner.colour.Count);

            foreach( int x in winner.value){
                Debug.Log("Wartość:" + v[i]);
                v[i]=x;
                i+=1;

            }
            for(int j =0; j< winner.value.Count; j++){
                int x = (c[j]-1) * 13 + (v[j]-2);
                Debug.Log("IKS: " + x);
                Debug.Log("c[j]: " + c[j]);
                Debug.Log("v[j]: " + v[j]);

                GameObject card = Instantiate(cards[x], new Vector3(0,0,0), Quaternion.identity);
                card.transform.SetParent(panel.transform, false);
            }
        
            panel.transform.SetParent(round_winner_panel.transform, false);


        }


    }

    void ClearRoundWinnerPanel(){
        round_winner_panel.GetComponent<Image>().color= new Color(1.0f,0.95f,0.68f,0.0f);
        for(int x=round_winner_panel.transform.childCount-1; x>=0; x--){
            DestroyImmediate(round_winner_panel.transform.GetChild(x).gameObject);
        }

    }

    public void Exit(){
        LeaveTable();
        SceneManager.LoadScene(3);

    }
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//FUNKCJE TESTOWE


     public void AllCards(){
        for(int i =0; i<8; i++){
            GameObject playerCard = Instantiate(cards[2], new Vector3(0,0,0), Quaternion.identity);
            GameObject playerCard2 = Instantiate(cards[2], new Vector3(0,0,0), Quaternion.identity);
            playerCard.transform.SetParent(user_panels[i].transform,false);
            playerCard2.transform.SetParent(user_panels[i].transform,false);  

        }
    }

    public void Tablee(){
        for(int i=0; i<5; i++){
            GameObject card = Instantiate(cards[15], new Vector3(0,0,0), Quaternion.identity);
            card.transform.SetParent(table.transform,false);
        }

    }
}
