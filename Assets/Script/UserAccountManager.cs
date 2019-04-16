using System.Collections;
using DatabaseControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserAccountManager : MonoBehaviour
{


    public static UserAccountManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    //These store the username and password of the player when they have logged in
    public static string LoggedIn_Username { get; protected set; }
    private static string LoggedIn_Password = "";

    public static bool isLoggedIn { get; protected set; }

    //public static string LoggedInData { get; protected set; }

    public string loggedInSceneName = "Lobby";
    public string loggedOutSceneName = "LoginMenu";

    public delegate void OnDataReceivedCallBack(string data);

    public void LogOut()
    {

        LoggedIn_Username = "";
        LoggedIn_Password = "";

        isLoggedIn = false;
        Debug.Log("User logged out.");
        SceneManager.LoadScene(loggedOutSceneName);
    }


    public void LogIn(string username, string password)
    {
        LoggedIn_Username = username;
        LoggedIn_Password = password;

        isLoggedIn = true;
        Debug.Log("User logged in as " + username);

        SceneManager.LoadScene(loggedInSceneName);
    }


    public void SendData(string data)
    { //called when the 'Send Data' button on the data part is pressed
        if (isLoggedIn)
        {
            //ready to send request
            StartCoroutine(sendSendDataRequest(LoggedIn_Username, LoggedIn_Password, data)); //calls function to send: send data request
        }
    }

    IEnumerator sendSendDataRequest(string username, string password, string data)
    {
        IEnumerator e = DCF.SetUserData(username, password, data); // << Send request to set the player's data string. Provides the username, password and new data string
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
            //The data string was set correctly. Goes back to LoggedIn UI
            Debug.Log("Data successfully set.");
        }
        else
        {
            //There was another error. Automatically logs player out. This error message should never appear, but is here just in case.
            Debug.Log("Error: Unknown Error. Please try again later.");
        }
    }

    // For getting the data, i.e. KILLS AND DEATHS
    public void GetData(OnDataReceivedCallBack onDataReceived)
    { //called when the 'Get Data' button on the data part is pressed

        if (isLoggedIn)
        {
            //ready to send request
            StartCoroutine(sendGetDataRequest(LoggedIn_Username, LoggedIn_Password, onDataReceived)); //calls function to send get data request

        }
    }


    IEnumerator sendGetDataRequest(string username, string password, OnDataReceivedCallBack onDataReceived)
    {
        string data = "ERROR";

        IEnumerator e = DCF.GetUserData(username, password); // << Send request to get the player's data string. Provides the username and password
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Error")
        {
            Debug.Log("Data Upload Error. Could be a server error. To check try again, if problem still occurs, contact us.");
        }
        else
        {
            data = response;
        }

        if (onDataReceived != null)
            onDataReceived.Invoke(data);
    }


}