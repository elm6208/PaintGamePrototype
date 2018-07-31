using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
    [SyncVar]
    public float TotalTime = 200f;
    public float timeLeft;
    public Text endGameText;
    public Text timerText;
    
    [SyncVar]
    public bool gameOver;

    [SyncVar]
    public bool gameIsActive = false;

    public Player topPlayer;
    private List<GameObject> allPlayers;
    public TextureDrawing textureDrawing;

    public bool AutoStart = false;

    // Use this for initialization
    void Start () {
        gameIsActive = false;
        if (AutoStart)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (!isServer)
            return;

        timeLeft = TotalTime;
        gameIsActive = true;
        gameOver = false;
        //get all players
        allPlayers = new List<GameObject>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] nonPlayers = GameObject.FindGameObjectsWithTag("NonPlayer");
        allPlayers.AddRange(players);
        allPlayers.AddRange(nonPlayers);

        //name them
        for (int i = 0; i < allPlayers.Count; i++)
        {
            allPlayers[i].GetComponent<Player>().playerName = "Player " + (i + 1);
        }

        topPlayer = allPlayers[0].GetComponent<Player>();

        endGameText.gameObject.SetActive(false);

    }
	
	// Update is called once per frame
	void Update () {
        // count down and display time remaining
        if(gameIsActive && timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = "Time Remaining: " + (int)timeLeft;
        }

        //only let the server trigger end game
        if (gameIsActive && isServer)
        {
            DetermineIfEndGame();
        }
	}

    public void DetermineIfEndGame()
    {
        if (timeLeft <= 0)
        {
            RpcEndGame();
        }
    }

    // End the game
    [ClientRpc]
    private void RpcEndGame()
    {
        //display game over text
        endGameText.gameObject.SetActive(true);
        endGameText.text = "GAME OVER: COLOR " + textureDrawing.leadingColor + " WINS";
        gameOver = true;

        //destroy all projectiles
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");

        foreach(GameObject p in projectiles)
        {
            Destroy(p);
        }

        //count top player
        foreach (GameObject play in allPlayers)
        {
            Player player = play.GetComponent<Player>();
            if(player.numCaptured > topPlayer.numCaptured)
            {
                topPlayer = player;
            }
        }

        //display Top Player, currently names are just numbered
        endGameText.text = endGameText.text + ", TOP PLAYER: " + topPlayer.playerName;
        
    }
}
