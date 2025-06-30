using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public GameObject mainGame;

    private static bool logoAlreadyShown = false;

    private void Start()
    {
        if (!logoAlreadyShown)
        {
            logoAlreadyShown = true;
            StartCoroutine(StartGameWithDelay(3));
        }
        else
        {
            loadingScreen.SetActive(false);
            mainGame.SetActive(true);
        }
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