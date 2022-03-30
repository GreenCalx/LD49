using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class Clip {
        public AudioClip Sound;
        public float LoopBeginningInSec;
        public float LoopBeginningInBars;
        public bool FirstPlay;
        public float BPM;
        public float TimeSignature;
        public string Name;
    }

    public Clip[] Clips;
    public Clip CurrentClip;
    public Clip FromClip;
    Clip GetClip(string name) {
        return (System.Array.Find(Clips, C => (C.Name == name)));
    }
    public AudioSource Current;
    public AudioSource From;

    private bool IsTransitioning = false;
    public float TransitionSpeedInSec;
    public float TransitionVolMin;
    private float CurrentTimeTransitioningInSec = 0;
    private bool QueuedTransition = false;
    private string QueuedTransitionName = "";
    // Start is called before the first frame update
    void Start()
    {
        foreach(var C in Clips)
        {
            C.LoopBeginningInSec = ( 60f/C.BPM) * C.TimeSignature * C.LoopBeginningInBars;
        }


        Current.clip = (Clips[0].Sound);
        CurrentClip = Clips[0];
        Current.Play(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsTransitioning) {
            CurrentTimeTransitioningInSec += Time.deltaTime;
            IsTransitioning = CurrentTimeTransitioningInSec < TransitionSpeedInSec;
            // lower BPM to match next song
            if (CurrentClip.BPM != FromClip.BPM) {
                // change pitch
                var DesiredPitch = CurrentClip.BPM / FromClip.BPM;
                From.pitch = 1 + ((DesiredPitch-1) * (CurrentTimeTransitioningInSec / TransitionSpeedInSec));
            }
            // lower volume to crossfade
            Current.volume = 1 - From.volume;
            From.volume = IsTransitioning ? 1 - (CurrentTimeTransitioningInSec / TransitionSpeedInSec) : 1;
            if(!IsTransitioning) {
                From.Stop();
                From.pitch = 1;
            }
            // wait for bar beginning before play
            // are we one a beat roughly?
            var Threshold = 0.1f;
            // NOTE toffa : here we dont take into account the time signature to be able to play the new song faster (not having to wait a whole bar)
            var BeginningOfBar = (From.time % ((60f/FromClip.BPM))) < Threshold;
            if (BeginningOfBar && !Current.isPlaying) {
                Current.Play();
            }
        }

        if (!Current.isPlaying && !From.isPlaying) {
            // loop by hand
            Current.time = CurrentClip.LoopBeginningInSec;
            Current.Play();
        }

        if (!IsTransitioning && QueuedTransition) {
            SwitchClip(QueuedTransitionName);
            QueuedTransition = false;
        }
    }

    public void SwitchClip (string name) {
        if (name == CurrentClip.Name) return;

        if(!IsTransitioning) {
            IsTransitioning = true;

            FromClip = CurrentClip;
            CurrentClip = GetClip(name);

            var Temp = Current ;
            Current = From;
            From = Temp;

            Current.clip = CurrentClip.Sound;
            Current.time = CurrentClip.LoopBeginningInSec;
            CurrentTimeTransitioningInSec = 0;
        } else {
            QueuedTransition = true;
            QueuedTransitionName = name;
        }
    }
}
