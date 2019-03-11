using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoMultiplayerNabori : Tile
{
    public int sprite_index;
    public Sprite[] sprites;
    public GameObject player_follow;

    private float sin_val;

    // Start is called before the first frame update
    void Start()
    {
        sin_val = 0;
        GetComponent<SpriteRenderer>().sprite = sprites[sprite_index];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, player_follow.transform.position, Time.deltaTime * 5f);
    }
}
