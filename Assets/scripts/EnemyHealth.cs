using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyHealth : MonoBehaviour
{
    public float HealthEnemy;
    public GameObject enemy;
    EnemySpawner Spawner;
    public GameObject bloodSplatterPrefab;
    public GameObject deathPrefab;
  

    public GameObject enemyModel1;
    public GameObject enemyModel2;
    public GameObject enemyModel3;
    public GameObject enemyModel4;
    public AudioSource deathSound;
    public AudioSource hitSound;

    
    public void TakeDamage(float damage)
    {
        HealthEnemy -= damage;
        hitSound.Play();
        Instantiate(bloodSplatterPrefab, transform.position, Quaternion.identity);

        if (HealthEnemy <= 0)
        {
            
            Destroy(enemy);
            if (Spawner != null)
            {
                Spawner.currentWaveMonsters.Remove(this.enemy);
            }
            Instantiate(deathPrefab, transform.position, Quaternion.identity);
        }
    }

    public void setSpawnedEnemy(EnemySpawner _spawner)
    {
        Spawner = _spawner;
    }

        private void Update()
    {
        if (HealthEnemy < 75 && HealthEnemy > 50)
        {
            enemyModel1.SetActive(false);
            enemyModel2.SetActive(true);
            enemyModel3.SetActive(false);
            enemyModel4.SetActive(false);
        }
        else if (HealthEnemy < 50 && HealthEnemy > 25)
        {
            enemyModel1.SetActive(false);
            enemyModel2.SetActive(false);
            enemyModel3.SetActive(true);
            enemyModel4.SetActive(false);
        }
        else if (HealthEnemy < 25)
        {
            //deathSound.Play();
            enemyModel1.SetActive(false);
            enemyModel2.SetActive(false);
            enemyModel3.SetActive(false);
            enemyModel4.SetActive(true);
        }
    }
}
