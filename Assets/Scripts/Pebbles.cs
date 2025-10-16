using UnityEngine;

public class Pebbles : MonoBehaviour
{
    public PebbleType pebbleType;

    // Grid position
    public int xIndex;
    public int yIndex;

    //matching/swapping vars
    public bool isMatched;
    private Vector2 currentPos;
    private Vector2 targetPos;

    
    public bool isMoving;

    //initializer
    public Pebbles(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

}
//colour of pebble
public enum PebbleType
{
    Red,
    Blue,
    Green,
    Yellow,
}
