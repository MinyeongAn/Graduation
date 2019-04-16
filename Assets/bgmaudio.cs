using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgmaudio : MonoBehaviour {
    public GameObject Lobbysound;
    void Awake()
    {
        DontDestroyOnLoad(Lobbysound);
    }
}
