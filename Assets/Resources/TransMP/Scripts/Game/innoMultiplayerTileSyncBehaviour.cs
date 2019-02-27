using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class innoMultiplayerTileSyncBehaviour : NetworkBehaviour
{

    // Components
    public float update_delay = 0.2f;

    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public SpriteRenderer tile_sr;
    [HideInInspector] public GameObject check_obj;
    [HideInInspector] public innoTileSync tileSync; 

    // Variables
    [HideInInspector]
    public bool load;
    [HideInInspector]
    public float timer;
    [HideInInspector]
    [SyncVar] public int image_index;
    [HideInInspector]
    [SyncVar] public string index_name;

    //Initialize Event
    void Start()
    {
        // Check if this is the Server TileSync
        if (innoMultiplayerServerBehaviour.instance.isServer) {
            NetworkServer.SpawnWithClientAuthority(gameObject, innoMultiplayerServerBehaviour.instance.player_objects[innoMultiplayerServerBehaviour.instance.player_authority].gameObject);
            tile_sr = check_obj.GetComponent<SpriteRenderer>();
            RpcUpdateIndexName(check_obj.name);
            RpcInitializeTileSync();
        }

        transform.parent = GameObject.Find("TileSync").transform;
    }

    //Update Event
    void Update()
    {
        if (load) {
            if (isServer) {
                if (check_obj == null) {
                    //RpcUpdateDestroy();
                    //return;
                }

                timer -= Time.deltaTime;
                if (timer <= 0) {
                    int new_index = -1;
                    for (int i = 0; i < tileSync.sprites.Length; i++) {
                        if (tileSync.sprites[i] == tile_sr.sprite) {
                            new_index = i;
                            break;
                        }
                    }

                    RpcUpdateSprite(new_index);
                    Debug.Log(new_index);
                    timer = update_delay;
                }

                //RpcUpdateTransform(check_obj.transform.position, check_obj.transform.eulerAngles);
            }
            else {
                if (image_index >= 0) {
                    sr.sprite = tileSync.sprites[image_index];
                }
                else {
                    sr.sprite = null;
                }
            }
        }
    }

    // Tile Initialize Method
    [ClientRpc]
    public void RpcUpdateIndexName(string new_name) {
        index_name = new_name;
    }

    [ClientRpc]
    public void RpcInitializeTileSync() {
        tileSyncInit();
    }

    public void tileSyncInit() {
        // Tile Sync
        if (GameManager.instance.multiplayer_data.ContainsKey(index_name)) {
            // Set tile sync
            Debug.Log(gameObject.name);
            tileSync = (innoTileSync) GameManager.instance.multiplayer_data[index_name];
            load = true;
        }
        else {
            // Destroy if no tile sync exists
            Destroy(gameObject);
            return;
        }

        // Components
        sr = GetComponent<SpriteRenderer>();
        if (isServer) {
            sr.enabled = false;
        }

        // Variables
        image_index = -1;
        timer = 0;
    }

    // Tile Update Methods
    [ClientRpc]
    public void RpcUpdateSprite (int sprite_index) {
        image_index = sprite_index;
    }

    [ClientRpc]
    public void RpcUpdateTransform (Vector3 position, Vector3 rotation) {
        transform.position = position;
        transform.eulerAngles = rotation;
    }

    [ClientRpc]
    public void RpcUpdateDestroy () {
        Destroy(gameObject);
    }

}
