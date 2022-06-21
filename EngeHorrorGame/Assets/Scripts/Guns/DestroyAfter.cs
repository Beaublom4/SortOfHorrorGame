using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] float destroyInSec;
    private void Start()
    {
        Destroy(gameObject, destroyInSec);   
    }
}
