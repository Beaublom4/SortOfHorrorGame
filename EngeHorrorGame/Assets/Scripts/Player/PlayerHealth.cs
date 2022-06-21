using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;
    PhotonView pv;

    public float health;
    public float currentHealth;
    [SerializeField] GameObject yourBody;
    [SerializeField] Animator yourBodyAnim, fullBodyAnim;
    [SerializeField] GameObject reviveCol, deadIcon;

    public bool dead;
    [SerializeField] Camera[] cams;

    PlayerLook interactScript;
    bool reviving;
    [SerializeField] float reviveTime;
    float reviveTimer;

    public void ADInput(InputAction.CallbackContext callback)
    {
        if (!pv.IsMine)
            return;

        Vector2 input = callback.ReadValue<Vector2>();

        if (callback.performed)
        {
            if (dead)
            {
                if (input.x < 0)
                    PlayerManager.Instance.NexTCam();
                else if(input.x > 0)
                    PlayerManager.Instance.RewindCam();
            }
        }
    }

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        currentHealth = health;
        if (pv.IsMine)
            Instance = this;
    }
    private void Update()
    {
        if (reviving)
        {
            if(reviveTimer >= 0)
            {
                reviveTimer -= Time.deltaTime;
            }
            else
            {
                Revive();
            }
        }
    }
    public void GetHit(float damage)
    {
        if (!pv.IsMine || dead)
            return;

        currentHealth -= damage;
        yourBodyAnim.SetTrigger("Hit");
        fullBodyAnim.SetTrigger("Hit");
        HudManager.instance.GetHit(currentHealth, health);

        if(currentHealth <= 0)
        {
            dead = true;
            yourBody.SetActive(false);
            GetComponentInChildren<PlayerLook>().canLook = false;
            GetComponent<PlayerMove>().canMove = false;
            fullBodyAnim.SetBool("Down", true);
            pv.RPC("RPC_Dead", RpcTarget.Others);

            PlayerManager.Instance.Dead(transform.position);
            foreach(Camera cam in cams)
            {
                cam.enabled = false;
            }
        }
    }
    public void AddHealth(float heal)
    {
        if (!pv.IsMine || dead)
            return;

        currentHealth += heal;
        if (currentHealth > health)
            currentHealth = health;
    }
    [PunRPC]
    void RPC_Dead()
    {
        dead = true;
        reviveCol.SetActive(true);
        deadIcon.SetActive(true);
    }
    public void StartRevive(PlayerLook _interactScript)
    {
        interactScript = _interactScript;
        reviveTimer = reviveTime;
        reviving = true;
        Debug.Log("Start revive");
    }
    public void StopRevive()
    {
        reviving = false;
        Debug.Log("Stop revive");
    }
    void Revive()
    {
        reviving = false;
        interactScript = null;
        pv.RPC("RPC_Revive", RpcTarget.All);
    }
    [PunRPC]
    void RPC_Revive()
    {
        dead = false;
        reviveCol.SetActive(false);
        deadIcon.SetActive(false);
        currentHealth = health;
        if (pv.IsMine)
        {
            yourBody.SetActive(true);
            GetComponentInChildren<PlayerLook>().canLook = true;
            GetComponent<PlayerMove>().canMove = true;
            fullBodyAnim.SetBool("Down", false);
        }
    }
}
