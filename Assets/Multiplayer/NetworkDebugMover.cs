using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkDebugMover : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float rate = 1f;

        if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
        {
            rate = 5f;
        }

		if(Input.GetKey( KeyCode.UpArrow) )
        {
            this.transform.Translate(0, Time.deltaTime * rate, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            this.transform.Translate(0, - Time.deltaTime * rate, 0);

        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            this.transform.Translate(- Time.deltaTime * rate, 0, 0);

        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            this.transform.Translate(Time.deltaTime * rate, 0, 0);

        }

    }
}
