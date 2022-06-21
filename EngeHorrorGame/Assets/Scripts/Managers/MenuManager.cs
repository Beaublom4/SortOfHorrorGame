using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] Transform cam;
    [SerializeField] MenuInfo[] menus;
    [SerializeField] Transform[] menuCamPos;

    [SerializeField] float moveTime;
    Transform wantedCamPos;

    private void Awake()
    {
        Instance = this;
    }
    public void SelectMenu(string _name)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == _name)
            {
                menus[i].gameObject.SetActive(true);
                //MoveCam(i);
            }
            else
            {
                menus[i].gameObject.SetActive(false);
            }
        }
    }
    IEnumerator MoveCam(int i)
    {
        wantedCamPos = menuCamPos[i];
        float t = moveTime;
        Vector3 startPos = cam.position;
        while(t >= 0)
        {
            cam.position = Vector3.Lerp(startPos, wantedCamPos.position, t / moveTime);
            t -= Time.deltaTime;
            yield return null;
        }
    }
}
