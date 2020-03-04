using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButton()
    {
        Debug.Log("Launch gameloop scene!!!");

        // TODO: link this button with the gameloop scene once it is established.
        //SceneManager.LoadScene("Gameloop");
		
		//temporary scene:
		SceneManager.LoadScene("WorldTest2");
    }

    public void AboutButton()
    {
        SceneManager.LoadScene("About");
    }

    public void SettingsButton()
    {
        SceneManager.LoadScene("Settings");
    }



}
