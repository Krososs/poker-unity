using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddCards : MonoBehaviour
{
    public GameObject table;
    public GameObject card1;
    public GameObject card2;
    // Start is called before the first frame update

    
    void Start()
    {
        
        
    }
    public void click(){
        /*
        for (var i=0; i<5; i++){
            GameObject playerCard = Instantiate(card2, new Vector3(0,0,0), Quaternion.identity);
            playerCard.transform.SetParent(table.transform,false);
        }*/
        GameManager.Instance.UpdateGameState(GameState.Draw_test);
        //UpdateGameState(GameState.Draw_test);
                                
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
