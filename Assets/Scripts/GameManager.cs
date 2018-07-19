﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public float timeLeft;
    public Text endGameText;
    public Text timerText;
    public TextureDrawing textureDrawing;
    public bool gameOver;
    public Player topPlayer;
    private List<GameObject> allPlayers;

    // Use this for initialization
    void Start () {
        gameOver = false;
        //get all players
        allPlayers = new List<GameObject>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] nonPlayers = GameObject.FindGameObjectsWithTag("NonPlayer");
        allPlayers.AddRange(players);
        allPlayers.AddRange(nonPlayers);

        //name them
        for(int i = 0; i < allPlayers.Count; i++)
        {
            allPlayers[i].GetComponent<Player>().playerName = "Player " + (i + 1);
        }
    }
	
	// Update is called once per frame
	void Update () {

        // count down and display time remaining
        if(timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = "Time Remaining: " + (int)timeLeft;
        }
        if(timeLeft <= 0)
        {
            EndGame();
        }
	}

    // End the game
    private void EndGame()
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
