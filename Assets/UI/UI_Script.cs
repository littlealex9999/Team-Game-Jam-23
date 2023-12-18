using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UI_Script : MonoBehaviour
{

    public Image healthBar;
    public Image ammoBar;
    public Image timeBar;
    public float tempHealthValue = 1;
    public float tempAmmoValue = 1;
    public float tempTimeValue = 1;


    // Start is called before the first frame update
    void Start()
    {
        healthBar.fillAmount = tempHealthValue; //somehow the value of health has to convert to between 1 and 0 and idk how its set up so im leaving it 
        ammoBar.fillAmount = tempAmmoValue;
        timeBar.fillAmount = tempTimeValue;
    }

    // Update is called once per frame
    void Update()
    {
        tempHealthValue -= 1f / 30.0f * Time.deltaTime; //just make it go down over 30 seconds to look sick while testing idk man
        healthBar.fillAmount = tempHealthValue;
        tempAmmoValue -= 1f / 30.0f * Time.deltaTime;
        ammoBar.fillAmount = tempAmmoValue;
        tempTimeValue -= 1f / 30.0f * Time.deltaTime;
        timeBar.fillAmount = tempTimeValue;
    }

    public void StartGame()
    {
        //SceneManager.LoadScene(1); //but the actual game scene idfk
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResumeGame()
    {
        //timescale stuff probably idk how youre doing the pause stuff and im not gonna fuck around with it 
    }
}
