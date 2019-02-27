using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class innoMultiplayerServerBehaviour : NetworkBehaviour
{
    // Singleton
    public static innoMultiplayerServerBehaviour instance { get; private set; }

    // Objects
    [Header("Multiplayer Game Scene Objects")]
    public GameObject gameManager;
    public GameObject gameCamera;
    public GameObject gameCanvas;

    // Settings
    [Header("Lobby Settings")]
    [SyncVar] public bool in_lobby;
    [SyncVar] public float lobby_timer;

    [Header("Game Data")]
    public string[] random_names;
    [HideInInspector] public SyncListString mod_list = new SyncListString();

    // Player Data
    [HideInInspector] public Queue<innoMultiplayerPlayerBehaviour> player_queue;

    [HideInInspector] public int player_authority;
    [HideInInspector] public List<innoMultiplayerPlayerBehaviour> player_objects;

    [HideInInspector] public SyncListBool players_connected = new SyncListBool();
    [HideInInspector] public SyncListString player_network_ids = new SyncListString();

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
            DontDestroyOnLoad(NetworkManager.singleton);
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
        lobby_timer = 4f;
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
                            break;
                        }
                    }
                }
            }

            // Check if Everyone is ready
            if (isServer) {
                float new_time = lobby_timer;
                if (allPlayersReady) {
                    new_time = Mathf.Clamp(new_time, 0, 3.8f);
                    new_time -= 0.01f;

                    // ModList
                    if (mod_list.Count <= 0) {
                        RpcUpdateHostModList(ContributorList.instance.activeContributorIDs);
                    }
                }
                else {
                    new_time = 4;
                }

                if (new_time <= 0) {
                    RpcUpdateLobbyTimer(4);
                    RpcUpdateInLobby(false);
                    RpcStartGame();

                    loadGameLevel();
                }
                else {
                    RpcUpdateLobbyTimer(new_time);
                }
            }

            // Update Timer
            if (lobby_timer > 3f) {
                GameObject.Find("LobbyCanvas").transform.GetChild(4).gameObject.SetActive(false);
            }
            else {
                GameObject.Find("LobbyCanvas").transform.GetChild(4).gameObject.SetActive(true);
                GameObject.Find("LobbyCanvas").transform.GetChild(4).GetComponent<Text>().text = Mathf.CeilToInt(Mathf.Clamp(lobby_timer, 0, 3)) + "";
            }
        }

        // Check if Player is Disconnected
        if (isServer) {
            for (int i = 0; i < 4; i++) {
                if (player_objects[i] == null && players_connected[i]) {
                    RpcRemovePlayer(i);
                }
            }
        }
    }

    // Load Game Methods
    [ClientRpc]
    public void RpcUpdateHostModList(string[] new_mod_list) {
        mod_list.Clear();

        for (int i = 0; i < new_mod_list.Length; i++) {
            mod_list.Add(new_mod_list[i]);
        }
    }

    [ClientRpc]
    public void RpcStartGame() {
        Destroy(GameObject.Find("LobbyCanvas"));
        Destroy(GameObject.Find("Main Camera"));

        Instantiate(gameCamera);
        Instantiate(gameCanvas);
        Instantiate(gameManager);
        GameObject tilesyncparent = new GameObject("TileSync");
        tilesyncparent.transform.position = Vector3.zero;
    }

    public void loadGameLevel() {
        
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
            //Debug.Log(players_connected[i] + ": " + player_network_ids[i]);
        }
    }

    [ClientRpc]
    public void RpcRemovePlayer(int index) {
        players_connected[index] = false;
        player_network_ids[index] = null;

        for (int i = 0; i < 4; i++) {
            //Debug.Log(players_connected[i] + ": " + player_network_ids[i]);
        }
    }

    // Server Lobby Methods
    [ClientRpc]
    public void RpcUpdateInLobby(bool is_in_lobby) {
        in_lobby = is_in_lobby;
    }

    [ClientRpc]
    public void RpcUpdateLobbyTimer(float time) {
        lobby_timer = time;
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

    // Public Get Variables
    public bool allPlayersReady {
        get {
            int players_num = 0;
            bool players_ready_now = true;
            for (int i = 0; i < 4; i++) {
                if (player_objects[i] != null) {
                    players_num++;
                    if (!player_objects[i].ready) {
                        players_ready_now = false;
                        break;
                    }
                }
            }

            return ((players_num > 1) && players_ready_now);
        }
    }
}
