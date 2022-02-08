using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dealer : MonoBehaviour
{
    public GameObject panel;
    public GameObject table;
    public GameObject card1;
    public GameObject card2;
    public MonoBehaviour myp;
    // Start is called before the first frame update

    void Awake() {
        GameManager.OnGameStateChanged += even;
        
    }

    void OnDestroy() {
        GameManager.OnGameStateChanged -= even;
        
    }
    private void even(GameState state){
        if(state==GameState.Draw_test){
            this.draw();
        }

    }

    public void draw(){
        for (var i=0; i<5; i++){
            GameObject playerCard = Instantiate(card2, new Vector3(0,0,0), Quaternion.identity);
            playerCard.transform.SetParent(table.transform,false);
        }
        Debug.Log("Ajem drawing");

    }

    public void initiate_user_panel(GameObject card, GameObject card2){
        GameObject playerCard = Instantiate(card, new Vector3(0,0,0), Quaternion.identity);
         playerCard.transform.SetParent(panel.transform,false);
         GameObject playerCard2 = Instantiate(card2, new Vector3(0,0,0), Quaternion.identity);
         playerCard2.transform.SetParent(panel.transform,false);

    }

    

    public void Start()
    {
         //initiate_user_panel(card1, card1);
         my_panel.Instance.set_cards(ref card1, ref card2);
         
        //GameObject playerCard = UnityEngine.GameObject.Instatiate(card1, new Vector(0,0,0), Quaternion.identity);
    }

    // Tutaj trzeba będzie sprawdzać jaki jest state gry i wtedy podejmować jakieś decyzje
    void Update()
    {
        
    }
}
