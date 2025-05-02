using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PrintHierogliph : MonoBehaviour
{
    public TextMeshProUGUI tmpText;

    //вывод иероглифа
    public void SetUnicode(string unicode)
    {
        try
        {
            //мб проше парсить можно, но....
            unicode = unicode.Trim('"');
            int unicodeInt = int.Parse(unicode, System.Globalization.NumberStyles.HexNumber);
            char character = (char)unicodeInt;
            tmpText.text = character.ToString();
        }
        catch (FormatException ex)
        {
            Debug.LogError($"Не верный формат юникода: '{unicode}'. Код: {ex.Message}");
            tmpText.text = "Error";
        }
        catch (OverflowException ex)
        {
            Debug.LogError($"Unicode out of range: '{unicode}'. Код: {ex.Message}");
            tmpText.text = "Out of range";
        }
    }
}
