using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PaintGameServer : MonoBehaviour {
    public TextureDrawing Drawing;


    private void OnEnable()
    {
        RegisterMessages();
    }

    private void OnDisable()
    {
        UnregisterMessages();
    }

    void RegisterMessages()
    {

    }

    void UnregisterMessages()
    {
    }

    void DidAddPlayer(NetworkMessage message)
    {
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
