using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerSteup))]

public class PlayerSteup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayerName = "DontDraw";

    [SerializeField]
    GameObject playerGrahpics;

    [SerializeField]
    GameObject playerUIPrefab;

    [HideInInspector]//숨기기
    public GameObject playerUIInstance;

    void Start()
    {
        //로컬플레이거 아닐시 구성요소 비활성화
        //로컬 플레이어의 캐릭터만 조작하기 위해서
        if (!isLocalPlayer)
        {
            DisalbeComponents();
            AssignRemotePlayer();
        }
        else
        {
           
            //disalbe player grahpics for loacl player
            SetLayerRecursively(playerGrahpics, LayerMask.NameToLayer(dontDrawLayerName));

            //PlayerUI 생성
            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;

            //Configure PlayerUI
            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
            if(ui == null)
            {
                Debug.LogError("PlayerUI 구성요소나 Prefab이 없다");
            }
            ui.SetPlayer(GetComponent<Player>());

            GetComponent<Player>().SetupPlayer();

            string _username = "Loading...";
            if (UserAccountManager.isLoggedIn)
                _username = UserAccountManager.LoggedIn_Username;
            else
                _username = transform.name;

            CmdSetUsername(transform.name, _username);
        }
    }

    [Command]
    void CmdSetUsername(string playerId, string username)
    {
        Player player = GameManager.GetPlayer(playerId);
        if(player != null)
        {
            Debug.Log(username + " has joined!");
            player.username = username;
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach(Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    void AssignRemotePlayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void DisalbeComponents()
    {

        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    void OnDisable()
    {

        Destroy(playerUIInstance);

        if(isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
        }
        GameManager.UnRegisterPlayer(transform.name);
    }

   
}
