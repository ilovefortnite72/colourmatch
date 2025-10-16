using UnityEngine;

public class Node
{

    public bool isUseable;

    public GameObject pebble;

    public Node(bool _isUseable, GameObject _pebble)
    {
        isUseable = _isUseable;
        pebble = _pebble;
    }
}
