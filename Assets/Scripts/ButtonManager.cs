using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public void play(Text buttonText) {
        SceneManager.LoadScene("Game");
        buttonText.text = "Loading...";
    }

    public void Menu() {
        SceneManager.LoadScene("Menu");
    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
