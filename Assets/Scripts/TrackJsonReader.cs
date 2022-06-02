using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TrackJsonReader
{
    public static string root = "Tracks/";

    public static Track LoadTrack(string trackName)
    {
        TextAsset json = Resources.Load<TextAsset>(root + trackName);
        return JsonUtility.FromJson<Track>(json.text);
    }
}

[Serializable]
public class Track {
    public List<Vector2> inner;
    public List<Vector2> outer;
    public List<CheckPoint> checkpoints;
    public List<StartPoint> starts;
}

[Serializable]
public class CheckPoint {
    public int order;
    public Vector2 point;
    public float angle;
    public float length;
}

[Serializable]
public class StartPoint {
    public Vector2 point;
    public float angle;
}