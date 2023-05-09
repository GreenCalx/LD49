using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum DIFFICULTIES
{
    NONE = 0,
    EASY = 1,
    MEDIUM = 2,
    HARD = 3,
    IRONMAN = 4
}

[System.Serializable]
public class TrackScoreData : EntityData
{
    public SerializableTrackScore loadedTrackScore;

    public override void OnLoad(GameObject gameObject)
    {
        TrackManager tm = Access.TrackManager();
        if (!!tm)
        {
            // 
            tm.track_score.track_time = loadedTrackScore.track_time;
            tm.track_score.selected_diff = loadedTrackScore.selected_diff;
            tm.track_score.track_name = loadedTrackScore.track_name;

            tm.track_score.track_score_data = this;
        }
    }
}

[System.Serializable]
public class SerializableTrackScore
{
    public double track_time = 0;
    public DIFFICULTIES selected_diff = DIFFICULTIES.NONE;
    public string track_name;

    public TrackScore buildTrackScore()
    {
        TrackScore retval = new TrackScore();
        retval.track_time = track_time;
        retval.selected_diff = selected_diff;
        retval.track_name = track_name;
        return retval;
    }

    public TrackScore TrackScore
    {
        get 
        { 
            return buildTrackScore(); 
        }
        set
        { 
            track_time = value.track_time;
            selected_diff = value.selected_diff;
            track_name = value.track_name;
        }
    }
    public static implicit operator TrackScore(SerializableTrackScore inst)
    {
        return inst.TrackScore;
    }
    public static implicit operator SerializableTrackScore(TrackScore iTS)
    {
        return new SerializableTrackScore { TrackScore = iTS };
    }
}

///////
[Serializable]
public class TrackScore : ISaveLoad
{
    public double track_time = 0;
    public DIFFICULTIES selected_diff = DIFFICULTIES.NONE;
    public string track_name;

    // save
    public TrackScoreData track_score_data;

    object ISaveLoad.GetData()
    {
        if (track_score_data==null)
            track_score_data = new TrackScoreData();

        track_score_data.loadedTrackScore = this;

        return track_score_data;
    }
}

///////
public class TrackManager : MonoBehaviour
{
    public double defaultPBValue = 999999999;
    
    private bool timer_is_on;

    public TrackScore track_score;

    void Awake()
    {
        track_score = new TrackScore();
    }

    // Start is called before the first frame update
    void Start()
    {
        reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer_is_on)
            track_score.track_time += Time.deltaTime;
    }

    private void reset()
    {
        track_score.track_time = 0;
    }

    public void launchTrack(string iTrackName)
    {
        reset();
        track_score.track_name = iTrackName;
        
        timer_is_on = true;
    }

    public void endTrack()
    {
        timer_is_on = false;
    }

    public string getRaceTimeKey()
    {
        return track_score.track_name + "_racetime_" + track_score.selected_diff.ToString();
    }

    public void saveRaceTime()
    {
        string str_racetime  = track_score.track_time.ToString();
    }

    public double getRaceTime()
    {

        return track_score.track_time;
    }

    public string getRacePBKey()
    {
        return track_score.track_name + "_pb_" + track_score.selected_diff.ToString();
    }

    public void saveRacePB()
    {
        SaveAndLoad.datas.Add(track_score);
        SaveAndLoad.save(getRacePBKey());
    }

    public double getRacePB()
    {
        if ( SaveAndLoad.loadTrackScore(getRacePBKey(), this) )
        {
            return track_score.track_time;
        }
        return defaultPBValue;
    }
}