using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public GameObject mainGame;
    public void Start()
    {
        StartCoroutine(StartGameWithDelay(3));
    }

    private IEnumerator StartGameWithDelay(int delaySeconds)
    {
        loadingScreen.SetActive(true);
        mainGame.SetActive(false);

        yield return new WaitForSeconds(delaySeconds);

        loadingScreen.SetActive(false);
        mainGame.SetActive(true);
    }
}