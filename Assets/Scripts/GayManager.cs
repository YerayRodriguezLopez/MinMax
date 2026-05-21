using UnityEngine;
using UnityEngine.UI;

public class GayManager : MonoBehaviour
{
    public static GayManager Instance;

    [Header("Tiles")]
    public Node[] map;

    [Header("Sprites")]
    public Sprite spriteX;
    public Sprite spriteO;

    [Header("UI - Selection Screen")]
    public GameObject selectionPanel;
    public Button btnGoFirst;
    public Button btnGoSecond;

    [Header("UI - Game Screen")]
    public GameObject gamePanel;
    public Text statusText;
    public Button restartButton;

    // 1 = jugador, -1 = máquina
    public int playerSymbol;
    public int machineSymbol;
    public bool playerTurn;
    private bool gameOver;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowSelectionScreen();
    }

    // ── Pantalla de selección ────────────────────────────────────────────────

    void ShowSelectionScreen()
    {
        selectionPanel.SetActive(true);
        gamePanel.SetActive(false);

        btnGoFirst.onClick.RemoveAllListeners();
        btnGoSecond.onClick.RemoveAllListeners();
        btnGoFirst.onClick.AddListener(() => StartGame(goFirst: true));
        btnGoSecond.onClick.AddListener(() => StartGame(goFirst: false));
    }

    void StartGame(bool goFirst)
    {
        selectionPanel.SetActive(false);
        gamePanel.SetActive(true);

        if (goFirst)
        {
            playerSymbol = 1;   // X
            machineSymbol = -1; // O
            playerTurn = true;
            statusText.text = "Tu turno (X)";
        }
        else
        {
            playerSymbol = -1;  // O
            machineSymbol = 1;  // X
            playerTurn = false;
            statusText.text = "Turno de la máquina...";
        }

        ResetBoard();

        if (!playerTurn)
            Invoke(nameof(MachinePlay), 0.5f);
    }

    // ── Lógica de juego ──────────────────────────────────────────────────────

    void ResetBoard()
    {
        gameOver = false;
        foreach (var node in map)
        {
            node.TileValue = 0;
            node.UpdateVisual();
            node.SetInteractable(true);
        }
    }

    public void OnTileClicked(Node node)
    {
        if (!playerTurn || gameOver || node.TileValue != 0) return;

        node.TileValue = playerSymbol;
        node.UpdateVisual();
        node.SetInteractable(false);

        if (CheckWin(playerSymbol))
        {
            statusText.text = "¡Ganaste! 🎉";
            gameOver = true;
            DisableAllTiles();
            return;
        }

        if (IsBoardFull())
        {
            statusText.text = "¡Empate!";
            gameOver = true;
            return;
        }

        playerTurn = false;
        statusText.text = "Turno de la máquina...";
        Invoke(nameof(MachinePlay), 0.6f);
    }

    void MachinePlay()
    {
        if (gameOver) return;

        int bestScore = int.MinValue;
        Node bestNode = null;

        foreach (var node in map)
        {
            if (node.TileValue != 0) continue;

            node.TileValue = machineSymbol;
            int score = Minimax(false, 0);
            node.TileValue = 0;

            if (score > bestScore)
            {
                bestScore = score;
                bestNode = node;
            }
        }

        if (bestNode != null)
        {
            bestNode.TileValue = machineSymbol;
            bestNode.UpdateVisual();
            bestNode.SetInteractable(false);
        }

        if (CheckWin(machineSymbol))
        {
            statusText.text = "¡La máquina gana!";
            gameOver = true;
            DisableAllTiles();
            return;
        }

        if (IsBoardFull())
        {
            statusText.text = "¡Empate!";
            gameOver = true;
            return;
        }

        playerTurn = true;
        statusText.text = playerSymbol == 1 ? "Tu turno (X)" : "Tu turno (O)";
    }

    // ── Minimax ──────────────────────────────────────────────────────────────

    int Minimax(bool isMaximizing, int depth)
    {
        if (CheckWin(machineSymbol)) return 10 - depth;
        if (CheckWin(playerSymbol)) return depth - 10;
        if (IsBoardFull()) return 0;

        if (isMaximizing)
        {
            int best = int.MinValue;
            foreach (var node in map)
            {
                if (node.TileValue != 0) continue;
                node.TileValue = machineSymbol;
                best = Mathf.Max(best, Minimax(false, depth + 1));
                node.TileValue = 0;
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            foreach (var node in map)
            {
                if (node.TileValue != 0) continue;
                node.TileValue = playerSymbol;
                best = Mathf.Min(best, Minimax(true, depth + 1));
                node.TileValue = 0;
            }
            return best;
        }
    }

    // ── Utilidades ───────────────────────────────────────────────────────────

    bool CheckWin(int symbol)
    {
        int[,] lines = {
            {0,1,2}, {3,4,5}, {6,7,8},   // filas
            {0,3,6}, {1,4,7}, {2,5,8},   // columnas
            {0,4,8}, {2,4,6}              // diagonales
        };

        for (int i = 0; i < lines.GetLength(0); i++)
        {
            if (map[lines[i, 0]].TileValue == symbol &&
                map[lines[i, 1]].TileValue == symbol &&
                map[lines[i, 2]].TileValue == symbol)
                return true;
        }
        return false;
    }

    bool IsBoardFull()
    {
        foreach (var node in map)
            if (node.TileValue == 0) return false;
        return true;
    }

    void DisableAllTiles()
    {
        foreach (var node in map)
            node.SetInteractable(false);
    }

    public Sprite GetSprite(int value)
    {
        if (value == 1) return spriteX;
        if (value == -1) return spriteO;
        return null;
    }

    // ── Restart ──────────────────────────────────────────────────────────────

    public void Restart()
    {
        ShowSelectionScreen();
    }
}