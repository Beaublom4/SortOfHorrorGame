using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ZombieEnemy : BaseEnemy
{
    [PunRPC]
    public override void RPC_GetHit(int _damage, Vector3 _shotPos)
    {
        base.RPC_GetHit(_damage, _shotPos);
    }
}
