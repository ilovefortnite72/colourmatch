using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameBoard : MonoBehaviour
{

    public int width = 10;
    public int height = 10;

    public float offsetX;
    public float offsetY;


    public GameObject[] pebblePrefab;

    public Node[,] gameBoard;

    public GameObject gameBoardGO;

    public ArrayLayout arrayLayout;

    public static GameBoard instance;

    public GameResult gameResult;


    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        InitialiseBoard();
    }

    void InitialiseBoard()
    {
        gameBoard = new Node[width, height];


        offsetX = (float)(width - 1) / 2;
        offsetY = (float)(height - 1) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                //where to spawn the pebble
                Vector2 position = new Vector2(x - offsetX, y - offsetY);

                if (arrayLayout.rows[0].row[0])
                {
                    gameBoard[x, y] = new Node(false, null);

                }
                else
                {
                    //get random potion
                    int randomIndex = Random.Range(0, pebblePrefab.Length);
                    //spawn pebble at random position
                    GameObject pebble = Instantiate(pebblePrefab[randomIndex], position, Quaternion.identity);

                    pebble.GetComponent<Pebbles>().SetIndicies(x, y);
                    gameBoard[x, y] = new Node(true, pebble);
                }

            }
        }

        if (CheckMatch())
        {
            Debug.Log("Already have matches, resetting board");
            
        }
        else
        {
            Debug.Log("No matches found, board is ready");
        }
    }

    bool FreshBoardChecker()
    {
        bool matchFound = false;

        List<Pebbles> pebblesToReplace = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gameBoard[x, y].isUseable)
                {
                    Pebbles pebble = gameBoard[x, y].pebble.GetComponent<Pebbles>();
                    if (!pebble.isMatched)
                    {
                        GameResult matchedPebbles = isConnected(pebble);
                        if (matchedPebbles.connectedPebbles.Count >= 3)
                        {
                            pebblesToReplace.AddRange(matchedPebbles.connectedPebbles);
                            foreach (Pebbles p in matchedPebbles.connectedPebbles)
                            {
                                p.isMatched = true;
                            }
                            matchFound = true;
                        }
                    }
                }
            }
        }

        foreach (Pebbles p in pebblesToReplace)
        {
            //get pebble position
            int x = p.xIndex;
            int y = p.yIndex;
            
            //remove pebble
            Destroy(gameBoard[x, y].pebble);

            //spawn new pebble in old pebble place
            int randomIndex = Random.Range(0, pebblePrefab.Length);
            Vector2 position = new Vector2(x - offsetX, y - offsetY);

            GameObject newPebble = Instantiate(pebblePrefab[randomIndex], position, Quaternion.identity);

            newPebble.GetComponent<Pebbles>().SetIndicies(x, y);
            gameBoard[x, y].pebble = newPebble;
            p.isMatched = false;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gameBoard[x, y].isUseable && gameBoard[x, y].pebble != null)
                {
                    Pebbles pebble = gameBoard[x, y].pebble.GetComponent<Pebbles>();
                    pebble.isMatched = false;
                }
            }
        }

        return matchFound;
    }

    public bool CheckMatch()
    {
        Debug.Log("Checking for matches");
        bool matchFound = false;

        List<Pebbles> pebblesToRemove = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gameBoard[x, y].isUseable)
                {
                    Pebbles pebble = gameBoard[x, y].pebble.GetComponent<Pebbles>();


                    if (!pebble.isMatched)
                    {

                        GameResult matchedPebbles = isConnected(pebble);

                        if (matchedPebbles.connectedPebbles.Count >= 3)
                        {
                            pebblesToRemove.AddRange(matchedPebbles.connectedPebbles);

                            foreach (Pebbles pebbles in matchedPebbles.connectedPebbles)
                            {
                                pebbles.isMatched = true;
                            }
                            matchFound = true;

                        }
                    }
                }
            }
        }
        return matchFound;
    }


    GameResult isConnected(Pebbles pebble)
    {
        List<Pebbles> connectedPebbles = new();
        PebbleType pebbleType = pebble.pebbleType;


        connectedPebbles.Add(pebble);




        //check right--------------------------------------------------------------------------------------------------------------------------
        CheckDirection(pebble, new Vector2Int(1, 0), connectedPebbles);


        //check left--------------------------------------------------------------------------------------------------------------------------
        CheckDirection(pebble, new Vector2Int(-1, 0), connectedPebbles);



        if (connectedPebbles.Count == 3)
        {
            //horizontal match present
            Debug.Log("Horizontal match found of " + connectedPebbles.Count + " pebbles of type " + pebbleType);
            return new GameResult
            {
                connectedPebbles = connectedPebbles,
                direction = MatchDirection.Horizontal
            };
        }
        else if (connectedPebbles.Count > 3)
        {
            //Long horizontal match present
            Debug.Log("Long Horizontal match found of " + connectedPebbles.Count + " pebbles of type " + pebbleType);
            return new GameResult
            {
                connectedPebbles = connectedPebbles,
                direction = MatchDirection.LongHorizontal
            };
        }

        connectedPebbles.Clear();

        connectedPebbles.Add(pebble);

        //check up--------------------------------------------------------------------------------------------------------------------------
        CheckDirection(pebble, new Vector2Int(0, 1), connectedPebbles);



        //check down--------------------------------------------------------------------------------------------------------------------------
        CheckDirection(pebble, new Vector2Int(0, -1), connectedPebbles);

        if (connectedPebbles.Count == 3)
        {
            //Vertical match present
            Debug.Log("Vertical match found of " + connectedPebbles.Count + " pebbles of type " + pebbleType);
            return new GameResult
            {
                connectedPebbles = connectedPebbles,
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedPebbles.Count > 3)
        {
            //Long Vertical match present
            Debug.Log("Long Vertical match found of " + connectedPebbles.Count + " pebbles of type " + pebbleType);
            return new GameResult
            {
                connectedPebbles = connectedPebbles,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new GameResult
            {
                connectedPebbles = connectedPebbles,
                direction = MatchDirection.None
            };
        }


    }



    void CheckDirection(Pebbles pebble, Vector2Int direction, List<Pebbles> connectedPebbles)
    {
        PebbleType pebbleType = pebble.pebbleType;
        int x = pebble.xIndex + direction.x;
        int y = pebble.yIndex + direction.y;

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (gameBoard[x, y].isUseable)
            {
                Pebbles nextPebble = gameBoard[x, y].pebble.GetComponent<Pebbles>();
                if (nextPebble.pebbleType == pebbleType && !nextPebble.isMatched)
                {
                    connectedPebbles.Add(nextPebble);
                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }



    
}


public class GameResult
{
    public List<Pebbles> connectedPebbles;

    public MatchDirection direction;



}

public enum MatchDirection
{
    Horizontal,
    Vertical,
    LongVertical,
    LongHorizontal,
    Super,
    None,
}