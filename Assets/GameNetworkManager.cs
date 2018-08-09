using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Smooth;

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

            var ns = player.GetComponent<SmoothSync>();
            ns.teleport();
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
        
        ConnectionStateManager.singleton.RpcUpdateConnectedPlayersState(GameNetworkManager.game.numPlayers);
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
    /*
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        RefreshPlayersList();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        RefreshPlayersList();
    }
*/

}
