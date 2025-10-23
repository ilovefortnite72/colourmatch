using System.Collections;
using Unity.VisualScripting;
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

    //move pebble to target position
    public void MoveToTarget(Vector2 _tarPos)
    {
        StartCoroutine(MoveCoroutine(_tarPos));
    }

    private IEnumerator MoveCoroutine(Vector2 _tarPos)
    {
        isMoving = true;
        float duration = 1f;

        Vector2 startPos = transform.position;
        float elapsed = 0f;


        while (elapsed < duration)
        {
            float t = elapsed / duration;

            transform.position = Vector2.Lerp(startPos, _tarPos, t);
            elapsed += Time.deltaTime;

            yield return null;
        }
        transform.position = _tarPos;

        isMoving = false;
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
