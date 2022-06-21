using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemPickUp : MonoBehaviour
{
    PhotonView pv;

    public Item item;
    [HideInInspector] public int itemCount = 1;
    [SerializeField] int itemCountMin = 1, itemCountMax = 1;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        itemCount = Random.Range(itemCountMin, itemCountMax + 1);
    }
    public void DestroyObj()
    {
        pv.RPC("RPC_DestroyObj", RpcTarget.MasterClient);
    }
    [PunRPC]
    void RPC_DestroyObj()
    {
        if(itemCount <= 0)
            PhotonNetwork.Destroy(gameObject);
    }
    public void ChangeCount(int _count)
    {
        pv.RPC("RPC_ChangeCount", RpcTarget.All, _count);
    }
    [PunRPC]
    void RPC_ChangeCount(int _count)
    {
        itemCount = _count;
    }
}
