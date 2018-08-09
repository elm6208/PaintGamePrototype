using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerObjectReferences : MonoBehaviour {

    public static PlayerObjectReferences singleton;
    private void Awake()
    {
        singleton = this;
    }

    public Text capturedText;
    public Text playerNameText;
    
}
