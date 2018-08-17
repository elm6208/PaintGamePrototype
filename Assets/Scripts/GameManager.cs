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
    public Text timerText;
    
    public Text endGameText;
    public GameObject endGamePanel;
    
    public Text eliminatedTeamText;
    [SyncVar]
    public float eTextTimer;

    [SyncVar]
    public bool gameOver;
    [SyncVar]
    public bool gameIsActive = false;
    [SyncVar]
    private string endGameStr = "GAME OVER";
    [SyncVar]
    private bool allOneColor = false;

    public Player topPlayer;
    
    public List<GameObject> allPlayers = new List<GameObject>();
    public GameObject nonPlayer; // nonplayer prefab to spawn
    public int numNonPlayers;
    public TextureDrawing textureDrawing;

    public bool AutoStart = false;
    public static GameManager instance;
    
    // Use this for initialization
    private void Awake()
    {
        instance = this;
    }

    void Start () {

        gameIsActive = false;

        if (AutoStart)
        {
            StartGame();
        }

        //spawn nonplayers based on given number
        if(isServer)
        {
            for (int i = 0; i < numNonPlayers; i++)
            {
                GameObject nonP = Instantiate(nonPlayer) as GameObject;
                NetworkServer.Spawn(nonP);
            }
        }
        
        #if UNITY_ANDROID
        
                //if on android, turn off screen timeout during game
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        #endif

    }

    //Start the game session
    public void StartGame()
    {
        if (!isServer)
            return;

        allOneColor = false;
        timeLeft = TotalTime;
        gameIsActive = true;
        gameOver = false;
        
        // reset all players
        for (int i = 0; i < allPlayers.Count; i++)
        {
            Player currentP = allPlayers[i].GetComponent<Player>();
            currentP.playerName = "Player " + (i + 1);
            currentP.maxHealth = 3;
            currentP.health = currentP.maxHealth;
            currentP.currentSize = 0;
            currentP.SetCurrentSize(0);
            currentP.SetNumCaptured(0);
            currentP.ScramblePosition();
            currentP.SetColor(currentP.originalColor);
            currentP.SetTeamColor(currentP.originalColor);

            //if speed powerup is active, end it
            currentP.speedPowerUpActive = false;
            currentP.speed = currentP.startSpeed;

            //if trail powerup is active, end it
            currentP.trailPowerUpActive = false;
        }

        //reset topPlayer
        topPlayer = allPlayers[0].GetComponent<Player>();

        RpcResetClientGame();
        
    }
	
	// Update is called once per frame
	void Update () {

        // count down and display time remaining
        if(gameIsActive && timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = "Time Remaining: " + (int)timeLeft;
        }

        // only let the server trigger end game
        if (gameIsActive && isServer)
        {
            DetermineIfEndGame();
        }

        //if timer for eliminated team text has ended, hide the text
        if(gameIsActive && Time.deltaTime > eTextTimer)
        {
            RpcHideEliminatedText();
        }
        
	}

    //check if all players have been converted to one color (if so, end the game)
    public void CheckIfAllOneColor()
    {
        bool isColorDifferent = false;

        //search through all players
        for (int i = 0; i < allPlayers.Count; i++)
        {
            // change bool if you find a player with a different color
            if(allPlayers[i].GetComponent<Player>().currentColor != allPlayers[0].GetComponent<Player>().currentColor)
            {
                isColorDifferent = true;
            }
        }
        
        if(isColorDifferent == false)
        {
            allOneColor = true;
        }
    }

    // Determine if the game is over
    public void DetermineIfEndGame()
    {
        //if time is 0 or all players are one color
        if ((timeLeft <= 0) || allOneColor)
        {
            timeLeft = 0;
            timerText.text = "Time Remaining: " + (int)timeLeft;
            //display winning team and top player
            endGameStr = "GAME OVER: " + textureDrawing.colorNames[textureDrawing.leadingColor] + " WINS, TOP PLAYER: " + topPlayer.playerName;
            RpcEndGame();
        }
    }

    // When a team is eliminated, notify players
    [ClientRpc]
    public void RpcDisplayEliminatedText(string teamName)
    {
        if(gameIsActive)
        {
            //turn on eliminated text and set timer for text display
            eliminatedTeamText.gameObject.SetActive(true);
            eliminatedTeamText.text = teamName + " Team Eliminated!";
            if (isServer)
            {
                eTextTimer = Time.deltaTime + 0.005f;
            }
        }
        
    }

    // Hide the "eliminated team" text
    [ClientRpc]
    public void RpcHideEliminatedText()
    {
        eliminatedTeamText.gameObject.SetActive(false);
        
    }

    // Hide text on client games when game is reset
    [ClientRpc]
    private void RpcResetClientGame()
    {
        endGameText.gameObject.SetActive(false);
        endGamePanel.gameObject.SetActive(false);
        eliminatedTeamText.gameObject.SetActive(false);
    }

    // End the game
    [ClientRpc]
    private void RpcEndGame()
    {
        gameOver = true;
        
        //destroy all projectiles on server
        if(isServer)
        {
            GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");

            foreach (GameObject p in projectiles)
            {
                NetworkServer.Destroy(p);
            }
        }
        
        // set top player
        foreach (GameObject play in allPlayers)
        {
            Player player = play.GetComponent<Player>();
            if(play == null && player == null)
            {
                Debug.LogError("No player script on player!");
                continue;
            }
            if(topPlayer == null)
            {
                topPlayer = player;
            }

            if (player.numCaptured > topPlayer.numCaptured)
            {
                topPlayer = player;
            }
        }

        // activate game over text
        endGameText.gameObject.SetActive(true);
        endGamePanel.gameObject.SetActive(true);
        endGameText.text = endGameStr;

    }

    // Tell player to try to shoot
    public void LocalPlayerShoot()
    {
        Player.localPlayer.TryToShoot();
    }
}
