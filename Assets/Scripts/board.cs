using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class board : MonoBehaviour
{
	public enum ECellType
	{
		Start,
		Purchasable,
		DeductMoney,
		Jail,
		Donut,
		Casino,
	}
	
	public enum ECellArea
	{
		Shops,
		Banks,
		Clothes,
		Apps,
		ITCompany,
		Confectionery,
		Restaurant,
		Airlines,
		Logistics,
		Manufacturers,
		None,
	}
	
	[System.Serializable]
	public class data_board
	{
		public Transform cells;
		public SpriteRenderer cellRenderer;
		public Sprite backgroundSprite; // Фоновое изображение для этой клетки
		public SpriteRenderer imageMislayed; // Картинка для отображения заложения клетки
		public TextMeshProUGUI priceLabel;
		public ECellType cellType;
		public ECellArea cellArea;
		public int price;
		public int rentPrice;
		public int level { get; set; }
		public int baseRent;
		public int level_1;
		public int level_2;
		public int level_3;
		public int level_4;
		public int level_5;
		public int upgradePrice;
		public int mislayPrice;
		public int unMislayPrice;
		public string cellName;
    	public string description;
		public bool IsMislayed { get; set; }
	}

	public List<Player> cellOwners = new List<Player>();

	public List<data_board> manager_board = new List<data_board>();

	void Start()
	{
		for (int i = 0; i < manager_board.Count; i++)
		{
			cellOwners.Add(null);
			manager_board[i].cellRenderer = manager_board[i].cells.gameObject.GetComponent<SpriteRenderer>();

			// If the cell is purchasable and has a price label, update its text based on ownership.
			if (manager_board[i].cellType == ECellType.Purchasable && manager_board[i].priceLabel != null)
			{
				// If the cell is already owned, display the rent price. Otherwise, display the purchase price.
				if (cellOwners[i] != null)
					manager_board[i].priceLabel.text = manager_board[i].rentPrice.ToString() + "$";
				else
					manager_board[i].priceLabel.text = manager_board[i].price.ToString() + "$";
			}
		}
	}

	public void ResetOwnedCellsByPlayer(Player player)
	{
		// Перебор всех клеток доски
		for (int i = 0; i < manager_board.Count; i++)
		{
			if (GetOwner(i) == player)
			{
				// Если текущий игрок является владельцем этой клетки, сбросить владельца
				SetOwner(null, i);
				manager_board[i].cellRenderer.color = Color.white;
			}
		}
	}

	public void SetOwner(Player player, int cellIndex)
	{
		// Existing logic to set the owner and update cell color

		var cell = manager_board[cellIndex];
		cell.cellRenderer.color = player == null ? Color.white : player.playerColor;
		cellOwners[cellIndex] = player;

		// If the cell is purchasable and has a price label, update its text.
		if (cell.cellType == ECellType.Purchasable && cell.priceLabel != null)
		{
			// If the player is not null (i.e., the cell has been purchased), display the rent price.
			// Otherwise, display the purchase price.
			if (player != null)
				cell.priceLabel.text = cell.rentPrice.ToString() + "$";
			else
				cell.priceLabel.text = cell.price.ToString() + "$";
		}
	}

	public void SetMislayed(int cellIndex, bool state)
	{
		var cell = manager_board[cellIndex];
		cell.imageMislayed.enabled = state;
		cell.IsMislayed = state;
	}

	public Player GetOwner(int cellIndex)
	{
		return cellOwners[cellIndex];
	}

	public string GetCellName(int index)
	{
		return manager_board[index].cellName;
	}

	public bool CanBuyCell(int cellIndex)
	{
		var cell = manager_board[cellIndex];
		return (cell.cellType == ECellType.Purchasable && cellOwners[cellIndex] == null);
	}

	public bool CanUpgradeCell(ECellArea cellAreaType, Player player)
	{
		// Перебираем все клетки на доске
		for (int i = 0; i < manager_board.Count; i++)
		{
			// Если клетка относится к нужному типу...
			if (manager_board[i].cellArea == cellAreaType)
			{
				// ...проверяем, принадлежит ли она игроку. Если нет, возвращаем false.
				if (cellOwners[i] != player)
				{
					return false;
				}
			}
		}
		// Если все клетки указанного типа принадлежат игроку, возвращаем true.
		return true;
	}

	public bool UpgradeCell(int cellIndex, Player player)
	{
		if (cellIndex >= 0 && cellIndex < manager_board.Count)
		{
			// Check if the cell can be upgraded
			if (CanUpgradeCell(manager_board[cellIndex].cellArea, player))
			{
				// Get the current level of the cell
				var cellData = manager_board[cellIndex];
				int currentLevel = GetCurrentLevel(cellData);

				// Determine the new level and update the price label accordingly
				switch (currentLevel)
				{
					case 0:
						cellData.rentPrice = cellData.level_1; // Set rent to level 1
						cellData.priceLabel.text = cellData.level_1.ToString() + "$"; // Prepare label for next level
						break;
					case 1:
						cellData.rentPrice = cellData.level_2; // Set rent to level 2
						cellData.priceLabel.text = cellData.level_2.ToString() + "$"; // Prepare label for next level
						break;
					case 2:
						cellData.rentPrice = cellData.level_3;
						cellData.priceLabel.text = cellData.level_3.ToString() + "$"; 
						break;
					case 3:
						cellData.rentPrice = cellData.level_4;
						cellData.priceLabel.text = cellData.level_4.ToString() + "$"; 
						break;
					case 4:
						cellData.rentPrice = cellData.level_5;
						cellData.priceLabel.text = cellData.level_5.ToString() + "$";
						break;
					default:
						return false; // No more upgrades available
				}
				return true; // Upgrade was successful
			}
		}
		return false; // Upgrade was not possible
	}

	public bool DowngradeCell(int cellIndex, Player player)
	{
		if (cellIndex >= 0 && cellIndex < manager_board.Count)
		{
			var cellData = manager_board[cellIndex];
			
			// Проверяем, принадлежит ли клетка игроку и не находится ли она на минимальном уровне
			if (cellOwners[cellIndex] == player && GetCurrentLevel(cellData) > 0)
			{
				int currentLevel = GetCurrentLevel(cellData);

				// Определяем новый уровень и обновляем ценник соответствующе
				switch (currentLevel)
				{
					case 1:
						cellData.rentPrice = cellData.baseRent; // Устанавливаем аренду на базовый уровень
						cellData.priceLabel.text = cellData.baseRent.ToString() + "$"; // Подготавливаем ценник для предыдущего уровня
						break;
					case 2:
						cellData.rentPrice = cellData.level_1; // Устанавливаем аренду на уровень 1
						cellData.priceLabel.text = cellData.level_1.ToString() + "$";
						break;
					case 3:
						cellData.rentPrice = cellData.level_2; // Устанавливаем аренду на уровень 2
						cellData.priceLabel.text = cellData.level_2.ToString() + "$";
						break;
					case 4:
						cellData.rentPrice = cellData.level_3; // Устанавливаем аренду на уровень 3
						cellData.priceLabel.text = cellData.level_3.ToString() + "$";
						break;
					case 5:
						cellData.rentPrice = cellData.level_4; // Устанавливаем аренду на уровень 4
						cellData.priceLabel.text = cellData.level_4.ToString() + "$";
						break;
					// Добавьте дополнительные случаи для более высоких уровней при необходимости
					default:
						return false; // Больше нет возможности для понижения
				}
				
				// Выполните дополнительную логику для понижения уровня клетки, если необходимо
				// ...

				return true; // Понижение уровня было успешным
			}
		}
		return false; // Понижение уровня не было возможно
	}

	public int GetCurrentLevel(data_board cellData)
	{
		// Check the current rent price against all levels to determine the cell's level
		if (cellData.rentPrice >= cellData.level_5)
		{
			return 5;
		}
		else if (cellData.rentPrice >= cellData.level_4)
		{
			return 4;
		}
		else if (cellData.rentPrice >= cellData.level_3)
		{
			return 3;
		}
		else if (cellData.rentPrice >= cellData.level_2)
		{
			return 2;
		}
		else if (cellData.rentPrice >= cellData.level_1)
		{
			return 1;
		}
		// If the rent price is less than level 1 or doesn't match any level, it is level 0
		return 0;
	}

	public int GetCellLevel(int cellIndex)
	{
		// Проверяем, что индекс клетки находится в пределах списка
		if (cellIndex >= 0 && cellIndex < manager_board.Count)
		{
			var cellData = manager_board[cellIndex];
			// Возвращаем текущий уровень клетки
			return GetCurrentLevel(cellData);
		}
		else
		{
			// Если индекс вне диапазона, возвращаем -1 или выбрасываем исключение
			Debug.LogError("GetCellLevel: Index out of range");
			return -1; // Индикатор того, что индекс клетки неверен
		}
	}

	public bool IsJailCell(int cellIndex){
		var cell = manager_board[cellIndex];
		return cell.cellType == ECellType.Jail;
	}

	public bool IsDonutCell(int cellIndex){
		var cell = manager_board[cellIndex];
		return cell.cellType == ECellType.Donut;
	}


	public bool isStartCell(int cellIndex){
		var cell = manager_board[cellIndex];
		return cell.cellType == ECellType.Start;
	}

	public bool IsDeductMoneyCell(int cellIndex)
	{
		var cell = manager_board[cellIndex];
		return cell.cellType == ECellType.DeductMoney;
	}

	public int GetCellPrice(int cellIndex)
	{
		return manager_board[cellIndex].price;
	}

	public int GetCellTax(int cellIndex)
	{
		//temporary
		return manager_board[cellIndex].rentPrice;
	}

	public int GetCellMislay(int cellIndex)
	{
		//temporary
		return manager_board[cellIndex].mislayPrice;
	}
	
	public int GetCellUnMislay(int cellIndex)
	{
		//temporary
		return manager_board[cellIndex].unMislayPrice;
	}

	public int GetCellUpgrade(int cellIndex)
	{
		//temporary
		return manager_board[cellIndex].upgradePrice;
	}

	public int GetCellLevel_1(int cellIndex)
	{
		return manager_board[cellIndex].level_1;
	}

	public int GetCellLevel_2(int cellIndex)
	{
		return manager_board[cellIndex].level_2;
	}

	public int GetCellLevel_3(int cellIndex)
	{
		return manager_board[cellIndex].level_3;
	}

	public int GetCellLevel_4(int cellIndex)
	{
		return manager_board[cellIndex].level_4;
	}

	public int GetCellLevel_5(int cellIndex)
	{
		return manager_board[cellIndex].level_5;
	}
	

	public ECellArea GetCellAreaType(int cellIndex)
	{
		// Проверяем, что индекс клетки находится в пределах списка
		if (cellIndex >= 0 && cellIndex < manager_board.Count)
		{
			// Возвращаем тип области для данной клетки
			return manager_board[cellIndex].cellArea;
		}
		else
		{
			// Если индекс вне диапазона, возвращаем None или выбрасываем исключение
			Debug.LogError("GetCellAreaType: Index out of range");
			return ECellArea.None;
		}
	}

	public Vector2 GetPosition(int cellIndex)
	{
		return manager_board[cellIndex].cells.position;
	}
}