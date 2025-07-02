using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class FirstUser : MonoBehaviour
{
    public TextMeshProUGUI balanceText;
    public Button skipButton;
    public ButtonClickUs ButtonClick;

    private int balance;
    public string uuid;
    public string timeID;
    private const string baseUrl = "http://127.0.0.1:8000";
    private const int skipCost = 50;

    private void Start()
    {
        
        if (PlayerPrefs.HasKey("uuid"))
        {
            uuid = PlayerPrefs.GetString("uuid");
            StartCoroutine(GetBalance());
        }
        else
        {
            timeID = GenerateRandomString(5);
            StartCoroutine(InitialLogin());
            
        }

    }
    private string GenerateRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        char[] stringChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
    private void Update()
    {
        UpdateSkipButtonState();
    }

    public void Skipgame()
    {
        if (balance >= skipCost)
        {
            UpdateBalance(-skipCost);
            ButtonClick.ResetGame();
        }
        else
        {
            Debug.LogWarning("Недостаточно очков для пропуска!");
        }
    }

    private void UpdateSkipButtonState()
    {
        if (skipButton != null)
            skipButton.interactable = balance >= skipCost;
    }

    [System.Serializable]
    public class InitialLoginResponse
    {
        public string uuid;
    }
    private IEnumerator InitialLogin()
    {
        string url = $"{baseUrl}/users/initial_login?device_id={timeID}";

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, string.Empty))
        {
            www.SetRequestHeader("Accept", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                InitialLoginResponse resp = JsonUtility.FromJson<InitialLoginResponse>(json);

                uuid = resp.uuid;
                PlayerPrefs.SetString("uuid", uuid);
                PlayerPrefs.Save();

                Debug.Log($"Initial login successful, uuid = {uuid}");

                StartCoroutine(GetBalance());
            }
            else
            {
                Debug.LogError($"InitialLogin error: {www.error}");
            }
        }
    }
    [Serializable]
    public class BalanceResponse
    {
        public int balance;
    }
    private IEnumerator GetBalance()
    {
        string url = $"{baseUrl}/users/get_balance?uuid={uuid}";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                Debug.Log("Баланс: " + json);

                BalanceResponse resp = JsonUtility.FromJson<BalanceResponse>(json);
                balance = resp.balance;

                PlayerPrefs.SetInt("balance", balance);
                balanceText.text = balance.ToString();
                Debug.Log("Баланс получен: " + balance);
            }
            else
            {
                Debug.LogError("Ошибка получения баланса: " + www.error);
            }
        }
    }

    public void UpdateBalance(int amount)
    {
        balance += amount;
        PlayerPrefs.SetInt("balance", balance);
        balanceText.text = balance.ToString();
        Debug.Log($"Баланс обновлен: {balance}");
        StartCoroutine(UpdateBalanceOnServer());
    }

    private IEnumerator UpdateBalanceOnServer()
    {
        string url = $"{baseUrl}/users/update_balance?uuid={uuid}&new_balance={balance}";
        using (UnityWebRequest www = UnityWebRequest.Post(url, new WWWForm()))
        {
            www.SetRequestHeader("Accept", "application/json");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Баланс обновлён");
            }
            else
            {
                Debug.LogError($"Ошибка обновления баланса {www.responseCode}: {www.error}\n{www.downloadHandler.text}");
            }
        }
    }
}
