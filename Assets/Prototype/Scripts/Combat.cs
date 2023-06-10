using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public Transform attackPosition;
    public LayerMask enemies;
    public float attackRange;
    public int damage;

    // Start is called before the first frame update
    void Start()
    {
        // Enemies layer mask equals the enemies layer
        enemies = LayerMask.GetMask("Enemies");
        
        damage = 1;
        attackRange = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
                Collider2D[] enemiesToHurt = Physics2D.OverlapCircleAll(attackPosition.position, attackRange, enemies);
                for(int i = 0; i < enemiesToHurt.Length; i++) {
                    //enemiesToHurt[i].GetComponent<Enemy>().TakeDamage(damage);
                    enemiesToHurt[i].GetComponent<Pathfinding.TopDownEnemy>().TakeDamage(damage);
                }
                Debug.Log("input");
            }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPosition.position, attackRange);
    }
}
