using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vital : MonoBehaviour {

    public int age = 25; // should be a range/type though, toddler, kids, teenager, young adult, adult, elderly
    public float temperature = 36.5f; // average temperature in degree Celcius
    public int breathing = 16; //average breathing rate of human per min (12 to 20 normal, below 12 and above 25 is abnormal)
    public int pulse = 75; //average pulse rate of human per min (60 to 100) but depends on individual, well trained athele can have pulse rate of 40

    float timeLeft = 15.0f;
    public float health = 50f;

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            //GameOver();
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    public void counter_Death()
    {
        if (temperature > 37.8 || temperature < 36) // temp more thn 37.8, less thn 36
        {
            Die();
            // reference: https://en.wikipedia.org/wiki/Human_body_temperature
            // TODO: if threshold go even further, timer reduce faster e.g.
            // TODO: if threshold go even furthest, death immediately e.g.
            // TODO: counter start
        }
        if (breathing > 25 || breathing < 12) // breath rate more thn 25, less thn 12
        {
            Die();
            // reference: https://en.wikipedia.org/wiki/Respiratory_rate
            // TODO: if threshold go even further, timer reduce faster e.g.
            // TODO: counter start
        }
        if (pulse > 110 || pulse < 12) // breath rate more thn 25, less thn 12
        {
            Die();
            // 
            // TODO: if threshold go even further, timer reduce faster e.g.
            // TODO: counter start
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
