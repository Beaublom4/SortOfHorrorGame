using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSync : MonoBehaviour
{
    public void SyncAttack()
    {
        GetComponentInParent<BaseEnemy>().HitBox();
    }
}
