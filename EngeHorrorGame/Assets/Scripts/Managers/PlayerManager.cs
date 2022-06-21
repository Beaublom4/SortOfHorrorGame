using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    PhotonView pv;

    [SerializeField] GameObject playerPrefab;
    GameObject yourPlayer;

    [SerializeField] Camera[] cams;
    [SerializeField] GameObject deadCamPrefab;
    GameObject currentDeadCam;
    int currentCam = 0;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
            Instance = this;
    }
    private void Start()
    {
        if (!pv.IsMine)
            return;
        yourPlayer = PhotonNetwork.Instantiate(playerPrefab.name, transform.position, transform.rotation);
    }
    public void Dead(Vector3 loc)
    {
        PlayerLook[] looks = FindObjectsOfType<PlayerLook>();
        cams = new Camera[looks.Length + 1];

        currentDeadCam = Instantiate(deadCamPrefab, new Vector3(loc.x, loc.y -1, loc.z), Quaternion.identity, null).transform.GetChild(0).gameObject;
        cams[0] = currentDeadCam.GetComponent<Camera>();
        for (int i = 1; i < cams.Length; i++)
        {
            cams[i] = looks[i - 1].gameObject.GetComponent<Camera>();
        }
    }
    public void NexTCam()
    {
        int oldCam = currentCam;
        currentCam++;
        if (currentCam >= cams.Length)
            currentCam = 0;
        if (cams[currentCam].tag != "DeadCam")
        {
            if (cams[currentCam].GetComponentInParent<PlayerHealth>().dead)
            {
                NexTCam();
                return;
            }
        }
        cams[oldCam].enabled = false;
        cams[currentCam].enabled = true;
    }
    public void RewindCam()
    {
        int oldCam = currentCam;
        currentCam--;
        if (currentCam < 0)
            currentCam = cams.Length - 1;
        if (cams[currentCam].tag != "DeadCam")
        {
            if (cams[currentCam].GetComponentInParent<PlayerHealth>().dead)
            {
                RewindCam();
                return;
            }
        }
        cams[oldCam].enabled = false;
        cams[currentCam].enabled = true;
    }
}
