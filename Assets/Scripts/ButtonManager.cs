using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public void play(Text buttonText) {
        AudioManager.Instance.PlayClickSFX();
        SceneManager.LoadScene("Game");
        buttonText.text = "Loading...";
    }

    public void Menu() {
        AudioManager.Instance.PlayClickSFX();
        SceneManager.LoadScene("Menu");
    }

    public void Restart() {
        AudioManager.Instance.PlayClickSFX();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
