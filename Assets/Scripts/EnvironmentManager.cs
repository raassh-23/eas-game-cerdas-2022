using System.Linq;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField]
    private SpaceshipController[] spaceshipControllers;
    public int maxStep = 20000;
    public int maxLap = 3;

    [SerializeField]
    private RaceTrackController raceTrackController;

    private int timer = 0;

    public bool isTraining = true;

    [SerializeField]
    private string[] trackNames;

    private void Awake()
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

        SetTrack();
    }

    private void Update() {
        SetPosition();
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

    private void SetTrack() {
        string trackName = trackNames[Random.Range(0, trackNames.Length)];
        raceTrackController.SetupTrack(trackName);
        raceTrackController.ResetObjects();
    }

    private void endEpisodeForAll(SpaceshipController winner = null) {
        if (winner == null) {
            Debug.Log("End episode, timer reached max step");
        } else {
            Debug.Log("End episode, " + winner.name + " won");
        }

        SetTrack();

        foreach (SpaceshipController spaceshipController in spaceshipControllers) {
            if (spaceshipController == winner) {
                spaceshipController.AddReward(1f);
            } else {
                spaceshipController.AddReward(-1*((spaceshipController.currentPosition - 1)/2f));
            }

            spaceshipController.EndEpisode();
        }
    }

    private void SetPosition() {
        var positions = spaceshipControllers.ToList();

        positions.Sort((a, b) => {
            if (a.currentLap != b.currentLap) {
                return a.currentLap.CompareTo(b.currentLap);
            } else if (a.nextCheckpoint.order != b.nextCheckpoint.order) {
                return a.nextCheckpoint.order.CompareTo(b.nextCheckpoint.order);
            } else {
                return b.GetDistanceToNextCheckpoint().CompareTo(a.GetDistanceToNextCheckpoint());
            }
        });
        
        for (int i = 0; i < positions.Count; i++) {
            positions[i].currentPosition = positions.Count - i;
        }
    }
}
