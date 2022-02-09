using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicMenu : MonoBehaviour
{

    
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void LoginScene(){
        SceneManager.LoadScene(1);       
    }

    public void RegisterScene(){
        SceneManager.LoadScene(2);
    }
    public void ExitGame(){
        Debug.Log("Wychodzimy");
    }

  
}
