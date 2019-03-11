using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innoMultiplayerCamera : MonoBehaviour
{

    public GameObject follow_obj;
    public float cameraFollowSpeed = 10f;

    void Update() {
        if (follow_obj != null) {
            Vector2 new_pos = Vector2.Lerp(transform.position, follow_obj.transform.position, cameraFollowSpeed * Time.deltaTime);
		    transform.position = new Vector3(new_pos.x, new_pos.y, transform.position.z);
        }
	}

}
