using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoLobbyManagerBehaviour : MonoBehaviour
{
    // Components
    public innoMultiplayerServerBehaviour sm;

    // Settings
    public GameObject[] lobby_ids;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (sm != null) {
            transform.GetChild(0).gameObject.SetActive(true);
            for (int i = 0; i < 4; i++) {
                // Check if Lobby ID exists
                if (sm.player_objects[i] != null && sm.players_connected[i]) {
                    lobby_ids[i].SetActive(sm.players_connected[i]);
                    if (sm.players_connected[i]) {
                        // Update Lobby IDs & Variables
                        lobby_ids[i].GetComponent<innoLobbyPlayerBehaviour>().local = sm.player_objects[i].isLocalPlayer;
                    }
                }
                else {
                    lobby_ids[i].SetActive(false);
                }
            }
        }
        else {
            transform.GetChild(0).gameObject.SetActive(false);
            for (int i = 0; i < 4; i++) {
                lobby_ids[i].SetActive(false);
            }
        }
    }
}
