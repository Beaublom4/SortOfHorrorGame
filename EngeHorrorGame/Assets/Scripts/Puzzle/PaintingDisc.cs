using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingDisc : MonoBehaviour
{
    public int discIndex;
    public void AddIndex(int maxInd)
    {
        discIndex++;
        if (discIndex >= maxInd)
            discIndex = 0;
    }
}
