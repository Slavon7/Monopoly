using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellInfoPanel : MonoBehaviour
{
	[SerializeField] private GameController gameController;
	[SerializeField] private Button mislayButton;
	[SerializeField] private Button smallMislayButton;
	[SerializeField] private Button unMislayButton;
	[SerializeField] private Button upgradeButton;
	[SerializeField] private Button unUpgradeButton;
	[SerializeField] private Button closeButton;
	[SerializeField] private Image cellImage;
	[SerializeField] private TextMeshProUGUI infoText;
	[SerializeField] private GameObject infoCanvas;

	public int _cellIndex;

	private void Awake()
	{
		upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
		unUpgradeButton.onClick.AddListener(OnUnUpgradeButtonClicked);
		mislayButton.onClick.AddListener(OnMislayButtonClicked);
		smallMislayButton.onClick.AddListener(OnMislayButtonClicked);
		unMislayButton.onClick.AddListener(OnUnMislayButtonClicked);
		closeButton.onClick.AddListener(HideCanvas);
	}

	public void SetCurrentCell(int cellIndex)
	{
		_cellIndex = cellIndex;
		var cellData = gameController.Board.manager_board[cellIndex];
		int cellPrice = gameController.Board.GetCellPrice(cellIndex);
		int cellRentPrice = gameController.Board.GetCellTax(cellIndex);
		int cellMislayPrice = gameController.Board.GetCellMislay(cellIndex);
		int cellUnMislayPrice = gameController.Board.GetCellUnMislay(cellIndex);
		int cellLevel_1 = gameController.Board.GetCellLevel_1(cellIndex);
		int cellLevel_2 = gameController.Board.GetCellLevel_2(cellIndex);
		int cellLevel_3 = gameController.Board.GetCellLevel_3(cellIndex);
		int cellLevel_4 = gameController.Board.GetCellLevel_4(cellIndex);
		int cellLevel_5 = gameController.Board.GetCellLevel_5(cellIndex);
		infoText.text = $"<b>{cellData.cellName}</b>\n{cellData.description}\n\n" +
		                $"Будуйте філіал для того щоб збільшити аренду\n\n" +
						$"*              {cellLevel_1}$\n" +
						$"**             {cellLevel_2}$\n" +
						$"***            {cellLevel_3}$\n" +
						$"****           {cellLevel_4}$\n" +
						$"*****          {cellLevel_5}$\n" +
		                $"Ціна купівлі:  {cellPrice}$\n" +
		                $"Базова аренда: {cellRentPrice}$\n\n" +
						$"Ціна застави:  {cellMislayPrice}$\n" +
						$"Ціна за викуп: {cellUnMislayPrice}$\n";

		if(cellData.backgroundSprite != null)
			cellImage.sprite = cellData.backgroundSprite;  // Установить фоновое изображение из данных клетки
		else
			cellImage.sprite = null;  // Установите sprite в null, если у клетки нет фонового изображения

		// Перевіряем, чи є поточний гравець власником клітинки
		var cell = gameController.Board.manager_board[cellIndex];
		var cellOwner = gameController.Board.GetOwner(cellIndex);

		board.ECellArea cellAreaType = cellData.cellArea; // Получаем тип зоны клетки из данных клетки
    	bool canUpgrade = gameController.Board.CanUpgradeCell(cellAreaType, gameController.localPlayer); // Проверяем, можно ли улучшить клетку
		int currentLevel = gameController.Board.GetCurrentLevel(cell);

		if (cellOwner != null && cellOwner == gameController.localPlayer && !cell.IsMislayed)
		{
			mislayButton.gameObject.SetActive(currentLevel <= 0 && !canUpgrade); // показать кнопку "Заложить"
			smallMislayButton.gameObject.SetActive(canUpgrade && currentLevel == 0);
			unMislayButton.gameObject.SetActive(false);
			upgradeButton.gameObject.SetActive(!cellData.IsMislayed && canUpgrade && currentLevel < 5);
			unUpgradeButton.gameObject.SetActive(currentLevel > 0);
		}
		else if (cellOwner != null && cellOwner == gameController.localPlayer && cell.IsMislayed)
		{
			mislayButton.gameObject.SetActive(false); // скрыть кнопку "Заложить"
			smallMislayButton.gameObject.SetActive(false);
			unMislayButton.gameObject.SetActive(true);
			// upgradeButton.gameObject.SetActive(true);
		}
		else
		{
			mislayButton.gameObject.SetActive(false); // скрыть кнопку "Заложить"
			unMislayButton.gameObject.SetActive(false); // скрыть кнопку "Заложить"
			smallMislayButton.gameObject.SetActive(false);
			upgradeButton.gameObject.SetActive(false);
			unUpgradeButton.gameObject.SetActive(false);
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
		gameController.MislayCell(_cellIndex);

		HideCanvas(); // скрыть канвас после залога, если вы хотите это сделать сразу
	}
	
	private void OnUnMislayButtonClicked()
	{
		// заложите клетку
		// например, через BoardManager.OnMislayButtonClicked(CellIndex);
		// в зависимости от того, где вы реализуете логику залога
		gameController.unMislayCell(_cellIndex);

		HideCanvas(); // скрыть канвас после залога, если вы хотите это сделать сразу
	}

	private void OnUpgradeButtonClicked()
	{
		gameController.UpgradeCell(_cellIndex);
		HideCanvas();
	}

	private void OnUnUpgradeButtonClicked()
	{
		gameController.UnUpgradeCell(_cellIndex);
		HideCanvas();
	}
}