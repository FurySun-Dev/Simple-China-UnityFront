using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    private const string baseUrl = "http://127.0.0.1:8000";

    [Header("Panels")]
    public TextMeshProUGUI nick;
    public GameObject loginPanel;       // Панель логина
    public GameObject registerPanel;    // Панель регистрации
    public GameObject loginButtonMane;  // Кнопка "Login" в меню
    public GameObject registerButtonMane; // Кнопка "Register" в меню

    [Header("Login Fields")]
    public TMP_InputField loginUsernameField;
    public TMP_InputField loginPasswordField;

    [Header("Register Fields")]
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;
    public TMP_InputField registerRepeatField;

    [Header("Error Messages")]
    public GameObject errorInputText;    // "Мин. 3 символа, не только цифры"
    public GameObject errorPasswordText; // "Пароли не совпадают"

    void Start()
    {
        if (PlayerPrefs.HasKey("isLoggedIn"))
        {
            if(PlayerPrefs.GetInt("isLoggedIn") == 1)
            {
                nick.text = PlayerPrefs.GetString("username");
            }
        }
    }

    // === UI Handlers ===
    public void ShowLoginPanel()
    {
        errorInputText.SetActive(false);
        errorPasswordText.SetActive(false);

        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        registerButtonMane.SetActive(false);
    }

    public void ShowRegisterPanel()
    {
        errorInputText.SetActive(false);
        errorPasswordText.SetActive(false);

        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
        loginButtonMane.SetActive(false);
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    // Вызывается при нажатии на кнопку внутри панели логина
    public void OnLoginSubmit()
    {
        errorInputText.SetActive(false);
        string user = loginUsernameField.text.Trim();
        string pass = loginPasswordField.text;

        if (!IsValid(user) || !IsValid(pass))
        {
            errorInputText.SetActive(true);
            return;
        }

        // Запускаем корутину логина
        StartCoroutine(Login(user, pass));
    }

    // Вызывается при нажатии на кнопку внутри панели регистрации
    public void OnRegisterSubmit()
    {
        errorInputText.SetActive(false);
        errorPasswordText.SetActive(false);

        string user = registerUsernameField.text.Trim();
        string pass = registerPasswordField.text;
        string pass2 = registerRepeatField.text;

        if (!IsValid(user) || !IsValid(pass))
        {
            errorInputText.SetActive(true);
            return;
        }
        if (pass != pass2)
        {
            errorPasswordText.SetActive(true);
            return;
        }

        // Запускаем корутину регистрации
        StartCoroutine(Register(user, pass));
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    // === Coroutines for network ===
    IEnumerator Login(string username, string password)
    {
        string url = $"{baseUrl}/users/login?username={UnityWebRequest.EscapeURL(username)}&password={UnityWebRequest.EscapeURL(password)}";
        using (UnityWebRequest www = UnityWebRequest.Post(url, new WWWForm()))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                PlayerPrefs.SetInt("isLoggedIn", 1);
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save();
                Debug.Log("Login successful!");
                nick.text = username;
                // Здесь можно загрузить следующий уровень или обновить UI
            }
            else
            {
                Debug.LogError("Login error: " + www.error);
            }
        }
    }

    IEnumerator Register(string username, string password)
    {
        string uuid = PlayerPrefs.GetString("uuid");
        string url = $"{baseUrl}/users/register?uuid={uuid}&username={UnityWebRequest.EscapeURL(username)}&password={UnityWebRequest.EscapeURL(password)}";
        using (UnityWebRequest www = UnityWebRequest.Post(url, new WWWForm()))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Registration successful!");
                // Возможно, скрыть панель и показать главное меню
            }
            else
            {
                Debug.LogError("Registration error: " + www.error);
            }
        }
    }

    // === Helpers ===
    private bool IsValid(string s)
    {
        if (s.Length < 3) return false;
        foreach (char c in s)
            if (!char.IsDigit(c))
                return true;
        return false;
    }
}

