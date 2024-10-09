using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    [Header("Target")]
    // This is the target the enemy is going to move towards
	public Transform player;

    // For choosing if orthographic camera scrolls sideways or not when following player
    public bool Sidescroll;

    // Update is called once per frame
    void Update () {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
		// Do nothing if the target hasn't been assigned or it was detroyed for some reason
		if(player == null)
			return;

        if(Sidescroll == true) {
            // Sidescroller camera
            this.transform.position = new Vector3(player.transform.position.x, 2.2f, -10);
        } else {
            // Topdown camera
            this.transform.position = player.transform.position + new Vector3(0, 1, -5);
        }
    }
}

