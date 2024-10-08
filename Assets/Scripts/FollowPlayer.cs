using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    [Header("Target")]
    // This is the target the enemy is going to move towards
	public Transform player;

    // Update is called once per frame
    void Update () {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
		// Do nothing if the target hasn't been assigned or it was detroyed for some reason
		if(player == null)
			return;

        // If using with orthographic camera
        // Sidescroller camera
        // this.transform.position = new Vector3(player.transform.position.x, 2.2f, -10);
        // Topdown camera
        transform.position = player.transform.position + new Vector3(0, 1, -5);
    }
}

