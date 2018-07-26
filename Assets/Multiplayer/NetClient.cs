using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

[Serializable]
public class NetworkStringEvent: UnityEvent<string, string>
    {}

public class NetClient : NetworkDiscovery
{
    public NetworkStringEvent OnFoundServer;

    void Start()
    {
        startClient();
    }

    public void startClient()
    {
        Initialize();
        StartAsClient();
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        var items = data.Split(':');

        if (useNetworkManager)
        {
            Debug.Log("received message from:" + fromAddress + " message:" + data);


            if (items.Length == 3 && items[0] == "NetworkManager")
            {
                if (NetworkManager.singleton != null && NetworkManager.singleton.client == null)
                {
                    NetworkManager.singleton.networkAddress = fromAddress; //items[1];
                    NetworkManager.singleton.networkPort = Convert.ToInt32(items[2]);
                    NetworkManager.singleton.StartClient();
                }
            }
        }

        if (OnFoundServer != null)
        {
            OnFoundServer.Invoke(fromAddress, data);
        }
    }
}