using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class innoMultiplayerServerBehaviour : NetworkBehaviour
{
    // Singleton
    public static innoMultiplayerServerBehaviour instance { get; private set; }

    // Settings
    [SyncVar] public bool in_lobby;
    public string[] random_names;

    // Player Data
    public Queue<innoMultiplayerPlayerBehaviour> player_queue;

    public int player_authority;
    public List<innoMultiplayerPlayerBehaviour> player_objects;

    public SyncListBool players_connected = new SyncListBool();
    public SyncListString player_network_ids = new SyncListString();

    // Instantiate
    void Awake()
    {
        // Singleton
        if (GameObject.Find("ServerManagement") != null) {
            Destroy(gameObject);
            return;
        }
        else {
            DontDestroyOnLoad(gameObject);
            gameObject.name = "ServerManagement";
            instance = this;
        }

        player_queue = new Queue<innoMultiplayerPlayerBehaviour>();
        player_objects = new List<innoMultiplayerPlayerBehaviour>();
        for (int i = 0; i < 4; i++) {
            player_objects.Add(null);
        }
    }    

    void Start() {
        // Player Data
        for (int i = 0; i < 4; i++) {
            players_connected.Add(false);
            player_network_ids.Add(null);
        }

        // Lobby
        in_lobby = true;
        player_authority = -1;
        
        GameObject.Find("LobbyActive").GetComponent<innoLobbyManagerBehaviour>().sm = this;
    }

    // Update Event
    void Update()
    {
        // Update Players in Lobby
        if (in_lobby) {
            // Add unadded multiplayer player behaviours and index them
            GameObject[] check_playertag_objs = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject check_obj in check_playertag_objs) {
                if (check_obj.GetComponent<innoMultiplayerPlayerBehaviour>() != null) {
                    if (!player_objects.Contains(check_obj.GetComponent<innoMultiplayerPlayerBehaviour>())) {
                        player_queue.Enqueue(check_obj.GetComponent<innoMultiplayerPlayerBehaviour>());
                    }
                }
            }

            if (player_queue.Count > 0) {
                checkPlayerQueue();
            }

            // Check Authority
            if (player_authority == -1) {
                for (int i = 0; i < 4; i++) {
                    if (player_objects[i] != null) {
                        if (player_objects[i].isLocalPlayer) {
                            player_authority = i;
                        }
                    }
                }
            }
        }
    }

    // Player Networking Methods

    public void checkPlayerQueue() {
        // Iterate through entire player queue
        while (player_queue.Count > 0) {
            innoMultiplayerPlayerBehaviour check_player = player_queue.Dequeue();
            string check_player_netid = check_player.netId.ToString();

            // Check if Player is Already Connected
            for (int i = 0; i < 4; i++) {
                if (players_connected[i]) {
                    if (player_network_ids[i] == check_player_netid) {
                        // Player is already connected
                        player_objects[i] = check_player;
                        return;
                    }
                }
            }

            // Player is not Connected so find a free slot to place them
            for (int i = 0; i < 4; i++) {
                if (!players_connected[i]) {
                    player_objects[i] = check_player;
                    // Checks if Server Authority
                    if (isServer) {
                        RpcAddNewPlayer(i, check_player_netid, "Player " + (i + 1));
                    }
                    return;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcAddNewPlayer(int index, string net_id, string player_name) {
        players_connected[index] = true;
        player_network_ids[index] = net_id;

        for (int i = 0; i < 4; i++) {
            Debug.Log(players_connected[i] + ": " + player_network_ids[i]);
        }
    }

    // Player Data Methods
    public string getName() {
        while (true) {
            string temp_name = random_names[Random.Range(0, random_names.Length -1)];

            bool can_use_name = true;
            for (int i = 0; i < 4; i++) {
                if (player_objects[i] != null) {
                    if (player_objects[i].name_tag == temp_name) {
                        can_use_name = false;
                        break;
                    }
                }
            }

            if (can_use_name) {
                return temp_name;
            }
        }
    }

    public int getFlag(int index) {
        int k = 0;
        while (k <= 16) {
            index++;

            if (index >= 16) {
                index = 0;
            }

            bool flag_unused = true;
            for (int i = 0; i < 4; i++) {
                if (players_connected[i]) {
                    if (player_objects[i].flag == index) {
                        flag_unused = false;
                        break;
                    }
                }
            }

            if (flag_unused) {
                return index;
            }

            k++;
        }

        return -1;
    }
}
