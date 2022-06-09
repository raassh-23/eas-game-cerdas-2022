using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playingPanel;

    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private Text gameOverText;

    [SerializeField]
    private Text positionText;

    [SerializeField]
    private Text timeText;

    public static GameUIManager Instance { get; private set; }

    [SerializeField]
    private AudioClip _clickSFX;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start() {
        gameOverPanel.SetActive(false);
        playingPanel.SetActive(true);
    }

    public void GameOver(int position, float time) {
        playingPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        gameOverText.text = position == 1 ? "You Win." : "You Lose.";

        string positionString = "";

        switch (position) {
            case 1:
                positionString = "1st";
                break;
            case 2:
                positionString = "2nd";
                break;
            case 3:
                positionString = "3rd";
                break;
            default:
                positionString = position.ToString() + "th";
                break;
        }

        positionText.text = positionString + " Place";
        float minutes = Mathf.Floor(time / 60);
        float seconds = time % 60;
        timeText.text = string.Format("Finish Time: {0:00}:{1:00}", minutes, seconds);
    }
}
