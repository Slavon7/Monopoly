using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseClick : MonoBehaviour
{
    public board BoardManager; // ссылка на ваш менеджер доски
    public int CellIndex;      // индекс этой клетки
    public GameObject infoCanvas;      // Ссылка на ваш Canvas или панель.
    public TextMeshProUGUI infoText;   // Если используете TextMeshPro. Если используете обычный Text, замените на Text infoText.

    public RectTransform canvasRect;  // Rect Transform вашего Canvas или панели.

    public Image cellImage;  // Добавьте эту строку

    public Button mislayButton; // добавьте ссылку на вашу кнопку "Заложить"

    private Player currentPlayer; // ссылка на текущего игрока

    void Awake()
    {
        mislayButton.onClick.AddListener(OnMislayButtonClicked);
    }

    void OnMouseDown()
    {
        var cellData = BoardManager.manager_board[CellIndex];
        int cellPrice = BoardManager.GetCellPrice(CellIndex);
        int cellRentPrice = BoardManager.GetCellTax(CellIndex);
        infoText.text = $"<b>{cellData.cellName}</b>\n{cellData.description}\n\n" +
                        $"Будуйте філіал для того щоб збільшити аренду\n\n" +
                        $"Ціна купівлі: {cellPrice}$\n" +
                        $"Ціна аренди: {cellRentPrice}$\n";

        if(cellData.backgroundSprite != null)
            cellImage.sprite = cellData.backgroundSprite;  // Установить фоновое изображение из данных клетки
        else
            cellImage.sprite = null;  // Установите sprite в null, если у клетки нет фонового изображения

        // Перевіряем, чи є поточний гравець власником клітинки
        if (BoardManager.GetOwner(CellIndex) == currentPlayer)
        {
            mislayButton.gameObject.SetActive(false); // показать кнопку "Заложить"
            Debug.Log("Mislay");
        }
        else
        {
            mislayButton.gameObject.SetActive(true); // скрыть кнопку "Заложить"
            Debug.Log("UnMislay");
        }

        infoCanvas.SetActive(true);
    }

    public void HideCanvas()
    {
        infoCanvas.SetActive(false);
    }

    private void OnMislayButtonClicked()
    {
        // заложите клетку
        // например, через BoardManager.OnMislayButtonClicked(CellIndex);
        // в зависимости от того, где вы реализуете логику залога

        HideCanvas(); // скрыть канвас после залога, если вы хотите это сделать сразу
    }

    public void SetCurrentPlayer(Player player)
    {
        this.currentPlayer = player; // обновляйте эту переменную каждый раз, когда сменяется текущий игрок
    }
}
