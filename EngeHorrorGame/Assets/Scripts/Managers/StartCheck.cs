using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class StartCheck : MonoBehaviour
{
    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene(0);
    }
}
