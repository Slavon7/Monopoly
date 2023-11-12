using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public TMP_Text currentPlayerText;
    public Text result_rollDice;
    public Text[] playerTexts = new Text[5];
    public Image[] playerSquares = new Image[5];
    public SpriteRenderer diceRenderer1;
    public SpriteRenderer diceRenderer2;
    public Canvas taxesCellCanvas;
    public Canvas rollCanvas;
    public Canvas rollDiceCanvas;
    public TextMeshProUGUI cellNameText;  // Drag your TextMeshProUGUI component here in the inspector
    public TextMeshProUGUI rentPriceText; // Drag your RentPriceText component here in the inspector

    [SerializeField] private board fromBoard;
    [SerializeField] private Button rollButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button confirmTaxesButton;
    [SerializeField] private Button surrenderButton;

    public board Board => fromBoard;
    
    public List<Player> allPlayers = new List<Player>();

    public static Player currentPlayer { get; private set; }

    private Dictionary<uint, Player> playersByNetId = new Dictionary<uint, Player>();

    public Player localPlayer;
    
    private void Awake()
    {
        rollButton.onClick.AddListener(OnRollDiceButtonClick);
        buyButton.onClick.AddListener(OnBuyCellButtonClick);
        endTurnButton.onClick.AddListener(OnEndturnButtonClick);
        confirmTaxesButton.onClick.AddListener(OnConfirmTaxesButtonClick);
        surrenderButton.onClick.AddListener(OnSurrenderButtonClick);
    }

    public void AddPlayer(Player player)
    {
        allPlayers.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        allPlayers.Remove(player);
    }

    public void PlayerJoin(uint netId, Player player)
    {
        playersByNetId.Add(netId, player);

        // Викликаємо метод SetNickName для об'єкта player, щоб встановити його нік
        string savedName = PlayerPrefs.GetString("PlayerName", "Player " + (player.netId)); // Используйте значение по умолчанию, если имя не найдено
        player.SetNickName(savedName);
        UpdatePlayersMoney();
    }

    public void PlayerLeft(uint netId)
    {
        if (playersByNetId.TryGetValue(netId, out var player))
        {
            allPlayers.Remove(player);
        }
        playersByNetId.Remove(netId);
        UpdatePlayersMoney();
    }

    public bool GetPlayer(uint newPlayerNetId, out Player player)
    {
        return playersByNetId.TryGetValue(newPlayerNetId, out player);
    }

    public void SetCurrentPlayer(uint playerUid)
    {
        // For the purpose of this example, let's say you want to update the currentPlayer on each client:
        if (GetPlayer(playerUid, out Player newPlayer))
        {
            SetCurrentPlayer(newPlayer);
        }
    }
    
    public void SetCurrentPlayer(Player player)
    {
        currentPlayer = player;

        var playerIndex = allPlayers.IndexOf(player);

        currentPlayerText.text = "Player " + (playerIndex + 1) + " turn";
        currentPlayerText.color = player.playerColor;

        for (int i = 0; i < playerSquares.Length; i++)
        {
            if (i == playerIndex)
            {
                playerSquares[i].color = player.playerColor;  // Установка цвета для текущего игрока
            }
            else
            {
                playerSquares[i].color = new Color(0.2863f, 0.2274f, 0.6705f);  // Установка стандартного цвета для всех других игроков
            }
        }
    }

    public void MislayCell(int cellIndex)
    {
        localPlayer.CmdMislay(cellIndex);
        Debug.Log($" Attempt mislay Cell {cellIndex}");
    }

    public void unMislayCell(int cellIndex)
    {
        localPlayer.CmdUnMislay(cellIndex);
        Debug.Log($" Attempt mislay Cell {cellIndex}");
    }

    public void UpgradeCell(int cellIndex)
    {
        localPlayer.CmdUpgradeCell(cellIndex);
        Debug.Log($" Attempt upgrade Cell {cellIndex}");
    }

    public void UnUpgradeCell(int cellIndex)
    {
        localPlayer.CmdUnUpgradeCell(cellIndex);
        Debug.Log($" Attempt upgrade Cell {cellIndex}");
    }

    public void AssignLocalPlayer(Player player)
    {
        localPlayer = player;
    }

    [Server]
    public void SERVER_AssignPlayerColor(Player player)
    {
        switch (allPlayers.Count)
        {
            case 1: // First player
                player.playerColor = new Color(0.1333f, 0.8392f, 0.2353f);
                break;
            case 2: // Second player
                player.playerColor = new Color(0.3412f, 0.451f, 1f);
                break;
            case 3: // Third player
                player.playerColor = new Color(1f, 0.6157f, 0.2f);
                break;
            case 4: // Fourth player
                player.playerColor = new Color(0.9608f, 0.2078f, 0.4980f);
                break;
            case 5: // Fifth player
                player.playerColor = new Color(0.6706f, 0.4431f, 1f);
                break;
            // Add more cases if there are more players with different colors.
        }
    }
    
    public void SetDiceSprites(Sprite diceSprite1, Sprite diceSprite2)
    {
        diceRenderer1.sprite = diceSprite1;
        diceRenderer2.sprite = diceSprite2;
    }

    public void UpdatePlayersMoney()
    {
        for (int i = 0; i < playerTexts.Length; i++)
        {
            if (i < allPlayers.Count)
            {
                Player player = allPlayers[i];
                // Получаем имя для каждого конкретного игрока
                // string playerName = PlayerPrefs.GetString("PlayerName" + player.netId, "Player " + (i + 1));
                playerTexts[i].text = player.nickName + "\n" + player.money.ToString() + "$";
            }
            else
            {
                playerTexts[i].text = "";
            }
        }
    }

    private void OnRollDiceButtonClick()
    {
        if (localPlayer != null)
        {
            localPlayer.CmdRollDices();
        }
    }
    
    public void DisplayCellName(string name)
    {
        cellNameText.text = name;
    }

    private void OnBuyCellButtonClick()
    {
        if (localPlayer != null)
        {
            localPlayer.CmdAttemptToBuyCell();
        }
    }
    
    private void OnEndturnButtonClick()
    {
        if (localPlayer != null)
        {
            localPlayer.CmdEndTurn();
            rollCanvas.gameObject.SetActive(false);
        }
    }

    private void OnSurrenderButtonClick()
    {
        if (localPlayer != null)
        {
            localPlayer.HideSurrenderCanvas();
        }
    }

    private void OnConfirmTaxesButtonClick()
    {
        if (localPlayer != null)
        {
            localPlayer.HideTaxesCanvas();
        }
    }

    public void ShowRollDiceCanvas(){
        rollDiceCanvas.gameObject.SetActive(true);
        Debug.Log("ShowRollDiceCanvas");
    }

    public void HideRollDiceCanvas(){
        rollDiceCanvas.gameObject.SetActive(false);
        Debug.Log("HideRollDiceCanvas");
    }

    public void ShowRollCanvas()
    {
        rollCanvas.gameObject.SetActive(true);
        Debug.Log("ShowRollCanvas");
    }
    public void HideRollCanvas()
    {
        rollCanvas.gameObject.SetActive(false);
        Debug.Log("HideRollCanvas");
    }

    public void ShowTaxesCellCanvas(int currentCellIndex)
    {
        // Fetch the rent price from the board
        int rentPrice = Board.GetCellTax(currentCellIndex);
        
        // Update the UI text component with the rent price
        rentPriceText.text = "Орендна плата: " + rentPrice.ToString() + "$";
        
        // Activate the canvas
        taxesCellCanvas.gameObject.SetActive(true);
        Debug.Log("ShowTaxesCellCanvas");
    }

    public void HideTaxesCellCanvas()
    {
        taxesCellCanvas.gameObject.SetActive(false);
        Debug.Log("HideTaxesCellCanvas");
    }
}
