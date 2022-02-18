using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using SimpleJSON;
using UnityEngine.Localization.Settings;

public class RankingMenu : MonoBehaviour
{
    public static string token;

    public GameObject scrollview;
    public GameObject user_panel;
    public GameObject username;
    public GameObject total_score;
 

    public static string server_adress="vps.damol.pl:4000";

    // Start is called before the first frame update
    void Start()
    {
        string adress= server_adress+"/ranking?token="+token;
        StartCoroutine(GetRequest(adress));     
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Back(){
        SceneManager.LoadScene(3);
    }



    void ProcessServerRespone(string rawRespone){
        JSONNode node = SimpleJSON.JSON.Parse(rawRespone);     
        Debug.Log(node);
        if(node["valid"]){
            Debug.Log(node["result"]);
            Debug.Log(node["result"]["result"]);
            int i=1;
            foreach(KeyValuePair<string, JSONNode> entry in node["result"]["result"]){
                Debug.Log(entry);
                Debug.Log(entry.Key);
                Debug.Log(entry.Value);

                GameObject user = Instantiate(username, new Vector3(0,0,0), Quaternion.identity);
                GameObject panel = Instantiate(user_panel, new Vector3(0,0,0), Quaternion.identity);

                user.transform.SetParent(panel.transform,false);
                user.GetComponent<Text>().text = i.ToString()+". "+entry.Value["username"]+" "+LocalizationSettings.StringDatabase.GetLocalizedString("UI", "total_score")+" "+ entry.Value["total_score"];
                panel.transform.SetParent(scrollview.transform,false);
                i+=1;

        
            }

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
