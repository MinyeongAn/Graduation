using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class WeaponManager : NetworkBehaviour {

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private PlayerWeapon primaryWeapon; //무기

    private PlayerWeapon currentWeapon; //현재 무기
    private WeaponGraphics currentGrahpics;

    public bool isReloading = false;

    void Start()
    {
        EquipWeapon(primaryWeapon);
    }

    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponGraphics weaponGraphics()
    {
        return currentGrahpics;
    }


    void EquipWeapon(PlayerWeapon _weapon)
    {
        currentWeapon = _weapon;

        GameObject _weaponIns = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
        _weaponIns.transform.SetParent(weaponHolder);

        currentGrahpics = _weaponIns.GetComponent<WeaponGraphics>();
        if(currentGrahpics == null)
        {
            Debug.LogError("무기 오브젝트에 무기 그래픽 구성요소가 없다:   " + _weaponIns.name);

        }

        if(isLocalPlayer)
        {
            Util.SetLayerRecursively(_weaponIns, LayerMask.NameToLayer(weaponLayerName));
        }
    }

    public void Reload()
    {
        if (isReloading)
            return;

        StartCoroutine(Reload_Coroutine());
    }

    private IEnumerator Reload_Coroutine()
    {
        Debug.Log("Reloading...");

        isReloading = true;

        CmdOnReload();

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.bullets = currentWeapon.maxBullets;

        isReloading = false;
    }

    [Command]
    void CmdOnReload()
    {
        RpcOnReload();
    }

    [ClientRpc]
    void RpcOnReload()
    {
        Animator anim = currentGrahpics.GetComponent<Animator>();
        if(anim != null)
        {
            anim.SetTrigger("Reload");
        }
    }
}
