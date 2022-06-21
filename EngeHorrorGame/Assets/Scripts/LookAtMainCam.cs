using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMainCam : MonoBehaviour
{
    private void Update()
    {
        transform.LookAt(Camera.main.transform.position);
    }
}
