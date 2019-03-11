using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class innoMultiplayerGameManager : GameManager
{
    // Variables
    public GameObject multiplayer_player;
    public GameObject multiplayer_nabori;
    public GameObject multiplayer_behaviour_object;

    [HideInInspector]
    public List<string> active_mods;

    // On Instantiate
    public override void Awake()
    {
        _instance = this;
        instantiateMultiplayerData();
    }

    public override void Start () {
        if (innoMultiplayerServerBehaviour.instance.isServer) {
            Destroy(transform.GetChild(0).gameObject);
            ContributorList.instance.deactivateContributorID("TransMP");
            base.Awake();
            ContributorList.instance.activateContributorID("TransMP");

            // Debug Instantiate Players
            Vector2 temp_position = Player.instance.transform.position;
            Destroy(Player.instance.gameObject);
            for (int i = 0; i < 4; i++) {
                if (innoMultiplayerServerBehaviour.instance.player_objects[i] != null) {
                    GameObject new_player = Instantiate(multiplayer_player);
                    new_player.transform.position = new Vector3(temp_position.x, temp_position.y, 0f);
                    new_player.GetComponent<innoMultiplayerPlayer>().client_index = i;
                    new_player.GetComponent<innoMultiplayerPlayer>().local_player = innoMultiplayerServerBehaviour.instance.player_objects[i];
                    GameObject new_nabori = Instantiate(multiplayer_nabori);
                    new_nabori.transform.position = new_player.transform.position;
                    new_nabori.GetComponent<innoMultiplayerNabori>().sprite_index = innoMultiplayerServerBehaviour.instance.player_objects[i].flag;
                    new_nabori.GetComponent<innoMultiplayerNabori>().player_follow = new_player;
                }
            }
        }
        else {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    // Update Event
    public override void Update () {

    }

    // Multiplayer Data Methods
    public void instantiateMultiplayerData() {
        multiplayer = true;
        multiplayer_behaviour = new List<GameObject>();
        multiplayer_behaviour.Add(multiplayer_behaviour_object);
        multiplayer_data = new Dictionary<string, ScriptableObject>();

        // Get List of Enabled mods
        string[] mod_list = innoMultiplayerServerBehaviour.instance.mod_list.ToArray();
        
		for (int i = 0; i < mod_list.Length; i++) {
            // Check if mod has a multiplayer folder
            try {
                // Get Mod from Resources Folder
                innoTileSync[] tile_syncs = Resources.LoadAll(mod_list[i] + "/Multiplayer", typeof(innoTileSync)).Cast<innoTileSync>().ToArray();

                // Check if Mod Exists
                if (tile_syncs.Length > 0) {
                    // Add mod!
                    //Debug.Log(mod_list[i] + " was added!");
                    for (int q = 0; q < tile_syncs.Length; q++) {
                        multiplayer_data.Add(tile_syncs[q].tile_name, tile_syncs[q]);
                        //Debug.Log(mod_list[i] + ": " + tile_syncs[q].tile_name);
                    }

                    active_mods.Add(mod_list[i]);
                }
            }
            catch (Exception e) {
                // Couldn't load mod :C
                Debug.Log(mod_list[i] + "couldn't be loaded :C");
            }
        }

    }

}