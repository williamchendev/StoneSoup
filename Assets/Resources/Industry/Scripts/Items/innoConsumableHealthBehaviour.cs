using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoConsumableHealthBehaviour : Tile
{

    public int heal_amount;

    public override void useAsItem(Tile tileUsingUs) {
		tileUsingUs.health += heal_amount;
        Destroy(gameObject);
	}

    protected virtual void Update() {
        if (_tileHoldingUs != null) {
			transform.localPosition = Vector3.zero;
		}
    }

}
