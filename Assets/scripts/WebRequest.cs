using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequest : MonoBehaviour
{
    public PrintHierogliph printHierogliph;
    public ScrollManager scrollManager;
    public ButtonClickUs ButtonClickUs;

    private string currentHieroglyph;

    private Coroutine currentHieroglyphCoroutine;
    private Coroutine currentSymbolsCoroutine;
    private Coroutine currentValidationCoroutine;
    private Coroutine currentNameCoroutine;

    public void Awake()
    {
        GetHieroglyphFi();
        GetSymbolsListAppend();
    }
    public void GetHieroglyphFi()
    {
        if (currentHieroglyphCoroutine != null)
            StopCoroutine(currentHieroglyphCoroutine);

        currentHieroglyphCoroutine = StartCoroutine(GetHieroglyph());
    }

    public void GetSymbolsListAppend()
    {
        if (currentSymbolsCoroutine != null)
            StopCoroutine(currentSymbolsCoroutine);

        currentSymbolsCoroutine = StartCoroutine(GetSymbolsList());
    }
    public void Validate(List<string> sequence)
    {
        if (currentValidationCoroutine != null)
            StopCoroutine(currentValidationCoroutine);

        currentValidationCoroutine = StartCoroutine(ValidateSequence(sequence, currentHieroglyph));
    }

    public void SendNameParth(List<string> name)
    {
        if (currentNameCoroutine != null)
            StopCoroutine(currentNameCoroutine);

        currentNameCoroutine = StartCoroutine(SendName(name));
    }
    private IEnumerator GetHieroglyph()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://backendforchina.onrender.com/hieroglyphs/random_hieroglyph");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            currentHieroglyph = request.downloadHandler.text.Trim();
            Debug.Log("Иероглиф получен: " + currentHieroglyph);
            printHierogliph.SetUnicode(currentHieroglyph);
        }
        else
        {
            Debug.LogError($"Ошибка получения иероглифа: {request.error}, Код: {request.responseCode}");
        }
    }
    private IEnumerator GetSymbolsList()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://backendforchina.onrender.com/graphems/all_graphems");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;

            try
            {
                string[] stringArray = JsonHelper.FromJson<string>(jsonResponse);
                scrollManager.CreateButtons(stringArray);

                Debug.Log("Список чисел получен: " + string.Join(", ", stringArray));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка парсинга JSON: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Ошибка получения списка символов: {request.error}, Код: {request.responseCode}");
        }
    }
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string wrappedJson = "{\"Items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.Items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
    public IEnumerator SendName(List<string> name)
    {
        //To json, mb replay on json library
        string jsonData = JsonUtility.ToJson(new StringListWrapper { graphemes = name });

        UnityWebRequest request = new UnityWebRequest("https://backendforchina.onrender.com/hieroglyphs/get_available_graphemes", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Список отправлен успешно! Ответ сервера: {request.downloadHandler.text}");
            string jsonResponse = request.downloadHandler.text;
            Debug.Log($"{jsonResponse}");
            try
            {
                GraphemeResponse response = JsonUtility.FromJson<GraphemeResponse>(jsonResponse);
                List<string> availableGraphemes = response.available_graphemes;

                Debug.Log($"Подходящие графемы: {string.Join(", ", availableGraphemes)}");
                string[] numbers = availableGraphemes.ToArray();
                scrollManager.CreateButtons(numbers);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка парсинга ответа: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Ошибка получения/отправки списка символов: {request.error}, Код: {request.responseCode}");
        }
    }
    public class GraphemeResponse
    {
        public List<string> available_graphemes;
    }
    [System.Serializable]
    public class StringListWrapper
    {
        public List<string> graphemes;
    }

    public IEnumerator ValidateSequence(List<string> sequence, string hieroglyph)
    {
        string jsonData = JsonUtility.ToJson(new StringListWrapper { graphemes = sequence });
        string url = "https://backendforchina.onrender.com/hieroglyphs/confirm?hieroglyph=" + hieroglyph;
        url = url.Replace("\"", "");

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            try
            {
                ValidationResponse validationResponse = JsonUtility.FromJson<ValidationResponse>(response);

                bool isCorrect = validationResponse.confirm;
                if (isCorrect)
                {
                    ButtonClickUs.TrueValid();
                    StartCoroutine(GetHieroglyph());
                    StartCoroutine(GetSymbolsList());
                }
                else { ButtonClickUs.FalseValid(); }
                Debug.Log(isCorrect ? "Последовательность верна!" : "Последовательность неверна!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка парсинга ответа проверки: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Ошибка проверки последовательности: {request.error}, Код: {request.responseCode}");
        }
    }
    [System.Serializable]
    public class ValidationResponse
    {
        public bool confirm;
    }
}
