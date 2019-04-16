using UnityEngine;

[System.Serializable] //일차원 배열 형태로 변환
public class PlayerWeapon{

    public string name = "Glock";
    public int damage = 10;
    public float range = 100f;

    public float fireRate = 0f;

    public int maxBullets = 30;

    [HideInInspector]
    public int bullets;

    public float reloadTime = 1f;

    public GameObject graphics;

    public PlayerWeapon()
    {
        bullets = maxBullets;
    }
}
