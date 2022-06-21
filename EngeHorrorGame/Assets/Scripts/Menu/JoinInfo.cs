using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;

public class JoinInfo : MonoBehaviour
{
    public Player player;
    public TMP_Text playerName;
    [SerializeField] GameObject emptyInfo, joinedInfo;


    public void SetJoinedInfo(bool toggle)
    {
        joinedInfo.SetActive(toggle);
        emptyInfo.SetActive(!toggle);
    }
}
