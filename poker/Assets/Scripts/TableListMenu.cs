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

    public GameObject join_button;


    void Start(){
        Debug.Log("Szukam stołów");
        token="db0bd26bac28473c9731cc88463ff97f";
        Debug.Log("Mój token to: "+token);
        string adress="http://localhost:3010/game/list?token="+token;
        StartCoroutine(GetRequest(adress)); 

    }

    public void Click(){
        Debug.Log("Dołączam do stołu o id:?");
    }
   
    public void Back(){     
       SceneManager.LoadScene(3);
    }

    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);
        //var node_obj=node.AsObject();

        //Debug.Log(node_obj);
        Debug.Log(node["result"]);

        int tables =node["result"]["number_of_tables"];

        Debug.Log("Ilość dostępnych stołów:" +tables);

        Debug.Log("Stoły");
        Debug.Log(node["result"]["tables"]);

        for(int i =0; i<tables; i++){
            //Debug.Log(node["result"]["tables"][i]);
            //GameObject panel = Instantiate(Table_panel, new Vector3(0,0,0), Quaternion.identity);
            //GameObject id = Instantiate(table_id, new Vector3(0,0,0), Quaternion.identity);
            //id.transform.SetParent(panel.transform,false);
            //id.GetComponent<Text>().text = "jeden";


           

        }

        foreach( KeyValuePair<string, JSONNode> entry in node["result"]["tables"])
        {
            GameObject id = Instantiate(table_id, new Vector3(0,0,0), Quaternion.identity);
            GameObject players = Instantiate(number_of_players, new Vector3(0,0,0), Quaternion.identity);
            GameObject state = Instantiate(game_state, new Vector3(0,0,0), Quaternion.identity);
            GameObject button = Instantiate(join_button, new Vector3(0,0,0), Quaternion.identity);

            GameObject panel = Instantiate(Table_panel, new Vector3(0,0,0), Quaternion.identity);

            id.transform.SetParent(panel.transform,false);
            id.GetComponent<Text>().text = "bardzo długie id stołu aaa";

            players.transform.SetParent(panel.transform,false);
            players.GetComponent<Text>().text = "Playes: "+0;

            state.transform.SetParent(panel.transform,false);
            state.GetComponent<Text>().text = "Blabla"+0;

            panel.transform.SetParent(scrollciew.transform,false);

           
            Debug.Log(entry);
            Debug.Log(entry.Key);
                // do something with entry.Value or entry.Key
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
