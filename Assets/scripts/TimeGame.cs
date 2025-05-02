using System.Collections;
using UnityEngine;
using TMPro;

public class TimeGame : MonoBehaviour
{
    public int countTime = 60;
    public TextMeshProUGUI countTimeText;

    private float timeRem;
    private ButtonClickUs gameManager;
    //простоая функция обновления времени
    private void Start()
    {
        timeRem = countTime;
        gameManager = FindObjectOfType<ButtonClickUs>();
    }

    void Update()
    {
        if (timeRem > 0)
        {
            timeRem -= Time.deltaTime;
            countTimeText.text = Mathf.Ceil(timeRem).ToString();
        }
        else
        {
            countTimeText.text = "0";
            OnTimeFinished();
        }
    }

    private void OnTimeFinished()
    {
        Debug.Log("Время вышло!");
        ResetGame();
    }

    private void ResetGame()
    {
        timeRem = countTime;
        countTimeText.text = countTime.ToString();
        if (gameManager != null)
        {
            gameManager.currentStreak = 0;
            gameManager.ResetGame();
        }
    }
}