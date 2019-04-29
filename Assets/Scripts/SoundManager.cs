using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    public static readonly List<AudioClip> Build = new List<AudioClip>
    {
        Resources.Load<AudioClip>("Sounds/build_1"),
        Resources.Load<AudioClip>("Sounds/build_2"),
        Resources.Load<AudioClip>("Sounds/build_3"),
        Resources.Load<AudioClip>("Sounds/build_4"),
        Resources.Load<AudioClip>("Sounds/build_5"),
    };
    
    public static readonly List<AudioClip> Shout = new List<AudioClip>
    {
        Resources.Load<AudioClip>("Sounds/shout_1"),
        Resources.Load<AudioClip>("Sounds/shout_2"),
        Resources.Load<AudioClip>("Sounds/shout_3"),
        Resources.Load<AudioClip>("Sounds/shout_4"),
        Resources.Load<AudioClip>("Sounds/shout_5"),
    };

    public static readonly AudioClip BuildStarted = Resources.Load<AudioClip>("Sounds/build_started");
    public static readonly AudioClip BuildFinished = Resources.Load<AudioClip>("Sounds/build_finished");
    public static readonly AudioClip ButtonClick = Resources.Load<AudioClip>("Sounds/button_click");
    public static readonly AudioClip Capture = Resources.Load<AudioClip>("Sounds/capture");
    public static readonly AudioClip Deselect = Resources.Load<AudioClip>("Sounds/deselect");
}