using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject backPanel;
    public GameObject VicPanel;
    public GameObject DefPanel;

    public int winPoints;
    public int moves;
    public int points;

    public bool isGameEnded;


    public TMP_Text movesText;
    public TMP_Text pointsText;
    public TMP_Text winPointsText;

    private void Awake()
    {
        Instance = this;

    }


    private void Update()
    {
        pointsText.text = "Points: " + points.ToString();
        movesText.text = "Moves Left: " + moves.ToString();
        winPointsText.text = "Goal: " + winPoints.ToString();
    }

    public void Init(int _moves, int _goal)
    {
        moves = _moves;
        winPoints = _goal;
    }

    public void ExecuteTurn(int _pointGain, bool _subtractMove)
    {
        points += _pointGain;
        if(_subtractMove)
        {
            moves--;
        }

        if(points >= winPoints)
        {
            
            WinGame();
            return;
        }
        else if(moves <= 0)
        {
            
            LoseGame();
            return;
        }

    }




    public void WinGame()
    {
        isGameEnded = true;
        VicPanel.SetActive(true);
        backPanel.SetActive(true);
        Debug.Log("You Win!");
    }
    public void LoseGame()
    {
        isGameEnded = true;
        DefPanel.SetActive(true);
        backPanel.SetActive(true);
        Debug.Log("you Lose!");
    }

}
