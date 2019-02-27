using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoMultiplayerSpawnTileSync : MonoBehaviour
{
    public GameObject network_sync;

    // Initialization
    void Start()
    {
        GameObject new_sync = Instantiate(network_sync, transform.parent.position, transform.parent.rotation);
        new_sync.GetComponent<innoMultiplayerTileSyncBehaviour>().check_obj = transform.parent.gameObject;
        Destroy(gameObject);
    }

}
