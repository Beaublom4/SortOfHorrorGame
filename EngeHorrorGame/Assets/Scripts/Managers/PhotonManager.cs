using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;
    PhotonView pv;

    [SerializeField] GameObject playerNameObj;

    [Header("Room")]
    [SerializeField] TMP_Text roomCodeText;
    [SerializeField] GameObject[] openOptions;
    [SerializeField] GameObject[] kickButtons;
    [SerializeField] Transform infoHolder;
    bool solo = false;

    [SerializeField] TMP_Text readyText, startText;
    [SerializeField] Color normalColor, readyColor;
    bool ready;
    int playersReady;

    [Header("PlayerInfo")]
    public static int photonId;

    private void Awake()
    {
        Instance = this;
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        MenuManager.Instance.SelectMenu("loading");
        Debug.Log("Connecting to server");
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        if (PhotonNetwork.NickName != "")
            playerNameObj.SetActive(false);
        PhotonNetwork.AutomaticallySyncScene = true;
        MenuManager.Instance.SelectMenu("menu");
    }
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3;
        int roomNum = Random.Range(1000, 9999);
        PhotonNetwork.CreateRoom(roomNum.ToString("0000"), roomOptions);
    }
    public override void OnJoinedRoom()
    {
        if (!solo)
        {
            roomCodeText.text = "Code: " + PhotonNetwork.CurrentRoom.Name;
            if (PhotonNetwork.IsMasterClient)
            {
                startText.gameObject.SetActive(true);
                foreach (GameObject g in openOptions)
                    g.SetActive(true);
                foreach (GameObject g in kickButtons)
                    g.SetActive(true);
            }
            pv.RPC("RPC_JoinGame", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, PhotonNetwork.CurrentRoom.PlayerCount - 1);
            pv.RPC("RPC_CheckReadyCount", RpcTarget.MasterClient);

            MenuManager.Instance.SelectMenu("room");
        }
        else
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            Debug.Log("Start game");
            PhotonNetwork.LoadLevel(1);
        }
    }
    [PunRPC]
    void RPC_JoinGame(Player _player, int joinedCount)
    {
        JoinInfo info = infoHolder.GetChild(joinedCount).GetComponent<JoinInfo>();
        info.player = _player;
        info.playerName.text = _player.NickName;
        info.SetJoinedInfo(true);
    }
    public void JoinGame(TMP_InputField inputField)
    {
        PhotonNetwork.JoinRoom(inputField.text);
    }
    public void ChangeName(TMP_InputField _name)
    {
        if (_name.text == "")
            return;

        _name.text.Replace(" ", "-");
        PhotonNetwork.NickName = _name.text;
        PlayerPrefs.SetString("PlayerName", _name.text);
        playerNameObj.SetActive(false);
    }
    public void Ready()
    {
        ready = !ready;
        if (ready)
            readyText.text = "Unready";
        else
            readyText.text = "Ready";

        pv.RPC("RPC_Ready", RpcTarget.AllBuffered, ready);
        pv.RPC("RPC_CheckReadyCount", RpcTarget.MasterClient);
    }
    [PunRPC]
    void RPC_Ready(bool ready)
    {
        if (ready)
        {
            playersReady++;
        }
        else
        {
            playersReady--;
        }
    }
    [PunRPC]
    void RPC_CheckReadyCount()
    {
        if (playersReady >= PhotonNetwork.CurrentRoom.PlayerCount)
            startText.color = readyColor;
        else
            startText.color = normalColor;
    }
    public void StartGameSolo()
    {
        solo = true;
        MenuManager.Instance.SelectMenu("loading");
        PhotonNetwork.CreateRoom(Random.Range(0, 9999).ToString("0000"));
    }
    public void StartGame()
    {
        if (playersReady == PhotonNetwork.PlayerList.Length)
        {
            Player[] players = PhotonNetwork.PlayerList;
            for (int i = 0; i < players.Length; i++)
            {
                pv.RPC("RPC_SetPhotonId", players[i], i);
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
            Debug.Log("Start game");
            PhotonNetwork.LoadLevel(1);
        }
    }
    [PunRPC]
    void RPC_SetPhotonId(int _id)
    {
        photonId = _id;
    }
}
