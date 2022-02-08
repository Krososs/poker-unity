using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class my_panel : MonoBehaviour
{

    public static my_panel Instance;
    private GameObject carde;
    private GameObject cardex;

    void Awake() {
        Instance=this;
        
    }

    public void set_cards( ref GameObject card, ref GameObject card2){
        //carde= card;
        //cardex= card2;
        GameObject playerCard = Instantiate(card, new Vector3(0,0,0), Quaternion.identity);
        playerCard.transform.SetParent(this.transform,false);
        GameObject playerCard2 = Instantiate(card2, new Vector3(0,0,0), Quaternion.identity);
        playerCard2.transform.SetParent(this.transform,false);

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
