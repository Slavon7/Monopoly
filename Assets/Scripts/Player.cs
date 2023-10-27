using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
	[SyncVar] public int cellBoardIndex = 0;
	[SyncVar] public bool dicesRolled;

	[SyncVar] public Color playerColor;

	[SyncVar(hook = nameof(OnMoneyChanged))]
	public int money = 15000;

	public bool isGameOver = false;

	public Sprite[] diceSprites;  // drag your 6 dice sprites here in the inspector, from 1 to 6

	private GameController gameController;

	private void OnMoneyChanged(int oldMoney, int newMoney)
	{
		gameController.UpdatePlayersMoney();

		if (newMoney < 0)
		{
			Debug.Log($"Player: {name} has lost with negative money!");
			// Optionally, you could implement further logic here to handle the player losing, 
			// such as removing them from the game or displaying a message to other players.
		}
	}
	
	private void Awake()
	{
		gameController = FindObjectOfType<GameController>();
	}

	private void Start()
	{
		// Добавьте текущего игрока в список всех игроков
		gameController.AddPlayer(this);
	
		gameController.UpdatePlayersMoney();

		// Если сервер, то определите текущего игрока
		if (isServer)
		{
			gameController.SetCurrentPlayer(this);
			gameController.SERVER_AssignPlayerColor(this);
		}
	}

	private void OnDestroy()
	{
		if (gameController != null)
		{
			gameController.RemovePlayer(this);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		//called on client and host
		gameController.PlayerJoin(netId, this);
		if (isLocalPlayer)
		{
			gameController.AssignLocalPlayer(this);
		}
	}

	public override void OnStopClient()
	{
		gameController.PlayerLeft(netId);
		base.OnStopClient();
	}

	private void Update()
	{
		if (!isLocalPlayer || isGameOver) 
			return;
		
		// if (Input.GetKeyDown(KeyCode.B))
		// {
		// 	CmdAttemptToBuyCell();
		// }

		if (Input.GetKeyDown(KeyCode.R))
		{
			CmdRollDices();
		}
			
		// if (Input.GetKeyDown(KeyCode.Space))
		// {
		// 	CmdEndTurn();
		// }
	}
	
	[Command]
	public void CmdAttemptToBuyCell()
	{
		Debug.Log($"Player: {name} Попытка купить клетку {cellBoardIndex}");
		if (gameController.Board.CanBuyCell(cellBoardIndex))
		{
			Debug.Log($"Клетка {cellBoardIndex} доступна для покупки");
			// Получаем цену клетки из доски
			int cellPrice = gameController.Board.GetCellPrice(cellBoardIndex);
			if (money >= cellPrice)
			{
				gameController.Board.SetOwner(this, cellBoardIndex);
				money -= cellPrice;
				RpcSetCellOwner(this.netId, cellBoardIndex);
				SwitchTurn();
				HideRollForPlayerCanvas(connectionToClient);
			}
			else
			{
				Debug.Log($"Player: {name} Недостатньо грошей для купівлі cell {cellBoardIndex}");
				RpcShowRollCanvas();
			}
		}
		else
		{
			Debug.Log($"Клетка {cellBoardIndex} не доступна для покупки");
		}
	}
	
	[Command]
	public void CmdRollDices()
	{
		if (GameController.currentPlayer != this)
		{
			Debug.Log($"{name} you are not allowed to roll the dice now. it is {GameController.currentPlayer.name}'s turn");
			return;
		}

		if (dicesRolled)
		{
			Debug.Log($"{name} Player already rolled dices!");
			return;
		}

		int random1 = Random.Range(1, 7);
		int random2 = Random.Range(1, 7);

		gameController.SetDiceSprites(diceSprites[random1 - 1], diceSprites[random2 - 1]);
		RpcShowRollDiceCanvas();

		int sum = random1 + random2;

		// Заменяем board_new на сумму случайных чисел
		cellBoardIndex += sum;

		// Проверка на выход за пределы 24 клеток
		if (cellBoardIndex >= 40)
		{
			// Возвращаем игрока на второй круг
			cellBoardIndex -= 40;
			// Add money for one round
			money += 1000;
			Debug.Log("Бонус за коло");
			//CmdIncreaseMoney(500);
		}

		dicesRolled = true;
		var targetPosition = gameController.Board.GetPosition(cellBoardIndex);
		transform.position = targetPosition;
		
		RpcUpdatePosition(targetPosition);
		RpcTollResult(random1, random2);

		var cellName = gameController.Board.GetCellName(cellBoardIndex);
		RpcShowCellName(cellName);

		
		//Check cell tax
		
		var cellOwner = gameController.Board.GetOwner(cellBoardIndex);
		if (cellOwner == null && gameController.Board.CanBuyCell(cellBoardIndex))
		{
			RpcShowRollCanvas(); // Show the buy canvas to the player
		}
		else if (cellOwner != null)
		{
			if (cellOwner != this)
			{
				RpcShowTaxesCell();
			}
			else
			{
				SwitchTurn();
			}
		}
		//Check cell actions

		if (gameController.Board.IsDeductMoneyCell(cellBoardIndex))
		{
			// Генерируем случайное число для определения, что произойдет с деньгами
			int randomChange = Random.Range(1, 4); // Генерируем случайное число от 1 до 3
			switch (randomChange)
			{
				// Определяем, что делать на основе случайного числа
				case 1:
					// Добавляем 1000 денег
					money += 1000;
					Debug.Log("Перукарня взяла з вас 1000$");
					break;
				case 2:
					// Вычитаем 1000 денег
					money -= 1000;
					break;
				case 3:
					// Вычитаем 2000 денег
					money -= 2000;
					break;
				default:
					// Добавляем 500 денег
					money += 500;
					break;
			}
			SwitchTurn();
		}
		if (gameController.Board.isStartCell(cellBoardIndex))
		{
			SwitchTurn();
		}

		if (gameController.Board.IsJailCell(cellBoardIndex))
		{
			Debug.Log("Ти потрапив за грати");
			SwitchTurn();
		}
		RpcHideRollDiceCanvas(connectionToClient);
	}

	[Command]
	public void CmdSuccessTaxCell(){
		// Проверяем, является ли текущая клетка арендуемой и имеет ли она владельца, который не является текущим игроком
		var cellOwner = gameController.Board.GetOwner(cellBoardIndex);
		if (cellOwner != null)
		{
			if (cellOwner != this)
			{
				int tax = gameController.Board.GetCellTax(cellBoardIndex);
				if (money >= tax)
				{
					money -= tax;            // Текущий игрок оплачивает налог
					cellOwner.money += tax;  // Владелец клетки получает налог
					Debug.Log($"Player: {name} paid {tax}$ tax to Player: {cellOwner.name}");
					SwitchTurn();
				}
				else
				{
					RpcShowTaxesCell();
					// Если у игрока недостаточно средств, вы можете обработать этот случай здесь
					Debug.Log($"Player: {name} doesn't have enough money to pay the tax!");
				}
			}
		}
	}

	[Command]
	public void CmdEndTurn()
	{
		if (GameController.currentPlayer == this)
		{
			if (GameController.currentPlayer.dicesRolled)
			{
				// Вызываем команду для перемещения игрока
				SwitchTurn();
			}
			else
			{
				Debug.Log($"{name} Can't end turn beacuse dices not rolled yet");
			}
		}
		else
		{
			Debug.Log($"{name} Can't end turn. It's not his turn");
		}
	}

	[Server]
	private void SwitchTurn()
	{
		int currentIndex = gameController.allPlayers.IndexOf(GameController.currentPlayer);
		int nextIndex = (currentIndex + 1) % gameController.allPlayers.Count;

		gameController.SetCurrentPlayer(gameController.allPlayers[nextIndex]);
		
		if (isServer)
		{
			GameController.currentPlayer.dicesRolled = false;
			RpcSwitchTurn(GameController.currentPlayer.netId);
		}
	}

	[ClientRpc]
	public void RpcUpdatePosition(Vector2 targetPosition)
	{
		// Обновление позиции игрока на клиенте
		transform.position = targetPosition;
	}

	[ClientRpc]
	public void RpcSwitchTurn(uint newPlayerNetId)
	{
		gameController.SetCurrentPlayer(newPlayerNetId);
	}

	[TargetRpc]
	public void RpcShowRollDiceCanvas(){
		gameController.ShowRollDiceCanvas();
	}

	[TargetRpc]
	public void RpcShowCellName(string cellName)
	{
		// Здесь вызывайте метод в GameController для отображения имени клетки
		gameController.DisplayCellName(cellName);
	}

	[TargetRpc]
	public void RpcHideRollDiceCanvas(NetworkConnection target){
		gameController.HideRollDiceCanvas();
	}

	[TargetRpc]
	public void RpcShowRollCanvas(){
		gameController.ShowRollCanvas();
	}

	[TargetRpc]
	public void HideRollForPlayerCanvas(NetworkConnection target){
		gameController.HideRollCanvas();
	}
	
	[TargetRpc]
	public void RpcShowTaxesCell(){
		gameController.ShowTaxesCellCanvas(cellBoardIndex);
	}


	public void HideTaxesCanvas()
	{
		CmdSuccessTaxCell();
		gameController.HideTaxesCellCanvas(); 
	}

	[TargetRpc]
	public void GameOver(){
		Debug.Log("Game Over!");
		isGameOver = true;

		// Отключить объект игрока
		this.gameObject.SetActive(false);

		// Сбросить клетки, которые игрок купил
		gameController.Board.ResetOwnedCellsByPlayer(this);
	}

	[ClientRpc]
	public void RpcSetCellOwner(uint ownerNetId, int cellIndex)
	{
		Debug.Log("RpcSetCellOwner");
		if (gameController.GetPlayer(ownerNetId, out var targetPlayer))
		{
			Debug.Log($"Player {targetPlayer} owned cell {cellIndex}");
			gameController.Board.SetOwner(targetPlayer, cellIndex);
		}
	}

	[ClientRpc]
	public void RpcTollResult(int dice1, int dice2)
	{
		gameController.result_rollDice.text = dice1.ToString() + "\n" + dice2.ToString();
		gameController.SetDiceSprites(diceSprites[dice1 - 1], diceSprites[dice2 - 1]);
		gameController.UpdatePlayersMoney();
	}
}