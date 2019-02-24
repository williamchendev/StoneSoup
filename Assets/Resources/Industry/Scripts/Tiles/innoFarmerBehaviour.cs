using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class innoFarmerBehaviour : Tile
{
    // Components
    private Animator anim;
    private Rigidbody2D rb;

    // Settings
    public float step_delay = 0.45f;
    public float harvest_delay = 3f;

    // Variables
    private float step_time;
    private float harvest_time;

    private bool field_snap;
    private Vector2 velocity;

    private innoFieldBehaviour field;
    private innoWindmillBehaviour windmill;

    // Initialize Name
    public virtual void Awake () {
        gameObject.name = "inno_industry_farmer";
    }

    // Initialize Farmer
    public virtual void Start () {
        // Components
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Variables
        step_time = step_delay + 1f;
    }

    // Update Event
    public virtual void Update () {
        if (!isBeingHeld) {
            step_time += Time.deltaTime;

            field_snap = false;
            velocity = Vector2.zero;
            if (field != null & windmill != null) {
                if (Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(field.transform.position.x, field.transform.position.y)) > 0.25f) {
                    velocity = Vector2.MoveTowards(Vector2.zero, field.transform.position - transform.position, Time.deltaTime * 3f);
                    anim.Play("walk");
                }
                else {
                    harvest_time += Time.deltaTime;

                    if (harvest_time > harvest_delay) {
                        field.grow();
                        harvest_time = 0;
                    }
                    anim.Play("harvest");
                }
            }
            else {
                if (step_time > step_delay) {
                    anim.Play("idle");
                    if (windmill != null) {
                        if (field == null) {
                            field = windmill.getField();
                            if (field != null) {
                                field.being_farmed = true;
                            }
                        }
                    }

                    step_time = 0;
                }
            }
        }
        else {
            transform.localPosition = new Vector3(0f, -0.05f, 0f);
        }
    }

    public virtual void FixedUpdate() {
        if (!isBeingHeld) {
            if (velocity != Vector2.zero) {
                rb.MovePosition(new Vector2(transform.position.x, transform.position.y) + velocity);
                velocity = Vector2.zero;
            }

            if (field != null) {
                if (Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(field.transform.position.x, field.transform.position.y)) <= 0.25f) {
                    if (!field_snap) {
                        rb.velocity = Vector2.zero;
                        rb.MovePosition(field.transform.position);
                        transform.position = field.transform.position;
                        velocity = Vector2.zero;

                        field_snap = true;
                    }
                }
                else {
                    field_snap = false;
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isBeingHeld) {
            if (other.gameObject.name == "inno_farm_collider") {
                if (windmill != other.gameObject.GetComponent<innoWindmillFarmArea>().windmill) {
                    if (field != null) {
                        field.being_farmed = false;
                    }
                    windmill = other.gameObject.GetComponent<innoWindmillFarmArea>().windmill;
                    transform.SetParent(windmill.transform.GetChild(0));
                    for (int i = 0; i < windmill.transform.GetChild(0).childCount; i++) {
                        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), windmill.transform.GetChild(0).GetChild(i).GetComponent<Collider2D>(), true);
                    }
                    step_time = 0;
                }
            }
        }
    }

    public override void pickUp(Tile tilePickingUsUp) {
        transform.parent = null;
        if (field != null) {
            field.being_farmed = false;
        }
        field = null;
        windmill = null;
        anim.Play("idle");

        Physics2D.IgnoreCollision(tilePickingUsUp.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);

        base.pickUp(tilePickingUsUp);
	}

    public override void dropped(Tile tileDroppingUs) {
		base.dropped(tileDroppingUs);
        step_time = step_delay + 1;

        Physics2D.IgnoreCollision(tileDroppingUs.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
	}

}
