using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
public class Spawner : MonoBehaviour
{
    // The prefab to be used when enemy dies
    public GameObject enemy;
    public int waitTime;

    float timer = 0;
    // Update is called once per frame

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > waitTime)
        {
            // Spawn enemy
            GameObject newObject = Instantiate<GameObject>(enemy);
            newObject.transform.position = this.transform.position;
            timer -= 1;
        }
    }

}
