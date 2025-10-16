using UnityEngine;

public class Node : MonoBehaviour
{

    public bool isUseable;

    public GameObject pebble;

    public Node(bool _isUseable, GameObject _pebble)
    {
        isUseable = _isUseable;
        pebble = _pebble;
    }
}
