using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingPuzzle : MonoBehaviour
{
    [SerializeField] float rotateTime;
    [SerializeField] float rotateAmount;
    [SerializeField] int maxRotTimes;

    [SerializeField] string code;
    [SerializeField] PaintingDisc[] discs;

    Transform currentDisc;

    private void Start()
    {
        maxRotTimes = (int)(360 / rotateAmount);
    }
    public void Interact(Transform _disc)
    {
        if (currentDisc != null)
            return;

        currentDisc = _disc;
        currentDisc.Rotate(rotateAmount, 0, 0);
        currentDisc.GetComponent<PaintingDisc>().AddIndex(maxRotTimes);
        currentDisc = null;

        int goodNumbers = 0;
        List<PaintingDisc> discsToCheck = new List<PaintingDisc>();
        foreach(PaintingDisc disc in discs)
        {
            discsToCheck.Add(disc);
        }

        for (int i = 0; i < code.Length; i++)
        {
            int codeIndexint = int.Parse(code.Substring(i, 1));
            foreach (PaintingDisc disc in discsToCheck)
            {
                if (disc.discIndex == codeIndexint)
                {
                    goodNumbers++;
                    discsToCheck.Remove(disc);
                    break;
                }
            }
        }
        Debug.Log("Good numbers: " + goodNumbers);
        if(goodNumbers >= code.Length)
            Debug.Log("Good");

        //StartCoroutine(RotateDisc());
    }
    IEnumerator RotateDisc()
    {
        

        //Debug.Log(currentDisc.eulerAngles);
        //float startRot = currentDisc.eulerAngles.x;
        //float endRot = startRot + rotateAmount;
        //float rot = 0;
        //float timer = 0;

        //while (timer < rotateTime)
        //{
        //    rot = Mathf.Lerp(startRot, endRot, timer / rotateTime);
        //    currentDisc.rotation = Quaternion.Euler(rot, currentDisc.rotation.y, currentDisc.rotation.z);

        //    timer += Time.deltaTime;
            yield return null;
        //}
        //currentDisc.rotation = Quaternion.Euler(endRot, 0, 0);

        currentDisc = null;
    }
}