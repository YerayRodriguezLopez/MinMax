using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    // 0 = vacío, 1 = jugador/X, -1 = máquina/O
    public int TileValue;
    public int index;

    [Header("Componentes")]
    public Image tileImage;
    public Button button;

    public void UpdateVisual()
    {
        Sprite s = GayManager.Instance.GetSprite(TileValue);
        tileImage.sprite = s;

        if (TileValue == GayManager.Instance.playerSymbol)
            tileImage.color = Color.green;
        else if (TileValue == GayManager.Instance.machineSymbol)
            tileImage.color = Color.red;
        else
            tileImage.color = Color.white;
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    public void OnClick()
    {
        Debug.Log("Click en tile " + index);
        GayManager.Instance.OnTileClicked(this);
    }
}