using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicMenu : MonoBehaviour
{
    
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
