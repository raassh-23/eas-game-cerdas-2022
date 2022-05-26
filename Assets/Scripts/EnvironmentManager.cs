using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField]
    private SpaceshipController[] spaceshipControllers;
    public int maxStep = 20000;
    public int maxLap = 3;

    [SerializeField]
    private RaceTrackController raceTrackController;

    public int timer = 0;

    public bool isTraining = true;

    private void Start()
    {
        foreach (var spaceshipController in spaceshipControllers)
        {
            spaceshipController.onFinishedRace.AddListener((winner) =>
            {
                if (isTraining)
                {
                    endEpisodeForAll(winner);
                }
            });
        }
        Debug.Log("EnvironmentManager Start");
    }

    private void FixedUpdate() {
        if (isTraining) {
            timer++;
            if (timer >= maxStep) {
                timer = 0;
                endEpisodeForAll();
            }
        }
    }

    private void endEpisodeForAll(SpaceshipController winner = null) {
        if (winner == null) {
            Debug.Log("End episode, timer reached max step");
        } else {
            Debug.Log("End episode, " + winner.name + " won");
        }

        raceTrackController.ResetObjects();

        foreach (SpaceshipController spaceshipController in spaceshipControllers) {
            if (spaceshipController == winner) {
                spaceshipController.AddReward(1f);
            } else {
                spaceshipController.AddReward(-1f);
            }

            spaceshipController.EndEpisode();
        }
    }
}
