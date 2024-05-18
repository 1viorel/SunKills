using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]

    public class WaveContent
    {
       [SerializeField] GameObject[] monsterSpawner;

       public GameObject[] GetMonsterSpawnerList(){
           return monsterSpawner;
       }
    }


    [SerializeField][NonReorderable] WaveContent[] waves;

    public static float GlobalDifficultyMultiplier = 1;
    int currentWave = 0;
    public Text text;
    float spawnRange = 50f;
    

    public bool checked_flag = false;
    public List<GameObject> currentWaveMonsters;
    // Start is called before the first frame update
    void Start()
    {
        text.text = "Wave: " + (currentWave+1);
        SpawnWave();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentWaveMonsters.Count == 0 && checked_flag==false)
           StartCoroutine(DelayedWaveStart());
        
    }

    public static float getGlobalDifficultyMultiplier() {
        return GlobalDifficultyMultiplier;
    }

    void SpawnWave() {
        for(int i=0; i<waves[currentWave].GetMonsterSpawnerList().Length; i++){
            GameObject newspawn = Instantiate(waves[currentWave].GetMonsterSpawnerList()[i], FindSpawnLocation(), Quaternion.identity);
            currentWaveMonsters.Add(newspawn);

            EnemyHealth monster = newspawn.GetComponent<EnemyHealth>();
            monster.setSpawnedEnemy(this); 
        }
    }

    Vector3 FindSpawnLocation() {
        Vector3 SpawnPos;
        float x = Random.Range(-spawnRange, spawnRange)+transform.position.x;
        float z = Random.Range(-spawnRange, spawnRange)+transform.position.z;
        float y = transform.position.y;

        SpawnPos = new Vector3(x, y, z);

        if (Physics.Raycast(SpawnPos, Vector3.down, 5))
            return SpawnPos;
        else
            return FindSpawnLocation();
        
    }

    private IEnumerator DelayedWaveStart() {
        checked_flag = true;
        yield return new WaitForSeconds(5.0f);
        
        GlobalDifficultyMultiplier *= 1.2f;
        currentWave++;
        text.text = "Wave: " + (currentWave + 1);

        SpawnWave();
        checked_flag = false;
    }
}


