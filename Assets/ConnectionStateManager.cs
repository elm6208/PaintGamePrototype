using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ConnectionStateManager : NetworkBehaviour {
    public static ConnectionStateManager singleton;
    public Text ConnectedPlayers;

    private void Awake()
    {
        singleton = this;
    }

    [ClientRpc]
    public void RpcUpdateConnectedPlayersState()
    {
        int count = GameNetworkManager.game.numPlayers;

        ConnectedPlayers.text = "Connected Players: " + count;
    }

  
}
