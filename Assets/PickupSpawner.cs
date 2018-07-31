using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
 * I am intended to ONLY run on the server. server must handle spawning
 * 
 * todo: only spawn a pickup if that position is empty
 * to start with, just keep track of pickups themselves
 * */
public class PickupSpawner : NetworkBehaviour {
    public List<Transform> StartPositions;

    //Make sure this is the same prefab in the GameNetworkManager
    public List<GameObject> PickupPrefabs;

    public void SpawnAllPickups()
    {
        foreach(var t in StartPositions)
        {
            SpawnPickup(t);
        }
    }

    public void SpawnRandomPickup()
    {
        var which = Random.Range(0, StartPositions.Count - 1);

        //i might not have any start positions
        if (which < StartPositions.Count)
        {
            SpawnPickup(StartPositions[which]);
        }
    }

    public void SpawnPickup(Transform reference)
    {
        int pickupNum = Random.Range(0, PickupPrefabs.Count);

        var prefab = GameObject.Instantiate(PickupPrefabs[pickupNum]);
        prefab.transform.position = reference.position;
        prefab.transform.rotation = reference.rotation;

        NetworkServer.Spawn(prefab);

        prefab.transform.position = reference.position;
        prefab.transform.rotation = reference.rotation;
    }

}
