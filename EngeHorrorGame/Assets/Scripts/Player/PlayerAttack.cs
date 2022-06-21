using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance;
    PhotonView pv;
    [SerializeField] LayerMask hittableMask, damageableMask;

    int currentSlot = 0;
    [HideInInspector] public bool canAttack = true;

    [Header("Gun")]
    [SerializeField] int pistolDamage;
    [SerializeField] float reloadTime;
    [SerializeField] float fireRate;
    [SerializeField] int currentMag;
    [SerializeField] int magSize;
    [SerializeField] Item ammoItem;

    [SerializeField] GameObject bulletImpactPrefab;

    [SerializeField] Transform[] muzzleFlashLoc, bulletEffectLoc;
    [SerializeField] ParticleSystem muzzleFlash, bulletEffect;

    bool canShoot = true;
    float shootTimer;
    float reloadTimer;
    bool reloading = false;

    [Header("Melee")]
    [SerializeField] int meleeDamage;
    [SerializeField] float meleeCooldown;
    [SerializeField] Transform meleeLocation;

    bool canMelee = true;
    [SerializeField] float meleeTimer;

    [Header("Objects")]
    [SerializeField] GameObject yourWeaponsHolder;
    [SerializeField] GameObject[] othersWeaponsHolderObjs;
    [SerializeField] GameObject yourLight;
    [SerializeField] GameObject otherLight;

    [SerializeField] GameObject[] yourWeapons;
    [SerializeField] GameObject[] othersWeapons;

    [Header("Animators")]
    [SerializeField] Animator firstPersonAnim;
    [SerializeField] Animator gunAnim;
    [SerializeField] Animator fullPersonAnim;
    [SerializeField] Animator gunAnim2;

    #region Inputs
    public void ShootInput(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || !canAttack)
            return;

        if (context.performed)
        {
            switch (currentSlot) 
            {
                case 0:
                    Shoot();
                    break;
                case 1:
                    Melee();
                    break;
            }
        }
    }
    public void AimInput(InputAction.CallbackContext context)
    {
        if (!pv.IsMine)
            return;

        if (context.performed)
        {
            Aim(true);
        }
        else if (context.canceled)
        {
            Aim(false);
        }
    }
    public void ReloadInput(InputAction.CallbackContext context)
    {
        if (!pv.IsMine)
            return;

        if (context.performed)
            Reload();
    }
    public void SwapWeaponInput(InputAction.CallbackContext context)
    {
        if (!pv.IsMine)
            return;

        if (context.performed)
        {
            SwapWeapon();
        }
            
    }
    #endregion
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            Instance = this;
            canAttack = true;
        }
    }
    private void Start()
    {
        if (pv.IsMine)
        {
            yourWeaponsHolder.SetActive(true);
            yourWeapons[0].SetActive(true);
            foreach(GameObject g in othersWeaponsHolderObjs)
            {
                g.layer = 7;
            }
            yourLight.SetActive(true);
        }
        else
        {
            yourWeaponsHolder.SetActive(false);
            othersWeapons[0].SetActive(true);
            otherLight.SetActive(true);
        }
    }
    private void Update()
    {
        if (!pv.IsMine)
            return;

        if(!canShoot && shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }
        else if(!canShoot)
        {
            canShoot = true;
            shootTimer = 0;
        }

        if(!canMelee && meleeTimer > 0)
        {
            meleeTimer -= Time.deltaTime;
        }
        else if (!canMelee)
        {
            canMelee = true;
            meleeTimer = 0;
        }

        if (reloading && reloadTimer > 0)
        {
            reloadTimer -= Time.deltaTime;
        }
        else if(reloading)
        {
            reloading = false;

            int ammoNeeded = magSize - currentMag;
            int ammoGot = InventoryManager.Instance.GetItemCount(0, 4);

            currentMag += ammoNeeded;
            InventoryManager.Instance.RemoveItem(4, ammoNeeded);
            reloadTimer = 0;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((transform.position + transform.forward), .5f);
    }
    void Aim(bool toggle)
    {
        DoAnim(firstPersonAnim, fullPersonAnim, "Aim", 1, toggle);
    }
    void Shoot()
    {
        if (!pv.IsMine)
            return;
        if (reloading)
            return;
        if(currentMag <= 0)
        {
            //empty shot
            return;
        }
        shootTimer = fireRate;
        canShoot = false;

        DoAnim(firstPersonAnim, fullPersonAnim, "Fire", 0, false);
        DoAnim(gunAnim, gunAnim2, "Fire", 0, false);

        /*
        foreach(Transform t in muzzleFlashLoc)
        {
            PhotonNetwork.Instantiate(muzzleFlash.name, t.position, t.rotation);
        }
        foreach(Transform t in bulletEffectLoc)
        {
            PhotonNetwork.Instantiate(bulletEffect.name, t.position, t.rotation);
        }
        */

        currentMag--;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 200, hittableMask, QueryTriggerInteraction.Ignore))
        {
            if(hit.collider.tag == "Enemy")
            {
                hit.collider.GetComponent<BaseEnemy>().GetHit(pistolDamage, transform.position);
                hit.collider.GetComponent<BaseEnemy>().HitParticle(hit.point, Quaternion.LookRotation(hit.normal));
            }
            else
            {
                BulletImpact(hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }
    [PunRPC]
    void BulletImpact(Vector3 loc, Quaternion rot)
    {
        GameObject impact = Instantiate(bulletImpactPrefab, loc, rot, null);
        Destroy(impact, 3);
    }
    void Reload()
    {
        if (!pv.IsMine)
            return;
        if (reloading)
            return;
        if(currentMag < magSize && InventoryManager.Instance.SearchForItem(0, 4))
        {
            DoAnim(firstPersonAnim, fullPersonAnim, "Reload", 0, false);
            reloading = true;
            reloadTimer = reloadTime;
        }
    }
    void Melee()
    {
        if (!pv.IsMine)
            return;
        if (!canMelee)
            return;

        DoAnim(firstPersonAnim, fullPersonAnim, "Fire", 0, false);
        canMelee = false;
        meleeTimer = meleeCooldown;
    }
    public void MeleeHitBox()
    {
        Collider[] hits = Physics.OverlapSphere(meleeLocation.position, 1f, damageableMask);
        if (hits.Length == 0)
            return;

        foreach (Collider c in hits)
        {
            if (c.GetComponent<BaseEnemy>())
                c.GetComponent<BaseEnemy>().GetHit(meleeDamage, transform.position);
        }
    }
    void SwapWeapon()
    {
        if (!pv.IsMine)
            return;

        pv.RPC("RPC_SwapWeapon", RpcTarget.Others);
        switch (currentSlot)
        {
            case 0:
                yourWeapons[currentSlot].SetActive(false);
                currentSlot = 1;
                DoAnim(firstPersonAnim, fullPersonAnim, "Knife", 1, true);
                yourWeapons[currentSlot].SetActive(true);
                break;
            case 1:
                yourWeapons[currentSlot].SetActive(false);
                currentSlot = 0;
                DoAnim(firstPersonAnim, fullPersonAnim, "Knife", 1, false);
                yourWeapons[currentSlot].SetActive(true);
                break;
        }
    }
    [PunRPC]
    void RPC_SwapWeapon()
    {
        switch (currentSlot)
        {
            case 0:
                othersWeapons[currentSlot].SetActive(false);
                currentSlot = 1;
                othersWeapons[currentSlot].SetActive(true);
                break;
            case 1:
                othersWeapons[currentSlot].SetActive(false);
                currentSlot = 0;
                othersWeapons[currentSlot].SetActive(true);
                break;
        }
    }
    void DoAnim(Animator yourAnim, Animator othersAnim, string animName, int triggerOrBool, bool setBool)
    {
        if (pv.IsMine)
        {
            switch (triggerOrBool)
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
            }
        }
    }
}
