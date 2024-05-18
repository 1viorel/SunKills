using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour
{
    public float HealthPlayer;
    public Image healthBar;
    public GameObject bloodSplatterPrefab;

    public void TakeDamage(int damage)
    {
        Debug.Log("Player took damage");
        HealthPlayer -= damage;
        Instantiate(bloodSplatterPrefab, transform.position, Quaternion.identity);
        if (HealthPlayer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        healthBar.fillAmount = HealthPlayer / 200f;
    }

    
}
