using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;


public class StartConfig : MonoBehaviour {

    string ipAddress;

	// Use this for initialization
	void Start ()
    {
        ipAddress = "127.0.0.1";
        //ipAddress = GetLocalIPAddress();
        Debug.Log("IP Address: " + ipAddress);
    }


    public static string GetLocalIPAddress()
    {
        string hostName = Dns.GetHostName();
        string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
        return myIP;
    }


    // Update is called once per frame
    void Update () {
		
	}
}
