using NUnit.Framework;
using System.Collections;
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

    public List<GameObject> pebblesToDestroy = new();

    [SerializeField]
    private Pebbles selectedPebble;

    [SerializeField]
    private bool isProcessingSwap;



    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray =  Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if(hit.collider != null && hit.collider.gameObject.GetComponent<Pebbles>())
            {
                if(isProcessingSwap)
                {
                    return;
                }

                Pebbles pebbles = hit.collider.gameObject.GetComponent<Pebbles>();
                Debug.Log("Clicked on pebble: "+pebbles.gameObject);

                SelectSquare(pebbles);
            }
        }
    }

    void Start()
    {
        InitialiseBoard();
    }

    void InitialiseBoard()
    {
        DestroyPotions();
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
                    pebblesToDestroy.Add(pebble);
                }

            }
        }

        if (CheckMatch())
        {
            Debug.Log("Reinitializing board to avoid starting matches");
            InitialiseBoard();
        }
        else
        {
            Debug.Log("Board initialized without starting matches");
        }

    }

    private void DestroyPotions()
    {
        if (pebblesToDestroy != null)
        {
            foreach (GameObject pebble in pebblesToDestroy)
            {
                Destroy(pebble);
            }
            pebblesToDestroy.Clear();
        }
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
        print(matchFound);
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




    //swapping potions logic

    public void SelectSquare(Pebbles _pebbles)
    {
        if (selectedPebble == null)
        {
            Debug.Log("Selected pebble at (" + _pebbles.xIndex + ", " + _pebbles.yIndex + ")");
            selectedPebble = _pebbles;
        }
        else if (selectedPebble == _pebbles)
        {
            selectedPebble = null;
        }
        else if (selectedPebble != _pebbles)
        {
            SwapPebble(selectedPebble, _pebbles);
            selectedPebble = null;
        }

    }

    private void SwapPebble(Pebbles _currentPebble, Pebbles _targetPebble)
    {
        if (!isAdjacent(_currentPebble, _targetPebble) || isProcessingSwap)
        {
            return;
        }

        DoSwap(_currentPebble, _targetPebble);

        isProcessingSwap = true;

        StartCoroutine(ExecuteMatch(_currentPebble, _targetPebble));

    }

    private IEnumerator ExecuteMatch(Pebbles _currentPebble, Pebbles _targetPebble)
    {
        bool matchFound = CheckMatch();

        if (!matchFound)
        {
            //no match found, swap back
            yield return new WaitForSeconds(0.5f);
            DoSwap(_currentPebble, _targetPebble);
            isProcessingSwap = false;
            yield break;
        }
        yield return new WaitForSeconds(0.5f);

        isProcessingSwap = false;
    }

    private void DoSwap(Pebbles _currentPebble, Pebbles _targetPebble)
    {
        //swap in array
        GameObject temp = gameBoard[_currentPebble.xIndex, _currentPebble.yIndex].pebble;
        gameBoard[_currentPebble.xIndex, _currentPebble.yIndex].pebble = gameBoard[_targetPebble.xIndex, _targetPebble.yIndex].pebble;
        gameBoard[_targetPebble.xIndex, _targetPebble.yIndex].pebble = temp;

        //swap indicies
        int tempX = _currentPebble.xIndex;
        int tempY = _currentPebble.yIndex;
        _currentPebble.xIndex = _targetPebble.xIndex;
        _currentPebble.yIndex = _targetPebble.yIndex;
        _targetPebble.xIndex = tempX;
        _targetPebble.yIndex = tempY;

        //call coroutine in pebble script to visibly move them
        _currentPebble.MoveToTarget(gameBoard[_targetPebble.xIndex, _targetPebble.yIndex].pebble.transform.position);
        _targetPebble.MoveToTarget(gameBoard[_currentPebble.xIndex, _currentPebble.yIndex].pebble.transform.position);


    }

    
     

    private bool isAdjacent(Pebbles _currentPebble, Pebbles _targetPebble)
    {
        return Mathf.Abs(_currentPebble.xIndex - _targetPebble.xIndex) + Mathf.Abs(_currentPebble.yIndex - _targetPebble.yIndex) == 1;
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