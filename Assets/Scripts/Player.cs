using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
	public enum EState
	{
		Active,
		Watcher,
		Winner,
	}

	[SyncVar(hook = nameof(OnStateChanged))] 
	public EState State;
	[SyncVar] public int cellBoardIndex = 0;
	[SyncVar] public bool dicesRolled;

	[SyncVar] public Color playerColor;
	[SyncVar] public string nickName;

	public int turnsToSkip = 0; // Количество ходов, которые игрок должен пропустить

	[SyncVar(hook = nameof(OnMoneyChanged))]
	public int money = 15000;
	public bool isGameOver = false;

	public Sprite[] diceSprites;  // drag your 6 dice sprites here in the inspector, from 1 to 6
	private GameController gameController;
	public ChatController chatMessageManager;
	public ChatBehaviour chatBehaviour;

	public void SetNickName(string name) {
    	nickName = name;
	}

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
		SetState(EState.Active);
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

	// В классе игрока
	public void SendChatMessageToServer(string message)
	{
		// Этот метод будет вызывать CmdSendChatMessage на объекте чата
		chatBehaviour.CmdSendChatMessage(message);
	}

	[Server]
	public void PerformUpgradeCell(int cellIndex)
	{
		// Make sure this method is in the same class that has the 'manager_board' list or can access it

		// First, we need to get the 'data_board' object from the 'manager_board' list
		if (cellIndex >= 0 && cellIndex < gameController.Board.manager_board.Count)
		{
			var cellData = gameController.Board.manager_board[cellIndex];
			
			// Now we can increment the level
			cellData.level++;
			RpcUpgradeCell(cellIndex, cellData.level);
			// Set new tax and rent prices here based on the new level
			// For example:
			switch(cellData.level)
			{
				case 1:
					cellData.rentPrice = cellData.level_1;
					break;
				case 2:
					cellData.rentPrice = cellData.level_2;
					break;
				case 3:
					cellData.rentPrice = cellData.level_3;
					break;
				case 4:
					cellData.rentPrice = cellData.level_4;
					break;
				case 5:
					cellData.rentPrice = cellData.level_5;
					break;
				// and so on for the other levels...
				default:
					// handle maximum level or invalid level
					break;
			}
			
			// You may want to update any other properties or UI elements that show the level to the player
			// ...
		}
		else
		{
			Debug.LogError("PerformUpgradeCell: Index out of range");
			// Handle the error as needed
		}
	}

	[Server]
	public void PerformDowngradeCell(int cellIndex)
	{
		// Убедитесь, что этот метод находится в том же классе, который имеет список 'manager_board' или имеет к нему доступ

		// Сначала нам нужно получить объект 'cellData' из списка 'manager_board'
		if (cellIndex >= 0 && cellIndex < gameController.Board.manager_board.Count)
		{
			var cellData = gameController.Board.manager_board[cellIndex];
			
			// Теперь мы можем уменьшить уровень
			if (cellData.level > 0) // Проверяем, чтобы уровень был больше 0
			{
				cellData.level--;
				RpcDowngradeCell(cellIndex, cellData.level);
				// Установите новые цены на налог и аренду, исходя из нового уровня
				// Например:
				switch(cellData.level)
				{
					case 0:
						cellData.rentPrice = cellData.baseRent; // Уровень снижен до базового, установите базовую арендную плату
						break;
					case 1:
						cellData.rentPrice = cellData.level_1;
						break;
					case 2:
						cellData.rentPrice = cellData.level_2;
						break;
					case 3:
						cellData.rentPrice = cellData.level_3;
						break;
					case 4:
						cellData.rentPrice = cellData.level_4;
						break;
					// и так далее для других уровней...
					default:
						// обработка максимального уровня или недопустимого уровня
						break;
				}
				
				// Вы можете обновить любые другие свойства или элементы пользовательского интерфейса, которые показывают уровень игроку
				// ...
			}
			else
			{
				Debug.LogError("PerformDowngradeCell: Cell is already at the lowest level");
				// Обработайте ошибку по необходимости
			}
		}
		else
		{
			Debug.LogError("PerformDowngradeCell: Index out of range");
			// Обработайте ошибку по необходимости
		}
	}


	[ClientRpc]
	public void RpcUpgradeCell(int cellIndex, int level)
	{
		// First, we need to access the 'data_board' object from the 'manager_board' list
		if (cellIndex >= 0 && cellIndex < gameController.Board.manager_board.Count)
		{
			var cellData = gameController.Board.manager_board[cellIndex];

			// Update the cell's level with the new level provided
			cellData.level = level;

			// Set new rent price here based on the new level
			switch(cellData.level)
			{
				case 1:
					cellData.rentPrice = cellData.level_1;
					break;
				case 2:
					cellData.rentPrice = cellData.level_2;
					break;
				case 3:
					cellData.rentPrice = cellData.level_3;
					break;
				case 4:
					cellData.rentPrice = cellData.level_4;
					break;
				case 5:
					cellData.rentPrice = cellData.level_5;
					break;
				// Continue with the other cases for additional levels...

				default:
					// Handle the case where the level is invalid or at maximum level
					break;
			}

			// Update any UI elements that display the level to the players
			if (cellData.priceLabel != null)
			{
				cellData.priceLabel.text = cellData.rentPrice.ToString() + "$";
			}

			// Additional synchronization logic goes here, if necessary...
		}
		else
		{
			Debug.LogError("RpcUpgradeCell: Index out of range");
			// Handle the error as needed
		}
	}

	[ClientRpc]
	public void RpcDowngradeCell(int cellIndex, int level)
	{
		// Сначала нам нужно получить доступ к объекту 'cellData' из списка 'manager_board'
		if (cellIndex >= 0 && cellIndex < gameController.Board.manager_board.Count)
		{
			var cellData = gameController.Board.manager_board[cellIndex];

			// Обновляем уровень клетки с новым уровнем, предоставленным сервером
			cellData.level = level;

			// Устанавливаем новую арендную плату исходя из нового уровня
			switch(cellData.level)
			{
				case 0:
					cellData.rentPrice = cellData.baseRent; // Если уровень понижен до базового, устанавливаем базовую арендную плату
					break;
				case 1:
					cellData.rentPrice = cellData.level_1;
					break;
				case 2:
					cellData.rentPrice = cellData.level_2;
					break;
				case 3:
					cellData.rentPrice = cellData.level_3;
					break;
				case 4:
					cellData.rentPrice = cellData.level_4;
					break;
				// Продолжаем со случаями для дополнительных уровней...

				default:
					// Обрабатываем случай, когда уровень недопустим или максимален
					break;
			}

			// Обновляем элементы пользовательского интерфейса, которые отображают уровень игрокам
			if (cellData.priceLabel != null)
			{
				cellData.priceLabel.text = cellData.rentPrice.ToString() + "$";
			}

			// Дополнительная логика синхронизации, если это необходимо...
		}
		else
		{
			Debug.LogError("RpcDowngradeCell: Index out of range");
			// Обрабатываем ошибку по мере необходимости
		}
	}


	[Command]
	public void CmdUpgradeCell(int cellIndex){
		Debug.Log($"Player: {name} Попытка улучшить клетку {cellIndex}");

		// Получаем тип зоны для клетки по индексу.
		board.ECellArea cellAreaType = gameController.Board.GetCellAreaType(cellIndex);

		// Теперь мы можем передать и тип зоны, и игрока в метод CanUpgradeCell.
		if (gameController.Board.CanUpgradeCell(cellAreaType, this)) // 'this' предполагает, что CmdUpgradeCell вызывается для объекта Player
		{
			int cellUpgradeCost = gameController.Board.GetCellUpgrade(cellIndex);
			//var upgradePrice = gameController.Board.GetCellUpgrade(cellIndex);
			if (money >= cellUpgradeCost)
			{
				money -= cellUpgradeCost;
				PerformUpgradeCell(cellIndex);
				Debug.Log("Клітинка прокачана");
				if (gameController.Board.UpgradeCell(cellIndex, this))
				{
					Debug.Log($"Клетка {cellIndex} успешно улучшена до уровня выше");
					// RpcSetCellUpgrade(cellIndex, this.netId);
				}
				else
				{
					Debug.Log($"Ошибка: Клетка {cellIndex} не может быть улучшена.");
					// money += cellUpgradeCost;
				}
			}
			else
			{
				Debug.Log($"Player: {name} Недостатньо грошей для прокачки клітинки {cellIndex}");
			}
		}
		else
    	{
        	Debug.Log($"Player: {name} Не может улучшить клетку {cellIndex}, так как не все условия для улучшения выполнены.");
    	}
	}

	[Command]
	public void CmdUnUpgradeCell(int cellIndex){
		Debug.Log($"Player: {name} Попытка улучшить клетку {cellIndex}");

		// Получаем тип зоны для клетки по индексу.
		board.ECellArea cellAreaType = gameController.Board.GetCellAreaType(cellIndex);

		// Теперь мы можем передать и тип зоны, и игрока в метод CanUpgradeCell.
		if (gameController.Board.CanUpgradeCell(cellAreaType, this)) // 'this' предполагает, что CmdUpgradeCell вызывается для объекта Player
		{
			int cellUpgradeCost = gameController.Board.GetCellUpgrade(cellIndex);
			//var upgradePrice = gameController.Board.GetCellUpgrade(cellIndex);
				money += cellUpgradeCost;
				PerformDowngradeCell(cellIndex);
				Debug.Log("Клітинка понижена");
				if (gameController.Board.DowngradeCell(cellIndex, this))
				{
					Debug.Log($"Клітинка {cellIndex} понижена на рівень нижче");
				}
				else
				{
					Debug.Log($"Помилка: Клітинка {cellIndex} не може бути понижена.");
				}
			
		}
		else
    	{
        	Debug.Log($"Player: {name} Не может улучшить клетку {cellIndex}, так как не все условия для улучшения выполнены.");
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

		// Проверка на пропуск хода
		if (turnsToSkip > 0)
		{
			Debug.Log($"{name}, вам нужно пропустить ход!");
			turnsToSkip--; // Уменьшаем значение на 1
			SwitchTurn();  // Переключаем ход на следующего игрока
			return;        // Выходим из функции, чтобы текущий игрок не делал ход
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
		var cell = gameController.Board.manager_board[cellBoardIndex];
		if (cellOwner == null && gameController.Board.CanBuyCell(cellBoardIndex))
		{
			RpcShowRollCanvas(); // Show the buy canvas to the player
		}
		else if (cellOwner != null)
		{
			if (cellOwner != this && cell.IsMislayed)
			{
				SwitchTurn();
			}
			else if (cellOwner != this)
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
			cellBoardIndex = 10; // Устанавливаем индекс клетки на 10
			targetPosition = gameController.Board.GetPosition(cellBoardIndex);
			transform.position = targetPosition;
			RpcUpdatePosition(targetPosition);
			turnsToSkip = 1;
			SwitchTurn();
		}

		if (gameController.Board.IsDonutCell(cellBoardIndex))
		{
			Debug.Log("Фух, перекус");
			SwitchTurn();
		}
	}

	[Command]
	public void CmdSuccessTaxCell(){
		// Проверяем, является ли текущая клетка арендуемой и имеет ли она владельца, который не является текущим игроком
		var cellOwner = gameController.Board.GetOwner(cellBoardIndex);
		var cell = gameController.Board.manager_board[cellBoardIndex];
		if (cellOwner != null)
		{
			if (cellOwner != this)
			{
				int tax = 0;
				int currentLevel = gameController.Board.GetCurrentLevel(cell);
				switch (currentLevel)
				{
					case 1:
						tax = cell.level_1;
						break;
					case 2:
						tax = cell.level_2;
						break;
					case 3:
						tax = cell.level_3;
						break;
					case 4:
						tax = cell.level_4;
						break;
					case 5:
						tax = cell.level_5;
						break;
					default:
						tax = cell.rentPrice; // Assuming you have a base rent for when the cell is not upgraded
						break;
				}
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

	[Command]
	public void CmdMislay(int cellIndex)
	{
		// Логіка для залогу клітинки. Наприклад:
		var cell = gameController.Board.manager_board[cellIndex];
		var cellOwner = gameController.Board.GetOwner(cellIndex);
		int mislayPrice = gameController.Board.GetCellMislay(cellIndex);
		// Перевірте, чи є поточний гравець власником клітинки
		if (cellOwner != null && cellOwner == this && !cell.IsMislayed)
		{
			cell.IsMislayed = true;
			cell.imageMislayed.enabled = true; // Картинка для відображення чи закладена клітинка
			// Ваша логіка для заставки клітинки тут.
			// Наприклад, змініть статус клітинки, додайте гроші гравцю тощо.
			money += mislayPrice;
			RpcSetCellMislayed(cellIndex, true);
			Debug.Log($"Cell {cell.cellName} has been mislayed!");
		}
		else
		{
			Debug.LogWarning($"Player is not the owner of the cell {cell.cellName} or cell does not have an owner.");
		}
	}

	[Command]
	public void CmdUnMislay(int cellIndex){
		var cell = gameController.Board.manager_board[cellIndex];
        var cellOwner = gameController.Board.GetOwner(cellIndex);
        int unMislayPrice = gameController.Board.GetCellUnMislay(cellIndex);
        if (cellOwner != null && cellOwner == this && cell.IsMislayed)
        {
            if (money >= unMislayPrice)
		    {
                cell.IsMislayed = false;
				cell.imageMislayed.enabled = false; // Картинка для відображення чи закладена клітинка
                // Ваша логіка для заставки клітинки тут.
                // Наприклад, змініть статус клітинки, додайте гроші гравцю тощо.
				money -= unMislayPrice;
				RpcSetCellUnMislayed(cellIndex, false);
                Debug.Log($"Cell {cell.cellName} has been unmislayed!");
            }
            else {
                Debug.Log("You need more money");
            }
        }
        else
        {
            Debug.LogWarning($"Player is not the owner of the cell {cell.cellName} or cell does not have an owner.");
        }
	}

	[Server]
	private void SwitchTurn()
	{
		int currentIndex = gameController.allPlayers.IndexOf(GameController.currentPlayer);
		int nextIndex;

		for (int i = 0; i < gameController.allPlayers.Count; i++) 
		{
			nextIndex = (currentIndex + 1 + i) % gameController.allPlayers.Count;
			Player potentialNextPlayer = gameController.allPlayers[nextIndex];
			
			if (potentialNextPlayer.State != EState.Watcher) 
			{
				gameController.SetCurrentPlayer(potentialNextPlayer);
				if (isServer)
				{
					GameController.currentPlayer.dicesRolled = false;
					RpcSwitchTurn(GameController.currentPlayer.netId);
				}
				break;
			}
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

	public void HideSurrenderCanvas(){
		CmdSetWatcher();
		gameController.HideTaxesCellCanvas();
	}

	public void HideTaxesCanvas()
	{
		CmdSuccessTaxCell();
		gameController.HideTaxesCellCanvas(); 
	}

	[Command]
	public void CmdSetWatcher(){
		Debug.Log("CmdSetWatcher");
		// Сбросить клетки, которые игрок купил
		SetState(EState.Watcher);
		SwitchTurn();
		gameController.HideTaxesCellCanvas(); 
		gameController.Board.ResetOwnedCellsByPlayer(this);
		// GetComponent<Renderer>().enabled = false;
	}

	[ClientRpc]
	public void RpcResetOwnedCellsByPlayer(){
		gameController.Board.ResetOwnedCellsByPlayer(this);
	}

	public void SetState(EState newState)
	{
		State = newState;
	}

	private void OnStateChanged(EState oldState, EState newState)
	{
		if (newState == EState.Winner) {
			Debug.Log("Ви перемогли!");
		} else if (newState == EState.Watcher) {
			Debug.Log("Ви стали спостерігачем.");
		}
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
	public void RpcSetCellMislayed(int cellIndex, bool state)
	{
		Debug.Log("RpcSetCellMislayed");
		gameController.Board.SetMislayed(cellIndex, state);
	}

	[ClientRpc]
	public void RpcSetCellUnMislayed(int cellIndex, bool state)
	{
		Debug.Log("RpcSetCellUnMislayed");
		gameController.Board.SetMislayed(cellIndex, state);
	}

	[ClientRpc]
	public void RpcSetCellUpgrade(int cellIndex, uint ownerNetId)
	{
		Debug.Log("RpcSetCellUpgrade");
		if (gameController.GetPlayer(ownerNetId, out var targetPlayer))
		{
			Debug.Log($"Player {targetPlayer} owned cell {cellIndex}");
			gameController.Board.UpgradeCell(cellIndex, targetPlayer);
		}
	}

	[ClientRpc]
	public void RpcSetCellUnUpgrade(int cellIndex, uint ownerNetId)
	{
		Debug.Log("RpcSetCellUpgrade");
		if (gameController.GetPlayer(ownerNetId, out var targetPlayer))
		{
			Debug.Log($"Player {targetPlayer} owned cell {cellIndex}");
			gameController.Board.DowngradeCell(cellIndex, targetPlayer);
		}
	}

	[ClientRpc]
	public void RpcTollResult(int dice1, int dice2)
	{
		gameController.result_rollDice.text = dice1.ToString() + "\n" + dice2.ToString();
		gameController.SetDiceSprites(diceSprites[dice1 - 1], diceSprites[dice2 - 1]);
		gameController.UpdatePlayersMoney();
	}

	public bool IsOwnerOfCell(board board, int cellIndex)
    {
        var cellOwner = board.GetOwner(cellIndex);
        return cellOwner == this;
    }
}