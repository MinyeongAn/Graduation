using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour {

    [SerializeField]
    GameObject playerScoreboardItem;

    [SerializeField]
    Transform playerScoreboardList;

    void OnEnable()
    {
        Player[] players = GameManager.GetAllPlayers();
        
        foreach(Player player in players)
        {
            GameObject itemGo = (GameObject)Instantiate(playerScoreboardItem, playerScoreboardList);
            PlayerScoreboardItem item = itemGo.GetComponent<PlayerScoreboardItem>();
            if(item != null)
            {
                item.Setup(player.username, player.kills, player.deaths);
            }
            //Debug.Log(player.name + " | " + player.kills + " | " + player.deaths);
        }
    }

    void OnDisable()
    {
        foreach(Transform child in playerScoreboardList)
        {
            Destroy(child.gameObject);
        }
    }
}
