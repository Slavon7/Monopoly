using UnityEngine;
using Mirror;

public class UniqueColorAssigner : NetworkBehaviour
{
    public Sprite[] colorSprites; // Array of color sprites (make sure it has 5 sprites)
    [SyncVar(hook = nameof(OnChangeColor))]
    private int selectedSpriteIndex; // Index of the selected sprite, synchronized across clients
    public SpriteRenderer playerRenderer; // Reference to the player's SpriteRenderer component

    // Статическая переменная для отслеживания количества игроков
    private static int playerCount = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Назначаем спрайт на основе playerCount, увеличиваем playerCount
        selectedSpriteIndex = playerCount % colorSprites.Length; // это предотвратит ошибки, если игроков больше 5
        playerCount++;

        // Обновляем спрайт
        OnChangeColor(0, selectedSpriteIndex);
    }

    // Этот метод вызывается каждый раз, когда значение selectedSpriteIndex изменяется
    void OnChangeColor(int oldSpriteIndex, int newSpriteIndex)
    {
        Sprite chosenSprite = colorSprites[newSpriteIndex];
        playerRenderer.sprite = chosenSprite;
    }
}
