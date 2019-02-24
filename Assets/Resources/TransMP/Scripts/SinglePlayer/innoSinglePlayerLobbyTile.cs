using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoSinglePlayerLobbyTile : Tile
{

    public GameObject teleport_effect;
    public GameObject transition_obj;

    private float timer;
    private bool lobby_scene;

    public override void pickUp(Tile tilePickingUsUp)
    {
        // Teleport Animation
        GameObject teleport = Instantiate(teleport_effect);
        teleport.transform.position = tilePickingUsUp.gameObject.transform.position;

        // Check if Game Object is the Player
        if (tilePickingUsUp.hasTag(TileTags.Player)) {
            GameObject transition = Instantiate(transition_obj);
            Vector3 cam_pos = Camera.main.transform.position;
            cam_pos.z = cam_pos.z + 1f;
            transition.transform.position = cam_pos;
        }
        else {
            Destroy(tilePickingUsUp.gameObject);
            return;
        }

        tilePickingUsUp.gameObject.SetActive(false);
    }

    public void createScene() {

    }

}
