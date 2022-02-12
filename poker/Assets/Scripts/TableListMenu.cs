using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using SimpleJSON;

public class TableListMenu : MonoBehaviour
{
    public static string token;

    public GameObject scrollciew;

    public GameObject Table_panel;

    public GameObject table_id;
    public GameObject number_of_players;
    public GameObject game_state;

    public static string server_adress="vps.damol.pl:4000";

    void Start(){
        Debug.Log("Szukam stołów");
        //token="db0bd26bac28473c9731cc88463ff97f";
        Debug.Log("Mój token to: "+token);
        string adress= server_adress+"/game/list?token="+token;
        StartCoroutine(GetRequest(adress)); 

    }

    public void Click(){
        Debug.Log("Dołączam do stołu o id:?");
    }
   
    public void Back(){     
       SceneManager.LoadScene(3);
    }

    public void JoinTable(){

    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);     
        Debug.Log(node);

        int tables =node["result"]["number_of_tables"];

        Debug.Log("Ilość dostępnych stołów:" +tables);

        Debug.Log("Stoły");
        Debug.Log(node["result"]["tables"]);
       
        foreach( KeyValuePair<string, JSONNode> entry in node["result"]["tables"])
        {
            GameObject id = Instantiate(table_id, new Vector3(0,0,0), Quaternion.identity);
            GameObject players = Instantiate(number_of_players, new Vector3(0,0,0), Quaternion.identity);
            GameObject state = Instantiate(game_state, new Vector3(0,0,0), Quaternion.identity);
            GameObject panel = Instantiate(Table_panel, new Vector3(0,0,0), Quaternion.identity);

            id.transform.SetParent(panel.transform,false);
            id.GetComponent<Text>().text = entry.Key;

            players.transform.SetParent(panel.transform,false);
            players.GetComponent<Text>().text = "Playes: "+entry.Value["number_of_players"];

            panel.transform.SetParent(scrollciew.transform,false);          
            Debug.Log(entry.Value);
            Debug.Log(entry.Key);            
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
            //Debug.Log(www.downloadHandler.text);
            ProcessServerRespone(www.downloadHandler.text);
        }

    }
}
