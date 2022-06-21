using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PianoPuzzle : MonoBehaviour
{
    PhotonView pv;

    public int keyId;
    [SerializeField] GameObject[] keyObjects;
    [SerializeField] GameObject bookcase;
    [SerializeField] Transform bookcaseLoc;
    [SerializeField] int wantedInsertedKeys = 3;
    int keysInserted;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    public void Interact()
    {
        //play song
    }
    public void AddKey()
    {
        keyObjects[keysInserted].SetActive(true);
        keysInserted++;
        if(keysInserted >= wantedInsertedKeys)
        {
            Debug.Log("Jippie");
            pv.RPC("RPC_StartMove", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    void RPC_StartMove()
    {
        StartCoroutine(Move());
    }
    IEnumerator Move()
    {
        while(Vector3.Distance(bookcase.transform.position, bookcaseLoc.position) > .1f)
        {
            bookcase.transform.position = Vector3.Lerp(bookcase.transform.position, bookcaseLoc.position, .1f * Time.deltaTime);
            yield return null;
        }
    }
}