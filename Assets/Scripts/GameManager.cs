using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Player player;

    [Space]
    public Image healthUI;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI ammoHeldText;

    void Awake()
    {
        if (instance != null) Destroy(this);
        else instance = this;
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (player != null) {
            if (healthUI) healthUI.fillAmount = player.health / player.maxHealth;
            if (ammoText) ammoText.text = player.currentAmmoClip + " / " + player.maxAmmoClip;
            if (ammoHeldText) ammoHeldText.text = player.currentAmmoHeld.ToString();
        }
    }

    public void GameOver()
    {

    }
}
