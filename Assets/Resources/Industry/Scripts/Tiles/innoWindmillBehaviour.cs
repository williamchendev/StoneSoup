using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoWindmillBehaviour : Tile
{
    // Objects
    public GameObject bread_obj;
    public float bread_spd = 300f;

    public List<innoFieldBehaviour> fields;
    public List<innoWindmillAttract> consumables;
    public List<float> consumables_wait;
    public GameObject farm_area_col_obj;
    public GameObject farmers_parent;

    // Components
    private Animator anim;
    private Collider2D col;

    // Variables
    private int wheat_count;

    // Initialize Name & Field List
    public virtual void Awake () {
        gameObject.name = "inno_industry_windmill";
        fields = new List<innoFieldBehaviour>();
        consumables = new List<innoWindmillAttract>();
        consumables_wait = new List<float>();
        farmers_parent = new GameObject("inno_windmill_farmers");
        farmers_parent.transform.position = transform.position;
        farmers_parent.transform.SetParent(transform);
    }

    // Initialize Farm Area
    public virtual void Start () {
        // Makes a farm collider for Farmers to figure out where the windmill is
        farm_area_col_obj = new GameObject("inno_farm_collider");
        farm_area_col_obj.transform.SetParent(transform);
        innoWindmillFarmArea temp_area = farm_area_col_obj.AddComponent<innoWindmillFarmArea>();
        temp_area.pos =  new Vector2(transform.parent.transform.position.x, transform.parent.transform.position.y);
        temp_area.windmill = this;

        // Components
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    // Update Consumable List
    public virtual void Update() {
        for (int i = consumables.Count - 1; i >= 0; i--) {
            innoWindmillAttract tile = consumables[i];

            if (!tile.can_attract || tile.windmill != this) {
                innoWindmillAttract temp_tile = tile;
                consumables.Remove(temp_tile);
                consumables_wait.RemoveAt(i);
                continue;
            }

            if (consumables_wait[i] <= 0) {
                if (tile.GetComponent<Rigidbody2D>() != null) {
                    tile.GetComponent<Rigidbody2D>().MovePosition(Vector2.MoveTowards(tile.transform.position, transform.position, Time.deltaTime * 9f));
                }
                else {
                    transform.position = Vector2.MoveTowards(tile.transform.position, transform.position, Time.deltaTime * 9f);
                }

                if (Vector2.Distance(tile.transform.position, transform.position) < 2.5f) {
                    innoWindmillAttract temp_tile = tile;
                    consumables.Remove(temp_tile);
                    consumables_wait.RemoveAt(i);
                    temp_tile.GetComponent<Tile>().takeDamage(this, temp_tile.GetComponent<Tile>().health + 1, DamageType.Normal);

                    wheat_count++;
                }
            }
            else {
                consumables_wait[i] -= Time.deltaTime;

                tile.transform.position += new Vector3(0f, 0.01f, 0);
            }
        }

        if (wheat_count >= 10) {
            wheat_count -= 10;
            
            GameObject temp_bread = Instantiate(bread_obj);
            temp_bread.transform.position = transform.position;
            float random_angle = Random.Range(0f, 2f * Mathf.PI);
            Physics2D.IgnoreCollision(col, temp_bread.GetComponent<Collider2D>(), true);
            temp_bread.GetComponent<Rigidbody2D>().AddForce(bread_spd * new Vector2(Mathf.Cos(random_angle), Mathf.Sin(random_angle)));
            temp_bread.AddComponent<innoWindmillRepulse>();
        }

        if (fields.Count > 0) {
            if (farmers_parent.transform.childCount > 14) {
                anim.speed = 4f;
            }
            else if (farmers_parent.transform.childCount > 8) {
                anim.speed = 2f;
            }
            else if (farmers_parent.transform.childCount > 3) {
                anim.speed = 1f;
            }
            else if (farmers_parent.transform.childCount > 0) {
                anim.speed = 0.5f;
            }
            else {
                anim.speed = 0;
            }
        }
        else {
            anim.speed = 0;
        }
    }

    public innoFieldBehaviour getField() {
        List<innoFieldBehaviour> new_list = new List<innoFieldBehaviour>();
        for (int i = fields.Count - 1; i >= 0; i--) {
            if (fields[i] != null) {
                if (!fields[i].being_farmed) {
                    new_list.Add(fields[i]);
                }
            }
            else {
                fields.RemoveAt(i);
            }
        }

        int random_index = Random.Range(0, new_list.Count - 1);
        if (new_list.Count > 0) {
            return new_list[random_index];
        }
        return null;
    }

}

public class innoWindmillFarmArea : MonoBehaviour {

    public Vector2 pos;

    public innoWindmillBehaviour windmill;

    private BoxCollider2D col;
    private Rigidbody2D rb;

    public void Start()
    {
        col = gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(15.5f, 11.5f);
        col.isTrigger = true;

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        transform.position = new Vector2(pos.x + 10f, pos.y + 8f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Tile>() != false) {
            Tile temp_tile = collision.gameObject.GetComponent<Tile>();
            if (temp_tile.hasTag(TileTags.Consumable)) {
                if (temp_tile.gameObject.GetComponent<innoWindmillRepulse>() != null) {
                    return;
                }

                if (temp_tile.gameObject.GetComponent<innoWindmillAttract>() == null) {
                    temp_tile.gameObject.AddComponent<innoWindmillAttract>().windmill = windmill;
                }
                else {
                    temp_tile.gameObject.GetComponent<innoWindmillAttract>().windmill = windmill;
                }
                windmill.consumables.Add(temp_tile.gameObject.GetComponent<innoWindmillAttract>());
                windmill.consumables_wait.Add(1f);
            }
        }
    }

}

public class innoWindmillAttract : MonoBehaviour {

    public bool can_attract;
    public innoWindmillBehaviour windmill;
    private Tile tile;

    void Awake()
    {
        can_attract = true;
        tile = GetComponent<Tile>();
    }

    void Update()
    {
        if (tile.isBeingHeld) {
            can_attract = false;
        }
    }

}

public class innoWindmillRepulse : MonoBehaviour {

}