using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
public class FirstUser : MonoBehaviour
{
    /// <summary>
    /// Тут регистрация временная, до момента пока на сервере не сделают более вменяемую ( рабочую регу )
    /// На деле большую часть нужно будет изменить
    /// Нужно будет сохранять только UID и его отправлять на сервер уже получая всё остальное
    /// </summary>
    private static string userId;
    private static string username = "basedUser";
    private int balance;
    private const string cacheKey = "User1";
    public string newUsername = "Default";
    public int updateskip = -50;
    public TextMeshProUGUI balanceText;
    public ButtonClickUs ButtonClick;
    public Button skipButton;
    private void Start()
    {
        if (PlayerPrefs.HasKey(cacheKey))
        {
            LoadUserData();
        }
        else
        {
            // Генерация рандомного имени пользователя ( временно )
            newUsername = GenerateRandomString(8);
            Debug.Log($"Сгенерированное имя пользователя: {newUsername}");
            // Генерация рандомного пароля ( временно )
            string hashedPassword = GenerateRandomString(6);
            Debug.Log($"Сгенерированный пароль: {hashedPassword}");
            StartCoroutine(RegisterUser(hashedPassword));
            balanceText.text = balance.ToString();
        }
        UpdateSkipButtonState();
    }
    private void Update()
    {
        UpdateSkipButtonState();
    }
    //кнопка
    public void Skipgame()
    {
        if (balance >= 50)
        {
            UpdateBalance(updateskip);
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
        {
            skipButton.interactable = balance >= 50;
        }
    }
    private void LoadUserData()
    {
        string cachedData = PlayerPrefs.GetString(cacheKey);
        UserData data = JsonUtility.FromJson<UserData>(cachedData);
        userId = data.id_user;
        username = data.username;
        balance = data.balance;
        balanceText.text = balance.ToString();

    }
    private void SaveUserData()
    {
        UserData data = new UserData
        {
            id_user = userId,
            username = username,
            balance = balance
        };

        string jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(cacheKey, jsonData);
        PlayerPrefs.Save();
    }

    public void UpdateBalance(int amount)
    {
        balance += amount;
        balanceText.text = balance.ToString();
        Debug.Log($"Баланс обновлен: {balance}");
        SaveUserData();
    }
    private IEnumerator RegisterUser(string hashedPassword)
    {
        balance = 100;
        string url = "https://backendforchina.onrender.com/users/testAdd/?username=" + newUsername + "&hashed_password=" + hashedPassword + $"&balance={balance}";
        Debug.Log($"{url}");
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            UserData data = JsonUtility.FromJson<UserData>(response);
            userId = data.id_user;
            username = data.username;
            balance = data.balance;

            PlayerPrefs.SetString(cacheKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError($"Код: {request.error}");
        }
    }
    //временно
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

    [System.Serializable]
    public class UserData
    {
        public string id_user;
        public string username;
        public int balance;
    }
}