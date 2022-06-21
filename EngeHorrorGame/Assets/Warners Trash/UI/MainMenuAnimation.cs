using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimation : MonoBehaviour
{
    public GameObject loadingScreen;
    public Animator mainMenuAn;
    public Animator videoAn;

    void Update()
    {
        if (!loadingScreen.activeSelf)
        {
            mainMenuAn.SetTrigger("Go");
            videoAn.SetTrigger("Go");
        }
        
    }


    public void backgroundRoom()
    {
        videoAn.SetTrigger("Room");
    }

    public void exit()
    {
        Application.Quit();
    }
}
