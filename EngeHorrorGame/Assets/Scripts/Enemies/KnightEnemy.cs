using Photon.Pun;
using System.Collections;
using UnityEngine;

public class KnightEnemy : BaseEnemy
{
    [Header("Knight")]
    [SerializeField] bool defenceType = true;
    [SerializeField] bool attackType;
    [Space]
    [SerializeField] Transform awakeLoc;
    [SerializeField] float awakeDistance;
    [SerializeField] LayerMask playerMask;
    [SerializeField] GameObject liveObj, deadObj;

    [Header("Blocking")]
    [SerializeField] float waitForBlockTime;
    [SerializeField] float blockTime;
    [SerializeField] BoxCollider weaponCol;
    bool blocking;

    bool lockedMode = true;
    bool attacking;

    public override void Awake()
    {
        base.Awake();
        wanderMode = false;
        if (defenceType)
            weaponCol.enabled = true;
    }
    public override void Start()
    {
        if (!pv.IsMine)
            return;
        StopAgent();
    }
    public override void Update()
    {
        if (!pv.IsMine || dead)
            return;
        base.Update();
        CheckIfInAwakeDist();
    }
    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        //Awake loc
        if (lockedMode)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(awakeLoc.position, awakeDistance);
        }
    }
    void CheckIfInAwakeDist()
    {
        if (!lockedMode)
            return;

        if( Physics.CheckSphere(awakeLoc.position, awakeDistance, playerMask))
        {
            AwakeKnight();
        }
    }
    void AwakeKnight()
    {
        if (defenceType)
        {
            if (!blocking)
            {
                blocking = true;
                Invoke(nameof(BlockInvoke), waitForBlockTime);
            }
        }
        else if (attackType)
        {
            if (!attacking)
            {
                attacking = true;
                Invoke(nameof(AttackInvoke), 1);
            }
        }
    }
    void BlockInvoke()
    {
        StartCoroutine(Block());
    }
    void AttackInvoke()
    {
        anim.SetTrigger("Activated");
        lockedMode = false;
        ChangeMode("Attack");
        EnableAgent();
    }
    IEnumerator Block()
    {
        anim.SetBool("Block", true);
        yield return new WaitForSecondsRealtime(blockTime);
        anim.SetBool("Block", false);
        blocking = false;
    }
    public override void GetHit(int _damage, Vector3 _shotPos)
    {
        if (defenceType)
            return;
        base.GetHit(_damage, _shotPos);
    }
    [PunRPC]
    public override void RPC_GetHit(int _damage, Vector3 _shotPos)
    {
        base.RPC_GetHit(_damage, _shotPos);
    }
    public override void Dead()
    {
        liveObj.SetActive(false);
        deadObj.SetActive(true);
        base.Dead();
    }
}
