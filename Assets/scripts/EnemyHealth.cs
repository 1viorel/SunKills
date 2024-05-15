using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyHealth : MonoBehaviour
{
    public int HealthEnemy;
    public GameObject enemy;
    EnemySpawner Spawner;
    public GameObject bloodSplatterPrefab;
    
    public void TakeDamage(int damage)
    {
        Debug.Log("Enemy took damage");
        HealthEnemy -= damage;
        Instantiate(bloodSplatterPrefab, transform.position, Quaternion.identity);

        if (HealthEnemy <= 0)
        {
            if (Spawner != null)
            {
                Spawner.currentWaveMonsters.Remove(this.enemy);
            }
           
            Destroy(enemy);
        }
    }

    public void setSpawnedEnemy(EnemySpawner _spawner)
    {
        Spawner = _spawner;
    }
}
