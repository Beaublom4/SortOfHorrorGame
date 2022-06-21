using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteract : MonoBehaviour
{
    [SerializeField] bool front;
    [SerializeField] bool right;

    public void Interact()
    {
        GetComponentInParent<Door>().Open(front, right);
    }
}
