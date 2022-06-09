using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Policies;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField]
    private SpaceshipController[] spaceshipControllers;
    public int maxStep = 20000;
    public int maxLap = 3;

    [SerializeField]
    private RaceTrackController raceTrackController;

    [SerializeField]
    private string[] shipNames;

    [SerializeField]
    private Color[] shipColors;

    private int stepTimer = 0;

    public bool isTraining = true;

    [SerializeField]
    private string[] trackNames;

    private int currentTrack = 0;

    private List<int> trackIndex;

    private float timer = 0;

    public bool isGameOver = false;
    
    private void Awake()
    {
        List<int> randomNameIndexes = new List<int>();
        List<int> randomColorIndexes = new List<int>();

        while(randomNameIndexes.Count < spaceshipControllers.Length - 1)
        {
            int index = Random.Range(0, shipNames.Length);
            if (!randomNameIndexes.Contains(index))
            {
                randomNameIndexes.Add(index);
            }
        }

        while (randomColorIndexes.Count < spaceshipControllers.Length - 1)
        {
            int index = Random.Range(0, shipColors.Length);
            if (!randomColorIndexes.Contains(index))
            {
                randomColorIndexes.Add(index);
            }
        }

        int i = 0;
        foreach (var spaceshipController in spaceshipControllers)
        {
            spaceshipController.onFinishedRace.AddListener((winner) =>
            {
                if (isTraining)
                {
                    endEpisodeForAll(winner);
                } else if (winner.isPlayer)
                {
                    SetGameOver(winner);
                }
            });

            if (!isTraining && !spaceshipController.isPlayer)
            {
                spaceshipController.gameObject.name = shipNames[randomNameIndexes[i]];
                spaceshipController.GetComponent<SpriteRenderer>().color = shipColors[randomColorIndexes[i]];
                i++;
            }
        }

        currentTrack = 0;
        trackIndex = new List<int>();
        for (int j = 0; j < trackNames.Length; j++)
        {
            trackIndex.Add(j);
        }
        trackIndex.Sort((a, b) => Random.Range(-1, 1));

        isGameOver = false;
        SetTrack();
    }

    private void Update()
    {
        SetPosition();

        timer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (isTraining)
        {
            stepTimer++;

            if (stepTimer >= maxStep)
            {
                stepTimer = 0;
                endEpisodeForAll();
            }
        }
    }

    private void SetTrack()
    {
        string trackName = isTraining ? trackNames[trackIndex[currentTrack++]]
                            : trackNames[Random.Range(0, trackNames.Length)];

        raceTrackController.SetupTrack(trackName);
        raceTrackController.ResetObjects();

        if (currentTrack >= trackIndex.Count)
        {
            currentTrack = 0;
            trackIndex.Sort((a, b) => Random.Range(-1, 1));
        }
    }

    private void endEpisodeForAll(SpaceshipController winner = null)
    {
        if (winner == null)
        {
            Debug.Log("End episode, timer reached max step");
        }
        else
        {
            Debug.Log("End episode, " + winner.name + " won");
        }

        SetTrack();

        foreach (SpaceshipController spaceshipController in spaceshipControllers)
        {
            if (spaceshipController == winner)
            {
                spaceshipController.AddReward(1f);
            }
            else
            {
                spaceshipController.AddReward(-1 * ((spaceshipController.currentPosition - 1) / 2f));
            }

            spaceshipController.EndEpisode();
        }
    }

    private void SetPosition()
    {
        var positions = spaceshipControllers.ToList();

        positions.Sort((a, b) =>
        {
            if (a.currentLap != b.currentLap)
            {
                return a.currentLap.CompareTo(b.currentLap);
            }
            else if (a.currectCheckpointOrder != b.currectCheckpointOrder)
            {
                return a.currectCheckpointOrder.CompareTo(b.currectCheckpointOrder);
            }
            else
            {
                return b.GetDistanceToNextCheckpoint().CompareTo(a.GetDistanceToNextCheckpoint());
            }
        });

        for (int i = 0; i < positions.Count; i++)
        {
            positions[i].currentPosition = positions.Count - i;
        }
    }

    private void SetGameOver(SpaceshipController player) {
        if (isGameOver)
        {
            return;
        }

        if (player.currentPosition == 1)
        {
            AudioManager.Instance.PlayWinSFX();
        } else {
            AudioManager.Instance.PlayLoseSFX();
        }

        isGameOver = true;
        GameUIManager.Instance.GameOver(player.currentPosition, timer);
        player.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
    }
}
