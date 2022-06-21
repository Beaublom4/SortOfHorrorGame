using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public static PlayerMove Instance;
    PhotonView pv;
    [SerializeField] Animator yourAnim, othersAnim;

    public bool canMove;
    [SerializeField] float speed;

    [SerializeField] bool isRunning;
    bool runInput;
    [SerializeField] float sprintMultiplier;
    float currentSprintMultiplayer = 1;
    [SerializeField] float runCharge = 3, waitForRunRechange = 2;
    float maxCharge;
    IEnumerator coroutine;

    Vector3 moveVector;

    public void Stop()
    {
        moveVector = Vector3.zero;
    }
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        maxCharge = runCharge;
        if (pv.IsMine)
            Instance = this;
    }
    #region Inputs
    public void MoveInput(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || !canMove)
            return;

        Vector2 moveInput = context.ReadValue<Vector2>();

        if (moveInput.magnitude > 0)
            DoAnim(yourAnim, othersAnim, "Walking", 1, true, 0);
        else
            DoAnim(yourAnim, othersAnim, "Walking", 1, false, 0);

        DoAnim(yourAnim, othersAnim, "Blend", 2, false, moveInput.y);
        moveVector = new Vector3(moveInput.x, 0, moveInput.y);
    }
    public void SprintInput(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || !canMove)
            return;

        if (context.performed)
        {
            runInput = true;
            if (runCharge > 0 && coroutine != null)
                StopCoroutine(coroutine);
        }
        else if (context.canceled)
        {
            runInput = false;
        }
    }
    IEnumerator RunRecharge()
    {
        yield return new WaitForSeconds(waitForRunRechange);
        while(runCharge < maxCharge)
        {
            runCharge += Time.deltaTime;
            yield return null;
        }
        runCharge = maxCharge;
    }
    private void Update()
    {
        if (!pv.IsMine || !canMove)
            return;

        if (moveVector.magnitude > 0)
            HudManager.instance.ChangeCrosshair(1);
        else
            HudManager.instance.ChangeCrosshair(0);

        if (runInput && runCharge > 0)
        {
            isRunning = true;
            runCharge -= Time.deltaTime;
        }
        else if (isRunning)
        {
            isRunning = false;

            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = RunRecharge();
            StartCoroutine(coroutine);
        }

        if (isRunning)
            currentSprintMultiplayer = sprintMultiplier;
        else
            currentSprintMultiplayer = 1;
    }
    #endregion
    private void FixedUpdate()
    {
        if (!pv.IsMine || !canMove)
            return;

        Move();
    }
    void Move()
    {
        transform.Translate(moveVector * speed * currentSprintMultiplayer * Time.fixedDeltaTime);
    }
    void DoAnim(Animator yourAnim, Animator othersAnim, string animName, int triggerOrBoolOrFloat, bool setBool, float setFloat)
    {
        if (pv.IsMine)
        {
            switch (triggerOrBoolOrFloat)
            {
                case 0:
                    //trigger
                    yourAnim.SetTrigger(animName);
                    othersAnim.SetTrigger(animName);
                    break;
                case 1:
                    //bool
                    yourAnim.SetBool(animName, setBool);
                    othersAnim.SetBool(animName, setBool);
                    break;
                case 2:
                    //Float
                    yourAnim.SetFloat(animName, setFloat);
                    othersAnim.SetFloat(animName, setFloat);
                    break;
            }
        }
    }
}
