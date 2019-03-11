using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class innoMultiplayerPlayerBehaviour : NetworkBehaviour
{

    // Components
    private innoMultiplayerServerBehaviour sm;

    // Objects
    public GameObject server_manager;

    // Player Data
    [SyncVar] public string name_tag;
    [SyncVar] public int flag;
    [SyncVar] public int ping;
    [SyncVar] public bool ready;

    [SyncVar] public Vector2 mouse_position;
    [SyncVar] public bool up;
    [SyncVar] public bool down;
    [SyncVar] public bool left;
    [SyncVar] public bool right;
    [SyncVar] public bool pick_up;
    [SyncVar] public bool click;

    // Instantiate
    void Awake()
    {
        // Player Init
        gameObject.tag = "Player";
        gameObject.name = "inno_multiplayer_player";
        DontDestroyOnLoad(gameObject);
    }

    // Server Init
    void Start()
    {
        if (isServer) {
            if (hasAuthority) {
                NetworkServer.SpawnWithClientAuthority(Instantiate(server_manager), gameObject);

                // Variables
                mouse_position = Vector2.zero;
            }
        }

        sm = innoMultiplayerServerBehaviour.instance;
        if (isLocalPlayer) {
            if (!isServer) {
                CmdInitializePlayer();
            }
            else {
                RpcUpdateInitializePlayer();
            }
        }
    }

    // Update
    void Update()
    {
        // Local Player Behaviour
        if (isLocalPlayer) {
            // Update Ping
            int game_ping = NetworkManager.singleton.client.GetRTT();
            if (!isServer) {
                CmdUpdatePing(game_ping);
            }
            else {
                RpcUpdatePing(game_ping);
            }

            // Controls
            Vector2 temp_mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            bool temp_up = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
		    bool temp_right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
		    bool temp_down = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
		    bool temp_left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);

            bool temp_space = Input.GetKeyDown(KeyCode.Space);
            bool temp_click = Input.GetMouseButtonDown(0);

            updateLocalControls(temp_up, temp_down, temp_left, temp_right, temp_space, temp_click, temp_mouse_position);
        }
    }

    // Player Data Methods
    [Command]
    public void CmdInitializePlayer() {
        RpcUpdateInitializePlayer();
    }

    [ClientRpc]
    public void RpcUpdateInitializePlayer() {
        name_tag = sm.getName();
        flag = sm.getFlag(-1);
        ping = 0;
        ready = false;
    }

    public void updateLocalControls(bool new_up, bool new_down, bool new_left, bool new_right, bool new_space, bool new_click, Vector2 new_mouse_position) {
        if (!isServer) {
            CmdUpdateControls(new_up, new_down, new_left, new_right, new_space, new_click, new_mouse_position);
        }
        else {
            RpcUpdateControls(new_up, new_down, new_left, new_right, new_space, new_click, new_mouse_position);
        }
    }

    [Command]
    public void CmdUpdateControls(bool new_up, bool new_down, bool new_left, bool new_right, bool new_space, bool new_click, Vector2 new_mouse_position) {
        RpcUpdateControls(new_up, new_down, new_left, new_right, new_space, new_click, new_mouse_position);
    }

    [ClientRpc]
    public void RpcUpdateControls(bool new_up, bool new_down, bool new_left, bool new_right, bool new_space, bool new_click, Vector2 new_mouse_position) {
        up = new_up;
        down = new_down;
        left = new_left;
        right = new_right;
        pick_up = new_space;
        click = new_click;
        mouse_position = new_mouse_position;
    }

    public void changeName() {
        string new_name = sm.getName();

        if (isLocalPlayer) {
            if (!isServer) {
                CmdChangeName(new_name);
            }
            else {
                RpcUpdateChangeName(new_name);
            }
        }
    }

    [Command]
    public void CmdChangeName(string new_name) {
        RpcUpdateChangeName(new_name);
    }

    [ClientRpc]
    public void RpcUpdateChangeName(string new_name) {
        name_tag = new_name;
    }

    public void changeFlag() {
        if (isLocalPlayer) {
            if (!isServer) {
                CmdChangeFlag();
            }
            else {
                RpcUpdateChangeFlag();
            }
        }
    }

    [Command]
    public void CmdChangeFlag() {
        RpcUpdateChangeFlag();
    }

    [ClientRpc]
    public void RpcUpdateChangeFlag() {
        flag = sm.getFlag(flag);
    }

    [Command]
    public void CmdUpdatePing(int new_ping) {
        RpcUpdatePing(new_ping);
    }

    [ClientRpc]
    public void RpcUpdatePing(int new_ping) {
        ping = new_ping;
    }

    public void changeReady() {
        if (isLocalPlayer) {
            if (!isServer) {
                CmdChangeReady();
            }
            else {
                RpcUpdateChangeReady();
            }
        }
    }

    [Command]
    public void CmdChangeReady() {
        RpcUpdateChangeReady();
    }

    [ClientRpc]
    public void RpcUpdateChangeReady() {
        ready = !ready;
    }

}
