using UnityEngine;
using UnityEngine.UI;

public class UserAccount_Lobby : MonoBehaviour
{

    public Text usernameText;

    void Start()
    {
        if (UserAccountManager.isLoggedIn)
            usernameText.text = UserAccountManager.LoggedIn_Username;
    }

    public void LogOut()
    {
        if (UserAccountManager.isLoggedIn)
            UserAccountManager.instance.LogOut();
    }

}