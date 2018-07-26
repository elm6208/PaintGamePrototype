using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RunAsHost : MonoBehaviour {
	// Use this for initialization
	void Start () {
            NetworkManager.singleton.StartHost();
    }
	
}
