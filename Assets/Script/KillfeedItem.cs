using UnityEngine;
using UnityEngine.UI;

public class KillfeedItem : MonoBehaviour {

    [SerializeField]
    Text text;

    public void Setup(string player, string source)
    {
        text.text = "<b>" + source + "</b>" + "<color=#FF0000>" + " killed " + "</color>" + "<b>" + player + "</b>";
    }

}
