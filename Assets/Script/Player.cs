using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkBehaviour {

    [SyncVar]
    private bool _isDead = false; //플레이어 사망 변수값
    public bool isDead //서버통신을하는 isDead의 보안강화
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField]
    private int maxHealth = 100; //플레이어 체력

    [SyncVar]
    private int currentHealth; //플레이어 동기화 체력

    public float GetHealthPct()
    {
        return (float)currentHealth / maxHealth;
    }

    [SyncVar]
    public string username = "Loading...";

    public int kills;
    public int deaths;

    [SerializeField]
    private Behaviour[] disalbeOnDeath; //죽음 판정에 오브젝트 작동 중지
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disalbeGameObjectOnDeath; //죽음 판정에 오브젝트 작종 중지

    [SerializeField]
    private GameObject deathEffect; //파괴 이펙트

    [SerializeField]
    private GameObject spawnEffect; //리스폰 이펙트

    private bool firstSeutup = true;

    public void SetupPlayer() //리모트와 로컬 플레이어 분리
    {
        if(isLocalPlayer)
        {
            //카메라 변경
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSteup>().playerUIInstance.SetActive(true);
        }
        CmdBroadCastNewPlayerSetup();
    }

    [Command]
    private void CmdBroadCastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if(firstSeutup)
        {
            wasEnabled = new bool[disalbeOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disalbeOnDeath[i].enabled;
            }
            firstSeutup = false;
        }
        
        SetDefaults();
    }

    //자살 함수
    void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }
        if(transform.position.x < 0 || transform.position.x > 80 || transform.position.z < 0 || transform.position.z > 80)
            CmdMapout();
    }

    [Command]
    void CmdMapout()
    {
        RpcMapout();
    }

    [ClientRpc]
    void RpcMapout()
    {
        if (isDead)
        {
            return;
        }
        currentHealth -= 99999;

        if (currentHealth <= 0)
        {
            MapDie();
        }
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _sourceID)
    {
        if(isDead)
        {
            return;
        }

        currentHealth -= _amount;

        Debug.Log(transform.name + "님의 체력은 " + currentHealth + "입니다.");

        if(currentHealth <= 0)
        {
            Die(_sourceID);
        }
    }

    private void Die(string _sourceID)
    {
        isDead = true;
        //컴포넌트 비활성화

        Player sourcePlayer = GameManager.GetPlayer(_sourceID);

        if(sourcePlayer != null)
        {
            sourcePlayer.kills++;
            GameManager.instance.onPlayerKilledCallback.Invoke(username, sourcePlayer.username);
        }
        
        deaths++;

        //Disalbe Componenets
        for (int i = 0; i < disalbeOnDeath.Length; i++)
        {
            disalbeOnDeath[i].enabled = false;
        }

        //Disalbe GameObject
        for (int i = 0; i < disalbeGameObjectOnDeath.Length; i++)
        {
            disalbeGameObjectOnDeath[i].SetActive(false);
        }


        //disable the collider
        Collider _col = GetComponent<Collider>(); //죽은 객체 충돌자 flase 값으로 세팅
        if (_col != null)
        {
            _col.enabled = false;
        }

        //spawn a death effect
        GameObject _gfxIns = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns,3f);


        //카메라 변경
        if(isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSteup>().playerUIInstance.SetActive(false);
        }

        Debug.Log(transform.name + " 죽었다!");

        //리스폰 활성화
        StartCoroutine(Respawn());
        
    }

    private void MapDie()
    {
        isDead = true;
        //컴포넌트 비활성화

        deaths++;

        //Disalbe Componenets
        for (int i = 0; i < disalbeOnDeath.Length; i++)
        {
            disalbeOnDeath[i].enabled = false;
        }

        //Disalbe GameObject
        for (int i = 0; i < disalbeGameObjectOnDeath.Length; i++)
        {
            disalbeGameObjectOnDeath[i].SetActive(false);
        }


        //disable the collider
        Collider _col = GetComponent<Collider>(); //죽은 객체 충돌자 flase 값으로 세팅
        if (_col != null)
        {
            _col.enabled = false;
        }

        //spawn a death effect
        GameObject _gfxIns = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 3f);


        //카메라 변경
        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSteup>().playerUIInstance.SetActive(false);
        }

        Debug.Log(transform.name + " 죽었다!");

        //리스폰 활성화
        StartCoroutine(Respawn());

    }

    private IEnumerator Respawn() //성능 개선을 위한 Coroutine ,yield 활용 yield return null << 다음 업데이트 까지 대기
    {
        // 리스폰 시간
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime); //GameManager에서 참조한 MatchSettings에 respawnTIme 값을 가져온다 (그값만큼 기다리는 yield 기능) 
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        yield return new WaitForSeconds(0.1f);

        SetupPlayer(); //다른 클라에게 메시지 보내기

        Debug.Log(transform.name + " 돌아왔다!");
    }

    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;

        //Enalbe the components        
        for (int i = 0; i < disalbeOnDeath.Length; i++)
        {
            disalbeOnDeath[i].enabled = wasEnabled[i];
        }

        //Enalbe the gameobject
        for (int i = 0; i < disalbeGameObjectOnDeath.Length; i++)
        {
            disalbeGameObjectOnDeath[i].SetActive(true);
        }

        //Enalbe the collider
        Collider _col = GetComponent<Collider>(); //충돌자 true값으로 세팅
        if(_col != null)
        {
            _col.enabled = true;
        }
               
        //리스폰 이펙트 생성
        GameObject _gfxIns = (GameObject)Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 3f);
    }
}
