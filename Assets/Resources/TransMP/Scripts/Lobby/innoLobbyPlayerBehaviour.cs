using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class innoLobbyPlayerBehaviour : MonoBehaviour
{

    // Components
    private innoMultiplayerServerBehaviour sm;

    public Text player_name;
    public Image player_flag;
    public Text player_ping;
    public GameObject player_check;

    public Button name_button;
    public Button flag_button;
    public Button check_button;

    public Sprite[] flag_sprites;

    // Settings
    public int player_num;
    public bool local;

    // Variables
    private bool buttons_init;
    private bool buttons_active;


    // Start is called before the first frame update
    void OnEnable()
    {
        // Find Lobby Object
        sm = innoMultiplayerServerBehaviour.instance;

        // Create Button Listeners
        if (!buttons_init) {
            buttons_init = true;
            buttons_active = true;

            name_button.onClick.AddListener(changeName);
            flag_button.onClick.AddListener(changeFlag);
            check_button.onClick.AddListener(changeCheck);
        }

        // Reset Graphics
        player_name.text = sm.player_objects[player_num].name_tag;
        player_flag.sprite = flag_sprites[sm.player_objects[player_num].flag];
        player_ping.text = "0ms";
        player_check.SetActive(sm.player_objects[player_num].ready);
    }

    // Update is called once per frame
    void Update()
    {
        // Update Local
        transform.GetChild(0).gameObject.SetActive(local);
        transform.GetChild(1).gameObject.SetActive(!local);
        if (buttons_active != local) {
            buttons_active = local;
            name_button.enabled = local;
            flag_button.enabled = local;
            check_button.enabled = local;
        }

        // Update Graphics
        player_name.text = sm.player_objects[player_num].name_tag;
        player_flag.sprite = flag_sprites[sm.player_objects[player_num].flag];
        player_ping.text = sm.player_objects[player_num].ping + "ms";
        player_check.SetActive(sm.player_objects[player_num].ready);
    }

    // Player Button Action methods
    public void changeName() {
        sm.player_objects[player_num].changeName();
    }

    public void changeFlag() {
        sm.player_objects[player_num].changeFlag();
    }

    public void changeCheck() {
        sm.player_objects[player_num].changeReady();
    }

}
