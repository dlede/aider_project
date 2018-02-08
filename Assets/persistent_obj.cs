using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class persistent_obj : MonoBehaviour {

    public string user_email;
    public string user_password;
    public string user_sessiontix;
    public string user_lastlogintime;
    public string user_playfabid;
    public float latitude;
    public float longitude;
    private double update_ticks;
    private int rounded_ticks;

    private void Start()
    {
        update_ticks = 0.0;
        update_ticks = 0;
        StartCoroutine(StartLocationService());
    }

    private void Update()
    {
        update_ticks += Time.deltaTime;
        rounded_ticks = Convert.ToInt32(Math.Round(update_ticks, MidpointRounding.AwayFromZero));
        if (rounded_ticks >= 5) // 5 seconds per update
        {
            StartCoroutine(StartLocationService());
        }
    }

    private IEnumerator StartLocationService()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start service before querying location
        Input.location.Start(1,0.1f); //10 metres thn update

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }

        this.latitude = Input.location.lastData.latitude;
        this.longitude = Input.location.lastData.longitude;

        // Stop service if there is no need to query location updates continuously
        //Input.location.Stop();
    }

    public void set_lat(float latitude)
    {
        this.latitude = latitude;
    }

    public float get_lat()
    {
        return this.latitude;
    }

    public void set_long(float longitude)
    {
        this.longitude = longitude;
    }

    public float get_long()
    {
        return this.longitude;
    }

    public void set_email(string email)
    {
        this.user_email = email;
    }

    public string get_email()
    {
        return this.user_email;
    }

    public void set_password(string password)
    {
        this.user_password = password;
    }

    public string get_password()
    {
        return this.user_password;
    }

    public void set_session(string sessiontix)
    {
        this.user_sessiontix = sessiontix;
    }

    public string get_session()
    {
        return this.user_sessiontix;
    }

    public void set_llr(string llr)
    {
        this.user_lastlogintime = llr;
    }

    public string get_llr()
    {
        return this.user_lastlogintime;
    }

    public void set_pfi(string pfi)
    {
        this.user_playfabid = pfi;
    }

    public string get_pfi()
    {
        return this.user_playfabid;
    }
}
