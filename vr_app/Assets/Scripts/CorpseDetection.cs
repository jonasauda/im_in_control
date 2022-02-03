using UnityEngine;
using System;

public class CorpseDetection : MonoBehaviour
{
    private DateTime timestamp;

    public void Init()
    {
        this.timestamp = DateTime.Now;
    }

    public void UpdateTime()
    {
        timestamp = DateTime.Now;
    }

    public bool IsCorpse()
    {
        return DateTime.Now.Subtract(timestamp).TotalSeconds > 1.0;
    }

}