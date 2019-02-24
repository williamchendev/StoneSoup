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
        // Update Ping
        if (isLocalPlayer) {
            int game_ping = NetworkManager.singleton.client.GetRTT();
            if (!isServer) {
                CmdUpdatePing(game_ping);
            }
            else {
                RpcUpdatePing(game_ping);
            }
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
