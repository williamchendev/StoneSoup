using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoFieldBehaviour : Tile
{
    // Components
    public GameObject wheat_obj;
    private Animator anim;

    // Variables
    public bool being_farmed;
    private int wheat_growth_counter;

    private innoWindmillBehaviour windmill;

    // Initialize Field
    public virtual void Start () {
        // Finds the windmill of the field's room
        if (transform.parent.transform.Find("inno_industry_windmill") != null) {
            GameObject temp_windmill = transform.parent.transform.Find("inno_industry_windmill").gameObject;
            if (temp_windmill.GetComponent<innoWindmillBehaviour>() != null) {
                windmill = temp_windmill.GetComponent<innoWindmillBehaviour>();
                windmill.fields.Add(this);
            }
        }

        // Components
        anim = GetComponent<Animator>();

        // Variables
        wheat_growth_counter = 0;
        being_farmed = false;
    }

    // Grow the crop
    public void grow() {
        wheat_growth_counter++;
        // If crop has been farmed instantiate the bundle of wheat
        if (wheat_growth_counter > 5) {
            wheat_growth_counter = 0;
            GameObject new_wheat = Instantiate(wheat_obj);
            new_wheat.transform.position = transform.position;
        }
        // Play animation
        anim.Play("" + wheat_growth_counter);
    }
}
