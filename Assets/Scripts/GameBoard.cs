using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

using UnityEditor;
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
        DestroyPebbles();
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

        

    }

    private void DestroyPebbles()
    {
        if (pebblesToDestroy != null)
        {
            foreach (GameObject pebble in pebblesToDestroy)
            {
                Destroy(pebble);
            }
            pebblesToDestroy.Clear();
            Debug.Log(pebblesToDestroy.Count);
        }
    }

    public bool CheckMatch(bool _executeAction)
    {
        
        bool matchFound = false;

        List<Pebbles> pebblesToRemove = new();

        foreach(Node nodePebble in gameBoard)
        {
            if(nodePebble.pebble != null)
            {
                nodePebble.pebble.GetComponent<Pebbles>().isMatched = false;
            }
        }


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
        if (_executeAction)
        {
            foreach(Pebbles pebbles in pebblesToRemove)
            {
                pebbles.isMatched = false;
            }
            RemoveRefill(pebblesToRemove);

            if (CheckMatch(false))
            {
                CheckMatch(true);
            }
        }
        return matchFound;
    }

    private void RemoveRefill(List<Pebbles>pebblesToRemove)
    {
        foreach(Pebbles pebbles in pebblesToRemove)
        {
            int _xIndex = pebbles.xIndex;
            int _yIndex = pebbles.yIndex;

            Destroy(pebbles.gameObject);

            gameBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(gameBoard[x,y].pebble == null)
                {
                    RefillNode(x, y);
                }
                    
            }
        }
    }

    private void RefillNode(int x, int y)
    {
        int yOffset = 1;
        //check if there are empty nodes above current node or top of board
        while (y + yOffset < height && gameBoard[x,y + yOffset].pebble == null)
        {
            yOffset++;
        }
        //move to correct position
        if (y + yOffset < height && gameBoard[x, y + yOffset].pebble != null)
        {
            Pebbles pebbleAbove = gameBoard[x, y + yOffset].pebble.GetComponent<Pebbles>();

            Vector3 targetpos = new Vector3(x - offsetX, y - offsetY, pebbleAbove.transform.position.z);
            //update position
            pebbleAbove.MoveToTarget(targetpos);
            //update indicies
            pebbleAbove.SetIndicies(x, y);
            //update gameboard array
            gameBoard[x,y] = gameBoard[x, y + yOffset];
            gameBoard[x, y + yOffset] = new Node(true, null);

        }
        if(y+yOffset == height)
        {
            SpawnPotionAtTop(x);
        }
    }

    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMove = 9 - index;

        int randomIndex = Random.Range(0, pebblePrefab.Length);

        GameObject newPebble = Instantiate(pebblePrefab[randomIndex], new Vector2(x - offsetX, (height - offsetY)), Quaternion.identity);

        newPebble.GetComponent<Pebbles>().SetIndicies(x, index);

        gameBoard[x, index] = new Node(true, newPebble);
        Vector3 targetPos = new Vector3(newPebble.transform.position.x, newPebble.transform.position.y, -locationToMove);
        newPebble.GetComponent<Pebbles>().MoveToTarget(targetPos);
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for(int y = 9; y >=0 ; y--)
        {
            if(gameBoard[x,y].pebble == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
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
            

            return new GameResult
            {
                connectedPebbles = connectedPebbles,
                direction = MatchDirection.Horizontal
            };
        }
        else if (connectedPebbles.Count > 3)
        {
            //Long horizontal match present
            

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
            

            return new GameResult
            {
                connectedPebbles = connectedPebbles,
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedPebbles.Count > 3)
        {
            //Long Vertical match present
            

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
        bool matchFound = CheckMatch(true);
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