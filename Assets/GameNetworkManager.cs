using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkManager {

    public List<Player> players;

    public static GameNetworkManager game
    {
        get
        {
            return NetworkManager.singleton as GameNetworkManager;
        }
    }

    public void ScramblePlayers()
    {
        foreach (var player in players)
        {
            player.ScramblePosition();
        }
    }

    public void RefreshPlayersList()
    {
        List<Player> PlayersList = new List<Player>();

        List<PlayerController> pc = this.client.connection.playerControllers;

        for (int i = 0; i < pc.Count; i++)
        {
            var go = pc[i].gameObject;
            var player = go.GetComponent<Player>();
            if (pc[i].IsValid && player != null)
            {
                PlayersList.Add(player);
            }
        }

        players = PlayersList;
        
        ConnectionStateManager.singleton.RpcUpdateConnectedPlayersState();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);
        ConnectionStateManager.singleton.NumConnected += 1;
        RefreshPlayersList();

    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        base.OnServerRemovePlayer(conn, player);
        ConnectionStateManager.singleton.NumConnected -= 1;
        RefreshPlayersList();

    }


}
