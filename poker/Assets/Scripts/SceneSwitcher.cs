using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public GameObject table;


    private void Awake() {
        Debug.Log("Awake");
        
    }
    public void play()
    {
    
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        


    }

    public void Back()
    {
        Debug.Log("Klikam back");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

}
