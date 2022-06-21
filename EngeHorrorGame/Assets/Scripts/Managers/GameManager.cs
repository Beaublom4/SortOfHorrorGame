using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    PhotonView pv;

    [SerializeField] GameObject playerManagerPrefab;
    [SerializeField] Transform[] spawnPoints;

    GameObject yourPlayerManager;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        yourPlayerManager = PhotonNetwork.Instantiate(playerManagerPrefab.name, spawnPoints[PhotonManager.photonId].position, spawnPoints[PhotonManager.photonId].rotation);
    }
}
