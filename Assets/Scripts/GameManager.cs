using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
    [SyncVar]
    public float TotalTime = 200f;
    [SyncVar]
    public float timeLeft;
    public Text endGameText;
    public Text timerText;
    
    [SyncVar]
    public bool gameOver;

    [SyncVar]
    public bool gameIsActive = false;

    [SyncVar]
    private string endGameStr = "GAME OVER";

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
            Player currentP = allPlayers[i].GetComponent<Player>();
            currentP.playerName = "Player " + (i + 1);
            currentP.maxHealth = 3;
            currentP.health = currentP.maxHealth;
            currentP.currentSize = 1;
            currentP.numCaptured = 0;
            currentP.ScramblePosition();
            currentP.SetColor(currentP.originalColor);
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

        //display game over text, display Top Player, currently names are just numbered
        endGameStr = "GAME OVER: COLOR " + textureDrawing.leadingColor + " WINS, TOP PLAYER: " + topPlayer.playerName;
        
        endGameText.gameObject.SetActive(true);
        endGameText.text = endGameStr;
    }

    public void LocalPlayerShoot()
    {
        Player.localPlayer.TryToShoot();
    }
}
