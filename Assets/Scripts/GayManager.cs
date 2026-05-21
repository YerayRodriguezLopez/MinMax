using UnityEngine;

public class GayManager : MonoBehaviour
{
    public static GayManager Instance;
    public Node[] map = new Node[9];

    public bool playerTurn = true;

    private void Awake()
    {

        //singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Restart()
    {
        for (int i = 0; i < map.Length; i++)
        {
            map[i].index = i;
            map[i].TileValue = 0;
            map[i].UpdateVisual();
        }
        playerTurn = true;
    }

    public void OnTileClicked(Node tile)
    {
        //si no es el torn del player o la cel·la esta ocupada, no fa res
        if (!playerTurn || tile.TileValue != 0) return;

        //asigna el valor del player a la tile i actualitza el mapa
        tile.TileValue = 1;
        tile.UpdateVisual();

        //si el joc no ha acabat, dona el torn a la ia
        if (CheckGame()) return;

        playerTurn = false;
        Invoke(nameof(AITurn), 0.5f);
    }

    bool CheckGame()
    {
        //retorna si el joc a acabat de cualsevol forma o si encara hauria de continuar
        int[] currentBoard = new int[9];
        for (int i = 0; i < 9; i++) currentBoard[i] = map[i].TileValue;

        int state = GetState(currentBoard);
        if (state != 0)
        {
            if (state == 1) Debug.Log("Guanya el jugador");
            else if (state == -1) Debug.Log("Guanya la IA");
            else if (state == 3) Debug.Log("Empat");
            return true;
        }
        return false;
    }

   
    public float MinMax(int[] board, int depth, float alpha, float beta, bool isMaximizing)
    {
        //si hi ha un guanyador o hi ha un empat es retorna el estat 
        int state = GetState(board);
        if (state != 0) return (state == 3) ? 0 : state;

        //torns hipotetics del player
        if (isMaximizing)
        {
            float bestEval = -Mathf.Infinity;
            //per cada cel·la lliure mira totes les posibilitats de moviments
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == 0)
                {
                    //asumeix que el jugador fa click en una cel·la, valida i torna al estat original
                    board[i] = 1;
                    float eval = MinMax(board, depth - 1, alpha, beta, false);
                    board[i] = 0;

                    //si es dona condicio de poda deixa de evaluar la resta de casos
                    bestEval = Mathf.Max(bestEval, eval);
                    alpha = Mathf.Max(alpha, eval);

                    if (beta <= alpha) break;
                }
            }
            return bestEval;
        }
        else
        {
            //el mateix que adalt pero en el torn de la ia
            float bestEval = Mathf.Infinity;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == 0)
                {
                    board[i] = -1;
                    float eval = MinMax(board, depth - 1, alpha, beta, true);
                    board[i] = 0;

                    bestEval = Mathf.Min(bestEval, eval);
                    beta = Mathf.Min(beta, eval);

                    
                    if (beta <= alpha) break;
                }
            }
            return bestEval;
        }
    }

    void AITurn()
    {
        int bestMove = -1;
        float bestValue = Mathf.Infinity;

        //fa una copia del mapa pero amb el valor numeric de les cel·les
        int[] currentBoard = new int[9];
        for (int i = 0; i < 9; i++) currentBoard[i] = map[i].TileValue;


        //mira les posibilitats de moviments d'entre les cel·les lliures i troba la mes optima
        for (int i = 0; i < 9; i++)
        {
            if (currentBoard[i] == 0)
            {
                currentBoard[i] = -1;
                
                float moveValue = MinMax(currentBoard, 9, -Mathf.Infinity, Mathf.Infinity, true);
                currentBoard[i] = 0;

                if (moveValue < bestValue)
                {
                    bestValue = moveValue;
                    bestMove = i;
                }
            }
        }

        //si troba un bon moviment marca la cel·la i actualitza el mapa
        if (bestMove != -1)
        {
            map[bestMove].TileValue = -1;
            map[bestMove].UpdateVisual();
        }

        //com abans, si el joc no ha acabat pasa el torn
        if (!CheckGame()) playerTurn = true;
    }

    public int GetState(int[] mapState)
    {
        //retorna l'estat del joc 1 -> guanya player, 0 -> joc encara segueix, -1 -> guanya ia, 3 -> empat

        for (int i = 0; i < 3; i++)
        {
            //files
            if (mapState[i * 3] != 0 && mapState[i * 3] == mapState[i * 3 + 1] && mapState[i * 3] == mapState[i * 3 + 2])
                return mapState[i * 3];
            //columnes
            if (mapState[i] != 0 && mapState[i] == mapState[i + 3] && mapState[i] == mapState[i + 6])
                return mapState[i];
        }

        //diagonals
        if (mapState[4] != 0)
        {
            if (mapState[0] == mapState[4] && mapState[4] == mapState[8]) return mapState[4];
            if (mapState[2] == mapState[4] && mapState[4] == mapState[6]) return mapState[4];
        }

        //si encara queden cel·les lliures i no ha retornat cap vencçedor abans encara segueix el joc
        foreach (int val in mapState) if (val == 0) return 0;

        //si no es dona cap condicio previa es un empat
        return 3;
    }
}