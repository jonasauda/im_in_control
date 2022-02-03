using System.Collections.Generic;
using UnityEngine;
using System;

public class PingLogger: MonoBehaviour
{

    private static DateTime timestamp;
    private static int timestampCount = 0;
    private static List<double> rtts = new List<double>();

    private static bool sendNewPing = false;
    private GameObject gameManager;
    private VinterUpstream vinterUpstream;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        vinterUpstream = gameManager.GetComponent<VinterUpstream>();
    }

    void Update()
    {
        if (sendNewPing)
        {
            sendNewPing = false;
            vinterUpstream.measureRoundTripTime = true;
        }
    }

    public static void pingReceived()
    {
        DateTime now = DateTime.Now;
        timestampCount += 1;
        if (timestamp != null && timestampCount > 20)
        {
            double millis = now.Subtract(timestamp).TotalMilliseconds;
            rtts.Add(millis);
            double sum = 0.0;
            rtts.ForEach(mil => sum += mil);
            double count = rtts.Count;
            double avg = sum / count;
            double varSum = 0.0;
            rtts.ForEach(mil => varSum += Math.Pow((mil - avg), 2));
            Debug.Log("Newest RTT: " + millis + " Count: " +  count + " Average: " + avg + " Variance: " + varSum / rtts.Count);
        }
        sendNewPing = true;
        timestamp = now;
    }
}
