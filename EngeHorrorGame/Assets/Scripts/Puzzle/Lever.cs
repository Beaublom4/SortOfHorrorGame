using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Lever : MonoBehaviour
{
    PhotonView pv;

    [SerializeField] LeverPuzzle puzzle;
    bool hasBeenPulled;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    public void Pull()
    {
        if (hasBeenPulled)
            return;
        pv.RPC(nameof(RPC_Pull), RpcTarget.All);
    }
    [PunRPC]
    void RPC_Pull()
    {
        hasBeenPulled = true;
        puzzle.AddPull();
    }
}
