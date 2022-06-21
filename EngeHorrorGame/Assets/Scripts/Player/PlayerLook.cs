using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Voice;
using Photon.Voice.Unity;

public class PlayerLook : MonoBehaviour
{
    public static PlayerLook Instance;
    PhotonView pv;

    [Header("Looking")]
    public bool canLook;
    public float sensitivity;

    Transform mainObj;
    public Vector2 mouseInput;
    float xRot;

    [Header("Interacting")]
    [SerializeField] LayerMask interactMask;
    [SerializeField] float interactRange, interactSize;
    RaycastHit hit;

    [Header("Player model")]
    [SerializeField] SkinnedMeshRenderer fullModelRenderer;
    [SerializeField] SkinnedMeshRenderer firstPersonModelRenderer;
    [SerializeField] Material[] fullModelMaterials, firstPersonMaterials;
    [SerializeField] GameObject glasses;

    #region Inputs
    public void LookInput(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        mouseInput = new Vector2(input.x, -input.y);

        if (!pv.IsMine || !canLook)
            return;

        xRot += mouseInput.y * sensitivity * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 90);
        Vector3 targetRot = transform.eulerAngles;
        targetRot.x = xRot;
        transform.eulerAngles = targetRot;

        mainObj.Rotate(0, mouseInput.x * sensitivity * Time.deltaTime, 0);
    }
    public void InteractInput(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || !canLook)
            return;

        if (context.performed)
        {
            Interact();
        }
        else if (context.canceled)
        {
            StopInteract();
        }
    }
    public void ToggleInventory(InputAction.CallbackContext context)
    {
        if (!pv.IsMine)
            return;

        if (context.performed)
        {
            if (PlayerHealth.Instance.dead)
                return;
            InventoryManager.Instance.ToggleInventory(this, GetComponentInParent<PlayerMove>(), GetComponent<PlayerAttack>());
        }
    }
    public void ToggleSettings(InputAction.CallbackContext context)
    {
        if (!pv.IsMine)
            return;

        if (context.performed)
        {
            HudManager.instance.ToggleSettings(this, GetComponentInParent<PlayerMove>(), GetComponent<PlayerAttack>());
     
        }
    }
    public void ToggleVoice(InputAction.CallbackContext context)
    {
        if (!pv.IsMine)
            return;

        if (context.performed)
        {
            VoiceToggle(true);
        }
        else if (context.canceled)
        {
            VoiceToggle(false);
        }
    }
    #endregion
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        mainObj = transform.root;
        if (pv.IsMine)
        {
            Instance = this;
            pv.RPC(nameof(SetPlayerLook), RpcTarget.AllBuffered, PhotonManager.photonId);
        }
    }
    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (!pv.IsMine)
        {
            cam.enabled = false;
            GetComponent<AudioListener>().enabled = false;
            return;
        }
        cam.gameObject.tag = "MainCamera";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Interact()
    {
        Physics.Raycast(transform.position, transform.forward, out hit, interactRange, interactMask);

        if (hit.collider == null)
        {
            Physics.SphereCast(transform.position, interactSize, transform.forward, out hit, interactRange, interactMask);
        }

        if (hit.collider == null)
            return;

        RaycastHit hitCheck;
        if (!Physics.Linecast(transform.position, hit.collider.transform.position, out hitCheck, interactMask))
            return;
        if (hitCheck.collider != hit.collider)
            return;

        if (hit.collider.tag == "Door")
        {
            InventoryManager.Instance.interactObj = hit.collider.GetComponentInParent<Door>();
            Door doorScript = hit.collider.GetComponent<DoorInteract>().GetComponentInParent<Door>();
            if (doorScript.lockedWithKey)
            {
                InventoryManager.Instance.ToggleInventory(this, GetComponentInParent<PlayerMove>(), GetComponent<PlayerAttack>());
            }
            else
            {
                hit.collider.GetComponent<DoorInteract>().Interact();
            }
        }
        else if (hit.collider.tag == "Item")
        {
            ItemPickUp _item = hit.collider.GetComponent<ItemPickUp>();
            InventoryManager.Instance.AddItem(_item);
        }
        else if (hit.collider.GetComponent<PianoPuzzle>())
        {
            hit.collider.GetComponent<PianoPuzzle>().Interact();
            InventoryManager.Instance.interactObj = hit.collider.GetComponent<PianoPuzzle>();
            InventoryManager.Instance.ToggleInventory(this, GetComponentInParent<PlayerMove>(), GetComponent<PlayerAttack>());
        }
        else if (hit.collider.tag == "ReviveCol")
        {
            Debug.Log("revive");
            hit.collider.GetComponentInParent<PlayerHealth>().StartRevive(this);
        }
        else if(hit.collider.tag == "PuzzleDisc")
        {
            hit.collider.GetComponentInParent<PaintingPuzzle>().Interact(hit.collider.transform);
        }
        else if(hit.collider.tag == "Drawer")
        {
            hit.collider.GetComponent<Animator>().SetTrigger("Toggle");
        }
        else if(hit.collider.tag == "Lever")
        {
            hit.collider.GetComponent<Lever>().Pull();
        }
    }
    void StopInteract()
    {
        if (hit.collider == null)
            return;

        if(hit.collider.tag == "ReviveCol")
        {
            hit.collider.GetComponentInParent<PlayerHealth>().StopRevive();
        }
    }
    [PunRPC]
    void SetPlayerLook(int _photonId)
    {
        fullModelRenderer.material = fullModelMaterials[_photonId];
        firstPersonModelRenderer.material = firstPersonMaterials[_photonId];
        if (!pv.IsMine)
        {
            switch (_photonId)
            {
                case 1:
                    glasses.SetActive(true);
                    break;
            }
        }
    }
    public void VoiceToggle(bool toggle)
    {
        Debug.Log("Voice: " + toggle);
        GetComponent<Recorder>().TransmitEnabled = toggle;
    }
}
