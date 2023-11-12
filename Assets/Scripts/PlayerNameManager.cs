using UnityEngine;
using UnityEngine.UI; // Необходимо для работы с UI элементами, такими как InputField

public class PlayerNameManager : MonoBehaviour
{
    // Метод для сохранения имени игрока
    public void SavePlayerName(InputField inputField)
    {
        // Проверяем, не пустое ли поле ввода
        if (!string.IsNullOrWhiteSpace(inputField.text))
        {
            // Сохраняем имя игрока в PlayerPrefs под ключом "PlayerName"
            PlayerPrefs.SetString("PlayerName", inputField.text);

            // Важно вызвать PlayerPrefs.Save(), чтобы гарантировать сохранение изменений
            PlayerPrefs.Save();

            // Отладочное сообщение для проверки того, что имя было сохранено
            Debug.Log("Player name saved: " + inputField.text);
        }
        else
        {
            Debug.LogError("Player name is empty or whitespace!");
        }
    }
}
