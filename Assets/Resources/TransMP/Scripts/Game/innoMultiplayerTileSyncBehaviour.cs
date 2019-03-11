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

    // Tile Properties
    [HideInInspector] public int client_index = -1;

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
            // Get Tile Name
            string tile_name = transform.parent.gameObject.name;
            gameObject.name = tile_name + "_debug";

            // Update Object transforms
            check_obj = transform.parent.gameObject;
            transform.position = Vector3.zero;
            transform.localPosition = Vector3.zero;
            transform.parent = null;

            // Check for Sprite Renderer to Sync
            if (check_obj.GetComponent<SpriteRenderer>() != null) {
                tile_sr = check_obj.GetComponent<SpriteRenderer>();
            }
            else if (check_obj.transform.childCount > 0) {
                if (check_obj.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>() != null) {
                    tile_sr = check_obj.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
                    check_obj = check_obj.transform.GetChild(0).gameObject;
                }
            }

            if (tile_sr == null) {
                Debug.Log(tile_name + ": lmao there's no sprite to sync");
                Destroy(gameObject);
                return;
            }

            // Spawn Authority
            NetworkServer.SpawnWithClientAuthority(gameObject, innoMultiplayerServerBehaviour.instance.player_objects[innoMultiplayerServerBehaviour.instance.player_authority].gameObject);
            RpcUpdatePosition(check_obj.transform.position);
            RpcInitializeTileSync(tile_name);

            // Server Variables
            loaded = false;
            can_sync = true;
        }
    }

    //Update Event
    void Update()
    {
        // Server Functions
        if (isServer) {
            // Check if object is destroyed
            if (check_obj == null) {
                Destroy(gameObject);
                return;
            }

            // Update RPC calls from Server Object
            if (tileSync != null) {
                // Update client index if player
                if (tileSync.player) {
                    if (client_index == -1) {
                        if (check_obj.GetComponent<innoMultiplayerPlayer>().local_player != null) {
                            if (check_obj.GetComponent<innoMultiplayerPlayer>().client_index != -1) {
                                RpcUpdateClientIndex(check_obj.GetComponent<innoMultiplayerPlayer>().client_index);
                            }
                        }
                    }
                }

                // Make sure to load all sprites and details
                if (!loaded) {
                    loaded = true;
                    RpcUpdateSprite(Array.IndexOf(tileSync.sprites, tile_sr.sprite));
                    RpcUpdateFlipX(tile_sr.flipX);
                    reloadSpriteCooldown();
                }

                // Update Animation on Change
                if (tileSync.update_sprite) {
                    int image_index = Array.IndexOf(tileSync.sprites, tile_sr.sprite);
                    if (image_index != sprite_index) {
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
            if (isServer && check_obj != null) {
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

        // Set Parent and Name
        transform.parent = GameObject.Find("TileSync").transform;
        gameObject.name = tileSync.name + "_tilesync";

        // Variables
        if (isServer) {
            RpcUpdateSprite(Array.IndexOf(tileSync.sprites, tile_sr.sprite));
            RpcUpdateFlipX(tile_sr.flipX);
        }
        last_position = new Vector2(transform.position.x, transform.position.y);
        real_position = new Vector2(transform.position.x, transform.position.y);
    }

    // Tile Update Methods
    [ClientRpc]
    public void RpcUpdateSprite (int new_index) {
        if (tileSync != null) {
            sprite_index = new_index;
            if (new_index >= 0) {
                sr.sprite = tileSync.sprites[new_index];
            }
            else {
                sr.sprite = null;
            }
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

    public IEnumerator reloadSpriteCooldown() {
        yield return new WaitForSeconds(1f);
        loaded = false;
    }

    public IEnumerator startNetworkCooldown() {
        lerp_time_start = Time.time;
        yield return new WaitForSeconds((1 / tileSync.sync_rate));
        updateNetwork();
    }

    public void updateNetwork() {
        lerp_time_end = Time.time;

        if (check_obj == null) {
            can_sync = true;
            return;
        }

        RpcUpdateLerpPosition(new Vector2(check_obj.transform.position.x, check_obj.transform.position.y), lerp_time_end - lerp_time_start);
        can_sync = true;
    }

    // Misc Methods

    [ClientRpc]
    public void RpcUpdateClientIndex(int new_index) {
        client_index = new_index;
        if (innoMultiplayerServerBehaviour.instance.player_objects[new_index].isLocalPlayer) {
            if (!isServer) {
                Camera.main.GetComponent<innoMultiplayerCamera>().follow_obj = gameObject;
            }
            else {
                Camera.main.GetComponent<innoMultiplayerCamera>().follow_obj = check_obj;
            }
            Debug.Log(client_index);
        }
    }

}
