using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    private const string PLAYER_TAG = "Player"; //플레이어 태그를 정의한다.

    private PlayerWeapon currentWeapon;

    [Header("총소리")]
    public AudioClip ShootAudio;

    [Header("장전")]
    public AudioClip ReloadAudio;

    [SerializeField]
    private Camera cam; //현재카메라

    [SerializeField]
    private LayerMask mask; //레이어

    private WeaponManager weaponManager; //weaponManager스크립트.

    void Start()
    {
        if(cam == null)
        {
            Debug.LogError("PlayerShoot : 카메라가 없다");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (PauseMenu.IsOn)
            return;

        // 재장전 버튼(R키)
        if(currentWeapon.bullets < currentWeapon.maxBullets)
        {
            if (Input.GetButtonDown("Reload"))
            {
                weaponManager.Reload();
                CmdReloadAudio();
                return;
            }
        }

        if(currentWeapon.fireRate <= 0f)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if(Input.GetButtonDown("Fire1")) //연사설정
            {
                InvokeRepeating("Shoot", 0f, 1f/currentWeapon.fireRate); //InvokeRepeating(실행할 함수, 지연 시간, 반복 호출 시간)
            }
            else if(Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot"); //인보크 중지
            }
        }
        
    }

    //플레이거가 사격할때 서버에서 호출한다.
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    //클라이언트에서 파티클 시스템 
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.weaponGraphics().muzzleFlash.Play();
    }

    //호출된다 서버에서 우리가 먼가를 쳣을때
    //히트포인트와 표면의 노멀값을 가져온다
    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }

    //모든 클라이언트에서 호출한다
    //총알 피격 파티클
    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(
            weaponManager.weaponGraphics().
            hitEffectPrefab, _pos, 
            Quaternion.LookRotation(_normal)); //피격 파티클 생성
        Destroy(_hitEffect, 2f); //피격 파티클 2초뒤에 삭제
    }

    [Command]
    void CmdCreateMap(Vector3 blockPos)
    {
        RpcCreateMap(blockPos);
    }

    [ClientRpc]
    void RpcCreateMap(Vector3 blockPos)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if ((!(x == 0 && y == 0 && z == 0)))
                    {
                        MapSapwn mapspawn = GameObject.Find("Map").GetComponent<MapSapwn>();
                        // Destroy된 자기 자신을 제외한 나머지부분의 애들을 보여지게 함
                        if (blockPos.x + x < 0 || blockPos.x + x > 100) continue;
                        if (blockPos.y + y < 0 || blockPos.y + y > 100) continue;
                        if (blockPos.z + z < 0 || blockPos.z + z > 100) continue;
                        Vector3 neighbour = new Vector3(blockPos.x + x, blockPos.y + y, blockPos.z + z);
                        mapspawn.DrawBlock(neighbour);
                    }
                }
            }
        }
    }

    [Command]
    void CmdDestroyCube(GameObject _cube)
    {
        RpcDestroyCube(_cube);
    }

    [ClientRpc]
    void RpcDestroyCube(GameObject _cube)
    {
        NetworkServer.Destroy(_cube);
    }

    [Command]
    void CmdShootAudio()
    {
        RpcShootAudio();
    }

    [ClientRpc]
    void RpcShootAudio()
    {
        AudioSource.PlayClipAtPoint(ShootAudio, transform.position);
    }

    [Command]
    void CmdReloadAudio()
    {
        RpcReloadAudio();
    }

    [ClientRpc]
    void RpcReloadAudio()
    {
        AudioSource.PlayClipAtPoint(ReloadAudio, transform.position);
    }

    [Client]
    void Shoot()
    {

        if(!isLocalPlayer || weaponManager.isReloading)
        {
            return;
        }

        if(currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
            CmdReloadAudio();
            return;
        }

        currentWeapon.bullets--;

        Debug.Log("Remaining bullets : " + currentWeapon.bullets);

        //사격, onshoot 메소드를 서버에서 호출한다.
        CmdOnShoot();
        CmdShootAudio();
        Debug.Log("빵야!");
        RaycastHit _hit;
        if(Physics.Raycast(
            cam.transform.position, 
            cam.transform.forward, 
            out _hit, currentWeapon.range, mask
            ))
        {
           if(_hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(_hit.collider.name , currentWeapon.damage, transform.name);
            }
            else if (_hit.collider.tag == "Cube")
            {
                Vector3 blockPos = _hit.transform.position;
                //NetworkServer.Destroy(_hit.collider.gameObject);
                if (blockPos.y <= 1 || blockPos.x == 0 || blockPos.x == 79 || blockPos.z == 79 || blockPos.z == 0)
                    return;
                CmdDestroyCube(_hit.collider.gameObject);
                CmdCreateMap(blockPos);
            }
            //맞는 체가 있을때 서버에 히트 메소드 호출
            CmdOnHit(_hit.point, _hit.normal);
        }

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
            CmdReloadAudio();
        }
    }

    [Command]
    void CmdPlayerShot(string _playerID,int _damage, string _sourceID)
    {
        Debug.Log(_playerID + " 총에맞음");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
    }
}
