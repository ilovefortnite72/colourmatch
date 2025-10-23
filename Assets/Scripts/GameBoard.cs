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
                // Pick random prefab and spawn just above the board
                int randomIndex = Random.Range(0, pebblePrefab.Length);
                Vector2 spawnPos = new Vector2(_xIndex - offsetX, height - offsetY + 1f); // +1 for nice fall
                GameObject newPebble = Instantiate(pebblePrefab[randomIndex], spawnPos, Quaternion.identity);

                Pebbles pebbleScript = newPebble.GetComponent<Pebbles>();
                pebbleScript.SetIndicies(_xIndex, _yIndex);

                // Update board
                gameBoard[_xIndex, _yIndex] = new Node(true, newPebble);

                // Move pebble visually into position
                Vector3 targetPos = new Vector3(_xIndex - offsetX, _yIndex - offsetY, newPebble.transform.position.z);
                pebbleScript.MoveToTarget(targetPos);
            }
        }
    }

    private void CollapseColumn(int _xIndex)
    {
        for (int _yIndex = 0; _yIndex < height; _yIndex++)
        {
            // If this cell is empty, look for a pebble above to pull down
            if (gameBoard[_xIndex, _yIndex].pebble == null)
            {
                for (int _aboveY = _yIndex + 1; _aboveY < height; _aboveY++)
                {
                    if (gameBoard[_xIndex, _aboveY].pebble != null)
                    {
                        Pebbles fallingPebble = gameBoard[_xIndex, _aboveY].pebble.GetComponent<Pebbles>();

                        // Calculate target position in world space
                        Vector3 targetPos = new Vector3(_xIndex - offsetX, _yIndex - offsetY, fallingPebble.transform.position.z);
                        fallingPebble.MoveToTarget(targetPos);

                        // Update pebble indices
                        fallingPebble.SetIndicies(_xIndex, _yIndex);

                        // Update grid array
                        gameBoard[_xIndex, _yIndex] = new Node(true, fallingPebble.gameObject);
                        gameBoard[_xIndex, _aboveY] = new Node(true, null);
                        break; // Done with this slot
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