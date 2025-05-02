using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollManager : MonoBehaviour
{
    public GameObject buttonPref;
    public Transform content;
    public Sprite[] sprites;
    public ButtonClickUs ButtonClickMen;
    public GridLayoutGroup GridLayoutGroup;

    private List<GameObject> buttons = new List<GameObject>();
    public void CreateButtons(string[] numbers)
    {
        ClearButtons();
        foreach (string number in numbers)
        {
            CreateButton(number);
        }
    }
    /// <summary>
    /// На замену, изменить постоянное создание заново, сохранять и по образу выбирать те что нужны
    /// Так будет быстрее
    /// сделать после изменения получения PNG, сейчас на клиенте, когда будут передоваться ++время
    /// Сохранять весь список и из него выбирать то что нужно ( доп функция )
    /// Сейчас функция одноразовой станет - везде кроме начала нужно поменять на новую
    /// или добавить проверку на новый список ( если не пустой ) - то заново не создавать а брать из него то что нужно
    /// заполнить два списка, выше проверка на не пустой доп список...
    /// </summary>

    private void CreateButton(string number)
    {
        GameObject newButton = Instantiate(buttonPref, content);
        newButton.name = number;

        Image buttonImage = newButton.GetComponent<Image>();
        Sprite matchedSprite = null;

        foreach (Sprite sprite in sprites)
        {
            if (sprite.name == number)
            {
                matchedSprite = sprite;
                break;
            }
        }

        if (matchedSprite != null)
        {
            buttonImage.sprite = matchedSprite;
        }
        else
        {
            Debug.LogWarning($"Не найдено соотв по имени {number}.");
        }
        buttons.Add(newButton);

        newButton.GetComponent<Button>().onClick.AddListener(() =>
            ButtonClickMen.OnButtonClicked(buttonImage.sprite, newButton.name));
    }
    private void ClearButtons()
    {
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();
    }
}
