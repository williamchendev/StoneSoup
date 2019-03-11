using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoMultiplayerPlayer : Player
{
    // Components
    [HideInInspector] public int client_index = -1;
    [HideInInspector] public innoMultiplayerPlayerBehaviour local_player;

    // Variables
    [HideInInspector] public bool isDead;

    // Initialization Event
    void Start()
    {
        init();
    }

    // Update Event
    public override void Update()
    {
        if (isDead) {
            return;
        }

        if (local_player != null) {
            // Update our aim direction
		    Vector2 mousePosition = local_player.mouse_position;
		    Vector2 toMouse = (mousePosition - (Vector2)transform.position).normalized;
		    aimDirection = toMouse;

		    // Update our invincibility frame counter.
		    if (_iFrameTimer > 0) {
			    _iFrameTimer -= Time.deltaTime;
			    _sprite.enabled = !_sprite.enabled;
			    if (_iFrameTimer <= 0) {
				    _sprite.enabled = true;
			    }
		    }

		    // If we press space, we're attempting to either pickup, drop, or switch items.
		    if (local_player.pick_up) {
			    bool pickedUpOrDroppedItem = false;

			    // First, drop the item we're holding
			    if (tileWereHolding != null) {
				    // Keep track of the fact that we just dropped this item so we don't pick it up again.
				    _lastTileWeHeld = tileWereHolding;
				    // Put it at out feet
				    tileWereHolding.dropped(this);

				    // If we're no longer holding an item, we successfully dropped it.
				    if (tileWereHolding == null) {
					    pickedUpOrDroppedItem = true;
				    }
			    }


			    // If we successully dropped the item
			    if (tileWereHolding == null) {
				    // Check to see if we're on top of an item that can be held
				    int numObjectsFound = _body.Cast(Vector2.zero, _maybeRaycastResults);
				    for (int i = 0; i < numObjectsFound && i < _maybeRaycastResults.Length; i++) {
					    RaycastHit2D result = _maybeRaycastResults[i];
					    Tile tileHit = result.transform.GetComponent<Tile>();
					    // Ignore the tile we just dropped
					    if (tileHit == null || tileHit == _lastTileWeHeld) {
						    continue;
					    }
					    if (tileHit.hasTag(TileTags.CanBeHeld)) {
						    tileHit.pickUp(this);
						    if (tileWereHolding != null) {
							    pickedUpOrDroppedItem = true;
							    break;
						    }
					    }
				    }
			    }

			    if (pickedUpOrDroppedItem) {
				    AudioManager.playAudio(pickupDropSound);
			    }

			    // Finally, clear the last tile we held so we can pick it up again next frame if we want to
			    _lastTileWeHeld = null;
		    }

		    // If we click the mouse, we try to use whatever item we're holding.
		    if (local_player.click) {
			    if (tileWereHolding != null) {
				    tileWereHolding.useAsItem(this);
			    }
		    }
        }
        else {
            isDead = true;
        }

		updateSpriteSorting();
    }

    // Physics Update Event
    public override void FixedUpdate()
    {
        if (isDead) {
            handSymbol.SetActive(false);
            return;
        }

        if (local_player != null) {
            bool tryToMoveUp = local_player.up;
		    bool tryToMoveRight = local_player.right;
		    bool tryToMoveDown = local_player.down;
		    bool tryToMoveLeft = local_player.left;

            Vector2 attemptToMoveDir = Vector2.zero;

		    if (tryToMoveUp) {
			    attemptToMoveDir += Vector2.up;
		    }
		    else if (tryToMoveDown) {
			    attemptToMoveDir -= Vector2.up;			
		    }
		    if (tryToMoveRight) {
			    attemptToMoveDir += Vector2.right;
		    }
		    else if (tryToMoveLeft) {
			    attemptToMoveDir -= Vector2.right;
		    }
		    attemptToMoveDir.Normalize();

		    // We flip our sprite based on whether we're facing right or not.
		    if (attemptToMoveDir.x > 0) {
			    _sprite.flipX = false;
		    }
		    else if (attemptToMoveDir.x < 0) {
			    _sprite.flipX = true;
		    }

		    // We use the walk direction variable to tell our animator what animation to play.
		    if (attemptToMoveDir.y > 0 && attemptToMoveDir.x == 0) {
			    _walkDirection = 0;
		    }
		    else if (attemptToMoveDir.y < 0 && attemptToMoveDir.x == 0) {
			    _walkDirection = 2;
		    }
		    else if (attemptToMoveDir.x != 0) {
			    _walkDirection = 1;
		    }
		    _anim.SetBool("Walking", attemptToMoveDir.x != 0 || attemptToMoveDir.y != 0);
		    _anim.SetInteger("Direction", _walkDirection);

		    // Finally, here's where we actually move.
		    moveViaVelocity(attemptToMoveDir, moveSpeed, moveAcceleration);

		    // Now check if we're on top of an item we can pick up, if so, display the hand symbol.
		    bool onItem = false;
		    int numObjectsFound = _body.Cast(Vector2.zero, _maybeRaycastResults);
		    for (int i = 0; i < numObjectsFound && i < _maybeRaycastResults.Length; i++) {
			    RaycastHit2D result = _maybeRaycastResults[i];
			    Tile tileHit = result.transform.GetComponent<Tile>();
			    if (tileHit != null && tileHit.hasTag(TileTags.CanBeHeld)) {
				    onItem = true;
				    if (tileWereHolding != null) {
					    break;
				    }
			    }
		    }
		    handSymbol.SetActive(onItem);
        }
        else {
            isDead = true;
        }
    }

    // Take Damage
    public override void takeDamage(Tile tileDamagingUs, int amount, DamageType damageType) {
		health -= amount;
        if (health <= 0) {

        }
	}
}
