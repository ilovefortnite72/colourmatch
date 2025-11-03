using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            
        }
        movesText = GameObject.Find("TXT_Moves").GetComponent<TMP_Text>();
        pointsText = GameObject.Find("TXT_Points").GetComponent<TMP_Text>();
        winPointsText = GameObject.Find("TXT_Goal").GetComponent<TMP_Text>();

        VicPanel = GameObject.Find("VictoryPanel");
        DefPanel = GameObject.Find("DefeatPanel");

        VicPanel.SetActive(false);
        DefPanel.SetActive(false);


    }

   

    private void Update()
    {
        pointsText.text = "Points: " + points.ToString();
        movesText.text = "Moves: " + moves.ToString();
        winPointsText.text = "Goal: " + winPoints.ToString();
    }

    public void Init(int _moves, int _goal)
    {
        moves = _moves;
        winPoints = _goal;
    }

    public void UpdatePoints(int _pointsGain)
    {
        points += _pointsGain;
    }

    public void ExecuteTurn(bool _subtractMove)
    {
        
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
        
        Debug.Log("You Win!");
    }
    public void LoseGame()
    {
        isGameEnded = true;
        DefPanel.SetActive(true);
        
        Debug.Log("you Lose!");
    }

}
