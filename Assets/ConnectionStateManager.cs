using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ConnectionStateManager : NetworkBehaviour {
    public static ConnectionStateManager singleton;
    public Text ConnectedPlayers;

    [SyncVar]
    public int NumConnected = 0;
    private void Awake()
    {
        singleton = this;
    }

    [ClientRpc]
    public void RpcUpdateConnectedPlayersState()
    {
        ConnectedPlayers.text = "Connected Players: " + NumConnected;
    }

  
}
