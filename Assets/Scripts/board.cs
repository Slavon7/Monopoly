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
	}
	
	[System.Serializable]
	public class data_board
	{
		public Transform cells;
		public SpriteRenderer cellRenderer;
		public Sprite backgroundSprite; // Фоновое изображение для этой клетки
		public TextMeshProUGUI priceLabel;
		public ECellType cellType;
		public int price;
		public int rentPrice;
		public string cellName;
    	public string description;
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
			}
		}
	}

	public void SetOwner(Player player, int cellIndex)
	{
		// Existing logic to set the owner and update cell color.
		var cell = manager_board[cellIndex];
		cell.cellRenderer.color = player.playerColor;
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

	public bool IsJailCell(int cellIndex){
		var cell = manager_board[cellIndex];
		return cell.cellType == ECellType.Jail;
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

	public Vector2 GetPosition(int cellIndex)
	{
		return manager_board[cellIndex].cells.position;
	}
}