using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour
{
    public int HealthPlayer;
    public Text healthText;

    public void TakeDamage(int damage)
    {
        Debug.Log("Player took damage");
        HealthPlayer -= damage;
        if (HealthPlayer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        healthText.text = "Health: " + HealthPlayer.ToString();
    }

    
}
