using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class innoMultiplayerTileSyncBehaviour : NetworkBehaviour
{

    // Components
    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public SpriteRenderer tile_sr;
    [HideInInspector] public GameObject check_obj;
    [HideInInspector] public innoTileSync tileSync; 

    // Sprite Variables
    [HideInInspector] public bool loaded;

    [HideInInspector] public int sprite_index;
    [HideInInspector] public bool flip_x;

    // Position Variables
    [HideInInspector] public float lerp_time_start;
    [HideInInspector] public float lerp_time_end;

    [HideInInspector] public bool can_sync;
    [HideInInspector] public Vector2 last_position;
    [HideInInspector] public Vector2 real_position;
    [HideInInspector] public float lerp_start;
    [HideInInspector] public float lerp_time;

    //Initialize Event
    void Start()
    {
        // Check if this is the Server TileSync
        if (innoMultiplayerServerBehaviour.instance.isServer) {
            // Update Object
            check_obj = transform.parent.gameObject;
            tile_sr = check_obj.GetComponent<SpriteRenderer>();
            transform.position = Vector3.zero;
            transform.localPosition = Vector3.zero;

            // Spawn Authority
            NetworkServer.SpawnWithClientAuthority(gameObject, innoMultiplayerServerBehaviour.instance.player_objects[innoMultiplayerServerBehaviour.instance.player_authority].gameObject);
            RpcUpdatePosition(check_obj.transform.position);
            RpcInitializeTileSync(check_obj.name);

            // Server Variables
            can_sync = true;
        }

        // Set Parent and Name
        transform.parent = GameObject.Find("TileSync").transform;
        gameObject.name = tileSync.name + "_tilesync";
    }

    //Update Event
    void Update()
    {
        // Server Functions
        if (isServer) {
            // Make sure to load all sprites and details
            if (!loaded) {
                loaded = true;
                RpcUpdateSprite(Array.IndexOf(tileSync.sprites, tile_sr.sprite));
                RpcUpdateFlipX(tile_sr.flipX);
            }

            // Update RPC calls from Server Object
            if (tileSync != null) {
                // Check if object is destroyed
                if (check_obj == null) {
                    Destroy(gameObject);
                    return;
                }

                // Update Animation on Change
                if (tileSync.update_sprite) {
                    if (Array.IndexOf(tileSync.sprites, tile_sr.sprite) != sprite_index) {
                        RpcUpdateSprite(Array.IndexOf(tileSync.sprites, tile_sr.sprite));
                    }

                    if (tileSync.update_flip) {
                        if (tile_sr.flipX != flip_x) {
                            RpcUpdateFlipX(tile_sr.flipX);
                        }
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // Check if has Tile Sync
        if (tileSync != null) {
            // Run Server Code
            if (isServer) {
                // Update Position and Roatation
                if (tileSync.update_transform) {
                    // Position
                    if (tileSync.position) {
                        if (can_sync) {
                            if (new Vector2(transform.position.x, transform.position.y) != new Vector2(check_obj.transform.position.x, check_obj.transform.position.y)) {
                                can_sync = false;
                                StartCoroutine(startNetworkCooldown());
                            }
                        }
                    }

                    // Rotation
                    if (tileSync.rotation) {
                        if (transform.eulerAngles.z != check_obj.transform.eulerAngles.z) {
                            RpcUpdateRotation(check_obj.transform.eulerAngles.z);
                        }
                    }
                }
            }
            else if (!isServer) {
                if (tileSync.position) {
                    float lerp_percent = (Time.time - lerp_start) / lerp_time;

                    Vector2 temp_pos = Vector2.Lerp(last_position, real_position, lerp_percent);
                    transform.position = new Vector3(temp_pos.x, temp_pos.y, transform.position.z);
                }
            }
        }
    }

    // Tile Initialize Method
    [ClientRpc]
    public void RpcInitializeTileSync(string tile_index) {
        tileSyncInit(tile_index);
    }

    public void tileSyncInit(string tile_index) {
        // Tile Sync
        if (GameManager.instance.multiplayer_data.ContainsKey(tile_index)) {
            // Retrieve Tile Sync Data
            tileSync = (innoTileSync) GameManager.instance.multiplayer_data[tile_index];
        }
        else {
            Destroy(gameObject);
            return;
        }

        // Components
        sr = GetComponent<SpriteRenderer>();
        if (isServer) {
            sr.enabled = false;
        }

        // Variables
        if (isServer) {
            RpcUpdateSprite(Array.IndexOf(tileSync.sprites, tile_sr.sprite));
            RpcUpdateFlipX(tile_sr.flipX);
        }
        last_position = new Vector2(transform.position.x, transform.position.y);
        real_position = new Vector2(transform.position.x, transform.position.y);

        // Debug
        if (tileSync.player) {
            Camera.main.GetComponent<innoMultiplayerCamera>().follow_obj = gameObject;
        }
    }

    // Tile Update Methods
    [ClientRpc]
    public void RpcUpdateSprite (int new_index) {
        sprite_index = new_index;
        if (new_index >= 0) {
            sr.sprite = tileSync.sprites[new_index];
        }
        else {
            sr.sprite = null;
        }
    }

    [ClientRpc]
    public void RpcUpdateFlipX (bool new_flip_x) {
        sr.flipX = new_flip_x;
        flip_x = new_flip_x;
    }

    [ClientRpc]
    public void RpcUpdatePosition (Vector2 position) {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    [ClientRpc]
    public void RpcUpdateLerpPosition (Vector2 position, float new_lerp_time) {
        last_position = real_position;
        real_position = position;
        lerp_time = new_lerp_time;
        lerp_start = Time.time;
    }

    [ClientRpc]
    public void RpcUpdateRotation (float rotation) {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, rotation);
    }

    // Movement Sync Methods

    public IEnumerator startNetworkCooldown() {
        lerp_time_start = Time.time;
        yield return new WaitForSeconds((1 / tileSync.sync_rate));
        updateNetwork();
    }

    public void updateNetwork() {
        lerp_time_end = Time.time;
        RpcUpdateLerpPosition(new Vector2(check_obj.transform.position.x, check_obj.transform.position.y), lerp_time_end - lerp_time_start);
        can_sync = true;
    }

}
