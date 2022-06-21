using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverPuzzle : MonoBehaviour
{
    [SerializeField] int wantedLevers;
    [SerializeField] int leversPulled;

    public void AddPull()
    {
        leversPulled++;
        if(leversPulled >= wantedLevers)
        {
            Debug.Log("Good");
        }
    }
}
