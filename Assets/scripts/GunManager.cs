using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunManager : MonoBehaviour
{

    [Header("Gun Stats")]
    public float damage;
    public float timeBetweenShots;
    public float range;
    public float reloadTime;
    public float spread;
    public float timeBetweenShooting;

    public int magSize;
    public int bulletsPerTap;

    public bool allowButtonHold;

    int bulletsLeft, bulletsShot;

    bool shooting, readyToShoot, reloading;

    public Camera playerCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    public PlayerController damageMultiplier;

    public Text text;

    void Awake() {
        
        bulletsLeft = magSize;
        readyToShoot = true;
    }

    void myInput() {
        
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magSize && !reloading) reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0) {
            bulletsShot = bulletsPerTap;
            shoot();
        }
    }

    void reload() {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);

    }

    void shoot() {
        readyToShoot = false;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 direction = playerCam.transform.forward + new Vector3(x, y, 0);

    
        if (Physics.Raycast(playerCam.transform.position, direction, out rayHit, range, whatIsEnemy)) {
            Debug.Log(rayHit.collider.name);

            
            if (rayHit.collider.CompareTag("Enemy")) {
                rayHit.collider.GetComponent<EnemyHealth>().TakeDamage(damage*damageMultiplier.damageMultiplier);
            }
        }

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0) {
            Invoke("shoot", timeBetweenShots);
        }
    }
     private void ResetShot()
    {
        readyToShoot = true;
    }

    void ReloadFinished() {

        bulletsLeft = magSize;
        reloading = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        myInput();
        text.text = (bulletsLeft + " / " + magSize);
    }
}



 