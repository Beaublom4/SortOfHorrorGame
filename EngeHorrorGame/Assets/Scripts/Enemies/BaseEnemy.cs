using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class BaseEnemy : MonoBehaviour
{
    [HideInInspector] public PhotonView pv;
    [HideInInspector] public Animator anim;

    [Header("Path Finding")]
    [SerializeField] Vector3 destination;
    public Transform closestPlayer;

    [Header("Wandering")]
    public bool wanderMode = true;

    [Header("Search")]
    bool searchMode;

    [Header("Player detection")] 
    [SerializeField] float maxDetectionRange;
    [SerializeField] float visionConeDegrees;
    [SerializeField] Transform rotObj;
    [SerializeField] Vector3 faceOffset;

    NavMeshAgent agent;
    bool stopped;

    [Header("Health")]
    [HideInInspector] public bool dead;
    [SerializeField] float health;
    [SerializeField] float currentHealth;
    [SerializeField] GameObject hitParticle;

    [Header("Attacking")]
    [SerializeField] float attackRange;
    [SerializeField] float attackCooldown;
    bool attackMode;
    bool canAttack = true;

    [SerializeField] float damage;
    [SerializeField] float damageRadius;
    [SerializeField] Transform hitLoc;
    [SerializeField] LayerMask mask;

    public virtual void Awake()
    {
        pv = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        InvokeRepeating(nameof(GetClosestPlayerLoop), 0, 1);
        currentHealth = health;
    }
    public virtual void Start()
    {
        RandomWanderingPoint();
    }
    public virtual void Update()
    {
        if (!pv.IsMine)
            return;

        if (attackMode) 
        {
            if (closestPlayer)
                SetDestination(closestPlayer.position);
            else
                ChangeMode("Search");
        }

        UpdateDestination();
        CheckIfAtDestination();

        CheckIfInAttackRange();

        WalkingCheck();
    }
    public virtual void OnDrawGizmosSelected()
    {
        //View range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDetectionRange);
        //Small viw range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2);
        //Vision cone
        Gizmos.color = Color.blue;
        Vector3 dir1 = Quaternion.AngleAxis(visionConeDegrees / 2, Vector3.up) * transform.forward * maxDetectionRange;
        Gizmos.DrawRay(transform.position, dir1);
        Vector3 dir2 = Quaternion.AngleAxis(-visionConeDegrees / 2, Vector3.up) * transform.forward * maxDetectionRange;
        Gizmos.DrawRay(transform.position, dir2);
        //Wandering line
        Gizmos.color = Color.red;
        if (wanderMode)
            Gizmos.DrawLine(transform.position, destination);
    }
    public void ChangeMode(string mode)
    {
        switch (mode) 
        {
            case "Attack":
                attackMode = true;
                wanderMode = false;
                searchMode = false;
                break;
            case "Wander":
                attackMode = false;
                wanderMode = true;
                searchMode = false;
                break;
            case "Search":
                attackMode = false;
                wanderMode = false;
                searchMode = true;
                break;
            default:
                Debug.LogError("Wrong mode selected");
                break;
        }
    }
    #region Path finding
    public void SetDestination(Vector3 _pos)
    {
        destination = _pos;
    }
    void UpdateDestination()
    {
        if (stopped)
            return;
        if (destination == null)
        {
            agent.isStopped = true;
            return;
        }
        else
            agent.isStopped = false;

        agent.SetDestination(destination);
    }
    void CheckIfAtDestination()
    {
        if (!stopped && (wanderMode || searchMode))
        {
            if(agent.velocity.magnitude <= .1f)
            {
                if (searchMode)
                    ChangeMode("Wander");
                RandomWanderingPoint();
            }
        }
    }
    public void StopAgent()
    {
        stopped = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
    public void EnableAgent()
    {
        agent.isStopped = false;
        stopped = false;
    }
    public void GetClosestPlayerLoop()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();

        Transform newClosestPlayer = null;
        float newDistance = 0;
        foreach(PlayerHealth p in players)
        {
            if (p.dead)
                continue;
            float testDistance = Vector3.Distance(transform.position, p.transform.position);
            if (testDistance >= maxDetectionRange)
                continue;
            Color rayColor = Color.green;
            RaycastHit hit;
            if (Physics.Linecast(transform.position + faceOffset, p.transform.position, out hit, -5, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.tag != "Player")
                {
                    rayColor = Color.white;
                    Debug.DrawLine(transform.position + faceOffset, p.transform.position, rayColor, 1f);
                    continue;
                }
            }
            Debug.DrawLine(transform.position + faceOffset, p.transform.position, rayColor, 1f);

            rotObj.LookAt(new Vector3(p.transform.position.x, transform.position.y, p.transform.position.z));
            float angle = Quaternion.Angle(transform.rotation, rotObj.rotation);

            if (testDistance >= 2)
            {
                if (angle > visionConeDegrees / 2)
                    continue;
            }

            if(newClosestPlayer == null)
            {
                newClosestPlayer = p.transform;
                newDistance = testDistance;
            }
            else if(testDistance < newDistance)
            {
                newClosestPlayer = p.transform;
                newDistance = testDistance;
            }
        }
        closestPlayer = newClosestPlayer;

        if(closestPlayer != null)
        {
            ChangeMode("Attack");
        }
    }
    void RandomWanderingPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * maxDetectionRange;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, maxDetectionRange, 1);
        Vector3 finalPosition = hit.position;
        SetDestination(finalPosition);
    }
    #endregion
    #region Health
    public virtual void GetHit(int _damage, Vector3 _shotPos)
    {
        if (dead)
            return;
        pv.RPC(nameof(RPC_GetHit), RpcTarget.All, _damage, _shotPos);
    }
    public void HitParticle(Vector3 loc, Quaternion rot)
    {
        PhotonNetwork.Instantiate(hitParticle.name, loc, rot);
    }
    [PunRPC]
    public virtual void RPC_GetHit(int _damage, Vector3 _shotPos)
    {
        currentHealth -= _damage;
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponentInChildren<Animator>().SetTrigger("Hit");
            if (!attackMode)
            {
                Debug.DrawLine(transform.position + faceOffset, _shotPos, Color.cyan, 1);
                RaycastHit hit;
                if (Physics.Linecast(transform.position + faceOffset, _shotPos, out hit, -5, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.tag == "Player")
                    {
                        SetDestination(_shotPos);
                    }
                }
            }
        }
        if (currentHealth <= 0)
        {
            Dead();
        }
    }
    public virtual void Dead()
    {
        dead = true;
        StopAgent();
        GetComponent<Collider>().enabled = false;
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponentInChildren<Animator>().SetBool("Dead", true);
            StartCoroutine(DeadTimer());
        }
    }
    IEnumerator DeadTimer()
    {
        yield return new WaitForSeconds(5);
        PhotonNetwork.Destroy(gameObject);
    }
    #endregion
    #region Attacking
    public void HitBox()
    {
        Collider[] cols = Physics.OverlapSphere(hitLoc.position, damageRadius, mask);
        if (cols.Length == 0)
            return;

        foreach(Collider col in cols)
        {
            col.GetComponent<PlayerHealth>().GetHit(damage);
        }
    }
    void CheckIfInAttackRange()
    {
        if (!attackMode)
            return;
        if (destination == null || stopped)
            return;
        if (!canAttack)
            return;

        bool isAttacking = false;
        if (Vector3.Distance(transform.position, destination) <= attackRange && !stopped)
        {
            isAttacking = true;
            canAttack = false;
            anim.SetTrigger("Attack");
            StartCoroutine(AttackCooldown());
        }
        else if (stopped)
        {
            isAttacking = false;
        }

        if (isAttacking)
        {
            transform.LookAt(new Vector3(destination.x, transform.position.y, destination.z));
        }
    }
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    #endregion
    #region Animating
    void WalkingCheck()
    {
        if (agent.velocity.magnitude > .1f)
            anim.SetBool("Walking", true);
        else
            anim.SetBool("Walking", false);
    }
    #endregion
}
