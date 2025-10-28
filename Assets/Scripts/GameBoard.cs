using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Unity.VisualScripting;

using UnityEditor;
using UnityEngine;

public class GameBoard : MonoBehaviour
{

    public int width = 10;
    public int height = 10;


    public float offsetX;
    public float offsetY;

    [SerializeField]
    public float pebbleSpacing;

    public GameObject[] pebblePrefab;

    public Node[,] gameBoard;

    public GameObject gameBoardGO;

    public ArrayLayout arrayLayout;

    public static GameBoard instance;

    public GameResult gameResult;

    public List<GameObject> pebblesToDestroy = new();

    [SerializeField]
    List<Pebbles> pebblesToRemove = new();

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

        pebblesToRemove.Clear();

        offsetX = (float)(width - 1) / 2;
        offsetY = (float)(height - 1) / 2;
        Debug.Log("Offset X: " + offsetX + " Offset Y: " + offsetY);


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                //where to spawn the pebble
                Vector2 position = new Vector2
                    ((x - offsetX) * pebbleSpacing,
                    (y - offsetY) * pebbleSpacing);
                
                
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
            
        }
    }

    public bool CheckMatch()
    {
        if(GameManager.Instance.isGameEnded)
        {
            return false;
        }

        bool matchFound = false;

        pebblesToRemove.Clear();

        foreach (Node nodePebble in gameBoard)
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
        
        return matchFound;
    }


    public IEnumerator ExecuteTurnOnMatchedBoard(bool _subtractMoves)
    {
        bool matchFound;
        do
        {
            foreach (Pebbles pebbles in pebblesToRemove)
            {
                pebbles.isMatched = false;
            }
            
            RemoveRefill(pebblesToRemove);
            
            yield return new WaitForSeconds(0.5f);
            matchFound = CheckMatch();
            //update score and moves
            Debug.Log("Total Pebbles Removed: " + pebblesToRemove.Count);
            GameManager.Instance.UpdatePoints(pebblesToRemove.Count);
        }
        while (matchFound);

        GameManager.Instance.ExecuteTurn(_subtractMoves);
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

        for(int _xIndex = 0; _xIndex < width; _xIndex++)
        {
            CollapseColumn(_xIndex);
            RefillColumn(_xIndex);
        }
    }

    private void RefillColumn(int _xIndex)
    {
        for (int _yIndex = 0; _yIndex < height; _yIndex++)
        {
            if (gameBoard[_xIndex, _yIndex].pebble == null)
            {
                
                int randomIndex = Random.Range(0, pebblePrefab.Length);
                Vector2 spawnPos = new Vector2
                    ((_xIndex - offsetX) * pebbleSpacing,
                    (height - offsetY + 1f)*pebbleSpacing);


                GameObject newPebble = Instantiate(pebblePrefab[randomIndex], spawnPos, Quaternion.identity);

                Pebbles pebbleScript = newPebble.GetComponent<Pebbles>();
                pebbleScript.SetIndicies(_xIndex, _yIndex);

                
                gameBoard[_xIndex, _yIndex] = new Node(true, newPebble);

                
                Vector3 targetPos = new Vector3
                    ((_xIndex - offsetX) * pebbleSpacing,
                    (_yIndex - offsetY) * pebbleSpacing,
                    newPebble.transform.position.z);
                pebbleScript.MoveToTarget(targetPos);
            }
        }
    }

    private void CollapseColumn(int _xIndex)
    {
        for (int _yIndex = 0; _yIndex < height; _yIndex++)
        {
            if (gameBoard[_xIndex, _yIndex].pebble == null)
            {
                for (int _aboveY = _yIndex + 1; _aboveY < height; _aboveY++)
                {
                    if (gameBoard[_xIndex, _aboveY].pebble != null)
                    {
                        Pebbles fallingPebble = gameBoard[_xIndex, _aboveY].pebble.GetComponent<Pebbles>();

                        Vector3 targetPos = new Vector3
                            ((_xIndex - offsetX) * pebbleSpacing,
                            (_yIndex - offsetY) * pebbleSpacing,
                            fallingPebble.transform.position.z);
                        fallingPebble.MoveToTarget(targetPos);

                        fallingPebble.SetIndicies(_xIndex, _yIndex);

                        gameBoard[_xIndex, _yIndex] = new Node(true, fallingPebble.gameObject);
                        gameBoard[_xIndex, _aboveY] = new Node(true, null);
                        break;
                    }
                }
            }
        }
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

    //swapping pebbles logic

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
    //try swap pebbles if adjacent
    private void SwapPebble(Pebbles _currentPebble, Pebbles _targetPebble)
    {
        //if not adjacent, return
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
        yield return new WaitForSeconds(0.5f);
        

        if (CheckMatch()) 
        { 
            StartCoroutine(ExecuteTurnOnMatchedBoard(true));
        }
        else
        {
            //no match found, swap back
            DoSwap(_currentPebble, _targetPebble);
        }

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

    //public void Wiggle()
    //{
    //    StartCoroutine(WiggleCoroutine());
    //}

    //private IEnumerator WiggleCoroutine()
    //{

    //}
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