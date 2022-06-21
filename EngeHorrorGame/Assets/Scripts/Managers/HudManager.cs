using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HudManager : MonoBehaviour
{
    public static HudManager instance;

    [Header("Settings")]
    [SerializeField] GameObject settingsObj;
    [SerializeField] Volume volume;

    [Header("Crosshair")]
    [SerializeField] float crossHairSpeed, passiveDist, moveDist;
    [SerializeField] RectTransform crosshairKnob;
    [SerializeField] RectTransform[] crosshairPoints;
    int crosshairIndex;
    float currentDist;

    [Header("Health")]
    [SerializeField] GameObject bloodPanel;
    IEnumerator bloodCoroutine;

    [Header("Camera")]
    public RawImage imageRenderer;

    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        //MoveCrosshair();
    }
    public void ToggleSettings(PlayerLook lookScript, PlayerMove moveScript, PlayerAttack attackScript)
    {
        bool toggle = !settingsObj.activeSelf;
        settingsObj.SetActive(toggle);
        if (!PlayerHealth.Instance.dead)
        {
            lookScript.canLook = !toggle;
            moveScript.Stop();
            moveScript.canMove = !toggle;
            attackScript.canAttack = !toggle;
        }
        if (!toggle)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void ChangeSens(float f)
    {
        PlayerLook.Instance.sensitivity = f;
    }
    public void ChangeBrightness(float f)
    {
        LiftGammaGain gamma;
        volume.profile.TryGet<LiftGammaGain>(out gamma);
        gamma.gamma.Override(new Vector4(1f, 1f, 1f, f));
    }
    public void ChangeCrosshair(int newIndex)
    {
        crosshairIndex = newIndex;
        switch(crosshairIndex)
        {
            case 0:
                currentDist = passiveDist;
                break;
            case 1:
                currentDist = moveDist;
                break;
        }
    }
    void MoveCrosshair()
    {
        crosshairPoints[0].anchoredPosition = Vector2.Lerp(crosshairPoints[0].anchoredPosition, new Vector2(-currentDist, currentDist), crossHairSpeed * Time.deltaTime);
        crosshairPoints[1].anchoredPosition = Vector2.Lerp(crosshairPoints[1].anchoredPosition, new Vector2(currentDist, currentDist), crossHairSpeed * Time.deltaTime);
        crosshairPoints[2].anchoredPosition = Vector2.Lerp(crosshairPoints[2].anchoredPosition, new Vector2(currentDist, -currentDist), crossHairSpeed * Time.deltaTime);
        crosshairPoints[3].anchoredPosition = Vector2.Lerp(crosshairPoints[3].anchoredPosition, new Vector2(-currentDist, -currentDist), crossHairSpeed * Time.deltaTime);
    }
    public void GetHit(float currentHealth, float maxHealth)
    {
        Image i = bloodPanel.GetComponent<Image>();
        float onePercent = maxHealth / maxHealth;
        float percentage = currentHealth * onePercent;
        percentage = 100 - percentage;
        percentage *= .5f;
        percentage /= 100;
        float f = (255 / 255) * (percentage);
        i.color = new Color(i.color.r, i.color.g, i.color.b, f);

        if (bloodCoroutine != null)
            StopCoroutine(bloodCoroutine);
        bloodCoroutine = BloodFade(i);
        StartCoroutine(bloodCoroutine);
    }
    IEnumerator BloodFade(Image i)
    {
        float timer = 1;
        Color startColor = i.color;
        Color newColor = new Color(i.color.r, i.color.g, i.color.b, 0);
        while(timer >= 0)
        {
            i.color = Color.Lerp(newColor, startColor, timer);
            timer -= Time.deltaTime;
            yield return null;
        }
    }
    public void DisplayMessage(string message, float time)
    {

    }
    IEnumerator DisplayMessageCoroutine()
    {

    }
}
