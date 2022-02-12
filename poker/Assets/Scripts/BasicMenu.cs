using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicMenu : MonoBehaviour
{

    private string server_adress="vps.damol.pl:4000";
    public void Awake(){
        MainMenu.server_adress=server_adress;
        GameManager.server_adress=server_adress;
        RegisterMenu.server_adress=server_adress;
        table.server_adress=server_adress;
        TableListMenu.server_adress=server_adress;
        //ListScene.server_adress=server_adress;
        LoginMenu.server_adress=server_adress;
        

    }
    public void LoginScene(){
        SceneManager.LoadScene(1);       
    }

    public void RegisterScene(){
        SceneManager.LoadScene(2);
    }
    public void ExitGame(){
        Application.Quit();
    }

  
}
