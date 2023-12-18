using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Image healthUI;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI ammoHeldText;

    void Start()
    {
        
    }

    // Update is called once per frame
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
}
