using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class Translate : MonoBehaviour
{
    [Header("References")]
    public ButtonClickUs buttonClick;
    public PrintHierogliph hierogliph;
    public TextMeshProUGUI textTranslate;
    public GameObject falsePanel;
    public List<string> sequence = new List<string>();
    [Header("Settings")]
    private const string baseUrl = "http://127.0.0.1:8000";

    [Serializable]
    private class StringListWrapper
    {
        public List<string> graphemes;
    }

    [Serializable]
    private class TranslateRequest
    {
        public string text;
    }

    [Serializable]
    private class TranslateToken
    {
        public string token;
        public string pinyin;
        public List<string> meanings;
    }

    [Serializable]
    private class TranslateResponseArray
    {
        public List<TranslateToken> tokens;
    }

    // Вызывается по кнопке
    public void TranslateButton()
    {
        // Берём последовательность из внешнего компонента
        sequence = buttonClick.sequence;
        //if (sequence == null || sequence.Count == 0)
        //{
        //    Debug.LogWarning("Sequence is empty, nothing to translate.");
        //    return;
        //}
        StartCoroutine(TranslateCoroutine(sequence));
    }

    private IEnumerator TranslateCoroutine(List<string> sequence)
    {
        var wrapper = new StringListWrapper { graphemes = sequence };
        string json1 = JsonUtility.ToJson(wrapper);
        byte[] body1 = Encoding.UTF8.GetBytes(json1);

        string url1 = $"{baseUrl}/hieroglyphs/find_hieroglyph";
        using (var req1 = new UnityWebRequest(url1, "POST"))
        {
            req1.uploadHandler = new UploadHandlerRaw(body1);
            req1.downloadHandler = new DownloadHandlerBuffer();
            req1.SetRequestHeader("Content-Type", "application/json");
            req1.SetRequestHeader("Accept", "application/json");

            yield return req1.SendWebRequest();
            if (req1.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"find_hieroglyph error {req1.responseCode}: {req1.error}");
                yield break;
            }
            string rawHex = req1.downloadHandler.text.Trim();
            Debug.Log("find_hieroglyph returned: " + rawHex);

            if (req1.downloadHandler.text == "Neverno")
            {
                falsePanel.SetActive(true);
                yield break;
            }

            hierogliph.SetUnicode(rawHex);

            var reqBody2 = new TranslateRequest { text = rawHex };
            string json2 = JsonUtility.ToJson(reqBody2);
            byte[] body2 = Encoding.UTF8.GetBytes(json2);
            string url2 = $"{baseUrl}/translation/translate/";

            using (var req2 = new UnityWebRequest(url2, "POST"))
            {
                req2.uploadHandler = new UploadHandlerRaw(body2);
                req2.downloadHandler = new DownloadHandlerBuffer();
                req2.SetRequestHeader("Content-Type", "application/json");
                req2.SetRequestHeader("Accept", "application/json");

                yield return req2.SendWebRequest();
                if (req2.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"translate error {req2.responseCode}: {req2.error}\n{req2.downloadHandler.text}");
                    yield break;
                }

                string respJson = req2.downloadHandler.text;
                List<TranslateToken> tokens = new List<TranslateToken>();
                textTranslate.text = "";
                if (respJson.Contains("\"tokens\""))
                {
                    var arr = JsonUtility.FromJson<TranslateResponseArray>(respJson);
                    if (arr.tokens != null) tokens = arr.tokens;
                }
                else
                {
                    var single = JsonUtility.FromJson<TranslateToken>(respJson);
                    if (single != null) tokens.Add(single);
                }
                var sb = new StringBuilder();
                foreach (var t in tokens)
                {
                    if (string.IsNullOrEmpty(t.token) || t.meanings == null || t.meanings.Count == 0)
                        continue;
                    sb.AppendLine($"Pinyin: {t.pinyin}");
                    sb.AppendLine("Meanings:");
                    foreach (var m in t.meanings)
                        sb.AppendLine($"  • {m}");
                    sb.AppendLine();
                }

                textTranslate.text = sb.ToString().TrimEnd();
            }
        }
    }
}