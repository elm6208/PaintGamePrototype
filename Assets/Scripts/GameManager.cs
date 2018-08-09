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
    public GameObject endGamePanel;
    public Text timerText;

    public GameObject nonPlayer;

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
    public TextureDrawing textureDrawing;

    public bool AutoStart = false;

    public static GameManager instance;

    public int numNonPlayers;

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

        //spawn nonplayers
        if(isServer)
        {
            for (int i = 0; i < numNonPlayers; i++)
            {
                GameObject nonP = Instantiate(nonPlayer) as GameObject;
                NetworkServer.Spawn(nonP);
            }
        }
        
        

#if UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

    }

    public void StartGame()
    {
        if (!isServer)
            return;

        allOneColor = false;
        timeLeft = TotalTime;
        gameIsActive = true;
        gameOver = false;
        //get all players
        //allPlayers = new List<GameObject>();
        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //GameObject[] nonPlayers = GameObject.FindGameObjectsWithTag("NonPlayer");
        //allPlayers.AddRange(players);
        //allPlayers.AddRange(nonPlayers);

        //name them
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
        }

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

        //only let the server trigger end game
        if (gameIsActive && isServer)
        {
            DetermineIfEndGame();
        }

        if(gameIsActive && Time.deltaTime > eTextTimer)
        {
            RpcHideEliminatedText();
        }
        
	}

    //check if all players have been converted to one color
    public void CheckIfAllOneColor()
    {
        bool isColorDifferent = false;

        for (int i = 0; i < allPlayers.Count; i++)
        {
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

    public void DetermineIfEndGame()
    {
        if ((timeLeft <= 0) || allOneColor)
        {
            timeLeft = 0;
            timerText.text = "Time Remaining: " + (int)timeLeft;

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
            eliminatedTeamText.gameObject.SetActive(true);
            eliminatedTeamText.text = teamName + " Team Eliminated!";
            if (isServer)
            {
                eTextTimer = Time.deltaTime + 0.005f;
            }
        }
        
    }

    [ClientRpc]
    public void RpcHideEliminatedText()
    {
        eliminatedTeamText.gameObject.SetActive(false);
        
    }


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

        //display game over text, display Top Player, currently names are just numbered
        //endGameStr = "GAME OVER";
        //count top player
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
        //endGameStr = "GAME OVER: COLOR " + textureDrawing.leadingColor + " WINS, TOP PLAYER: " + topPlayer.playerName;

        endGameText.gameObject.SetActive(true);
        endGamePanel.gameObject.SetActive(true);
        endGameText.text = endGameStr;

    }

    public void LocalPlayerShoot()
    {
        Player.localPlayer.TryToShoot();
    }
}
