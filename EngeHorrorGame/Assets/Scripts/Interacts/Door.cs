using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Door : MonoBehaviour
{
    PhotonView pv;

    [SerializeField] bool dubbleDoor;
    [SerializeField] Transform doorRight, doorLeft;
    float startRot1, startRot2;
    [SerializeField] float openingTime;
    float wantedRot;
    float wantedRot1, wantedRot2;

    public bool lockedWithKey;
    public int keyId;
    [SerializeField] bool oneWayFront, oneWayBack;

    float normalRot;

    bool moving;
    float startRot;
    float timer;

    bool opened;
    bool openedFront;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        normalRot = transform.eulerAngles.y;
    }
    private void Update()
    {
        if (moving)
        {
            if (timer < openingTime)
            {
                float yRot = Mathf.Lerp(startRot, wantedRot, timer / openingTime);

                if(!dubbleDoor)
                    transform.rotation = Quaternion.Euler(transform.rotation.x, yRot, transform.rotation.z);
                else
                {
                    float yRot1 = Mathf.Lerp(startRot1, wantedRot1, timer / openingTime);
                    float yRot2 = Mathf.Lerp(startRot2, wantedRot2, timer / openingTime);
                    doorRight.rotation = Quaternion.Euler(doorRight.rotation.x, yRot1, doorRight.rotation.z);
                    doorLeft.rotation = Quaternion.Euler(doorLeft.rotation.x, yRot2, doorLeft.rotation.z);
                }

                timer += Time.deltaTime;
            }
            else if(timer >= openingTime)
            {
                if(!dubbleDoor)
                    transform.rotation = Quaternion.Euler(transform.rotation.x, wantedRot, transform.rotation.z);
                else
                {
                    doorRight.rotation = Quaternion.Euler(doorRight.rotation.x, wantedRot1, doorRight.rotation.z);
                    doorLeft.rotation = Quaternion.Euler(doorLeft.rotation.x, wantedRot2, doorLeft.rotation.z);
                }
                moving = false;
            }
        }
    }
    public void Unlock()
    {
        lockedWithKey = false;
        pv.RPC("RPC_Unlock", RpcTarget.Others);
    }
    [PunRPC]
    void RPC_Unlock()
    {
        lockedWithKey = false;
    }
    public void Open(bool front, bool right)
    {
        if (moving || lockedWithKey)
            return;

        pv.RPC("RPC_Interact", RpcTarget.All, front, right);
    }
    [PunRPC]
    void RPC_Interact(bool front, bool right)
    {
        if (!opened)
        {
            if (front)
            {
                if (oneWayBack)
                    return;
                else if (oneWayFront)
                    oneWayFront = false;

                openedFront = true;
                timer = 0;
                if (!dubbleDoor)
                {
                    wantedRot = normalRot - 90;
                    startRot = transform.eulerAngles.y;
                }
                else
                {
                    wantedRot1 = normalRot - 90;
                    wantedRot2 = normalRot + 90;

                    startRot1 = doorRight.eulerAngles.y;
                    startRot2 = doorLeft.eulerAngles.y;
                }
                moving = true;
                opened = true;
            }
            else
            {
                if (oneWayFront)
                    return;
                else if (oneWayBack)
                    oneWayBack = false;

                openedFront = false;
                timer = 0;
                if (!dubbleDoor)
                {
                    wantedRot = normalRot + 90;
                    startRot = transform.eulerAngles.y;
                }
                else
                {
                    wantedRot1 = normalRot + 90;
                    wantedRot2 = normalRot - 90;

                    startRot1 = doorRight.eulerAngles.y;
                    startRot2 = doorLeft.eulerAngles.y;
                }
                moving = true;
                opened = true;
            }
        }
        else
        {
            if (openedFront)
            {
                if(!dubbleDoor)
                    wantedRot = transform.eulerAngles.y + 90;
                else
                {
                    wantedRot1 = doorRight.eulerAngles.y + 90;
                    wantedRot2 = doorLeft.eulerAngles.y - 90;
                }
            }
            else
            {
                if(!dubbleDoor)
                    wantedRot = transform.eulerAngles.y - 90;
                else
                {
                    wantedRot1 = doorRight.eulerAngles.y - 90;
                    wantedRot2 = doorLeft.eulerAngles.y + 90;
                }
            }

            timer = 0;
            if(!dubbleDoor)
                startRot = transform.eulerAngles.y;
            else
            {
                startRot1 = doorRight.eulerAngles.y;
                startRot2 = doorLeft.eulerAngles.y;
            }
            moving = true;
            opened = false;
        }
    }
}
