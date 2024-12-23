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

    void Start()
    {
        enemy = (GameObject)Resources.Load("prefabs/enemy", typeof(GameObject));
    }

    void Update()
    {
        timer += Time.deltaTime;
        Debug.Log("Timer: " + timer + ", Wait Time: " + waitTime);

        if (timer > waitTime)
        {
            // Spawn enemy
            GameObject newObject = Instantiate<GameObject>(enemy);
            newObject.transform.position = this.transform.position;
            timer -= waitTime;
            Debug.Log("Enemy Spawn");
        }
    }

}
