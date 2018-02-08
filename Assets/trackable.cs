using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class trackable : MonoBehaviour, ITrackableEventHandler
{
    private TrackableBehaviour mTrackableBehaviour;
    public bool bodySpawned = false;
    // Use this for initialization
    void Start () {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status newStatus, TrackableBehaviour.Status previousStatus)
    {
        if (previousStatus == TrackableBehaviour.Status.DETECTED ||
            previousStatus == TrackableBehaviour.Status.TRACKED)// || newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            bodySpawned = true;
        }
        else
        {
           bodySpawned = false;
        }
    }

    public bool get_spawned()
    {
        return bodySpawned;
    }
}
