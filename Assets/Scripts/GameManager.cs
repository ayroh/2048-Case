using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState {
    CreateGame,
    Ready,
    Moving
}

public class GameManager : MonoBehaviour
{

    [Header("Prefabs")]
    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private Block blockPrefab;

    [Header("Grid")]
    public int gridLength;

    [Header("Parents")]
    [SerializeField] private Transform blockParent;

    [Header("UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject restartGameButton;
    [SerializeField] private TextMeshProUGUI gridLengthText;

    [Header("Misc")]
    [SerializeField] private TouchManager touchManager;
    [SerializeField] private float movementTime = .1f;
    [SerializeField] private PoolingManager poolingManager;
    [SerializeField] private UnityEngine.UI.Slider gridLengthSlider;

    private List<Block> blocks;
    private List<Tile> tiles;
    private List<Tile> movingTileList;

    private GameState gameState;

    private void ChangeState(GameState newState) => gameState = newState;


    #region Move/Merge
    private void QueryMoves(List<Tile> orderedTiles, Vector3 direction, bool isEndGameQuery = false) {
        // Positions changes according to gridLength
        direction *= 4 / (float)gridLength;
        Block next = null;
        movingTileList = new();

        for (int i = 0;i < orderedTiles.Count;++i) {
            while (true) {
                // Try finding adjacent block
                next = blocks.Find(block => block.pos == orderedTiles[i].block.pos + direction);

                // If there is one
                if (next != null) {

                    // If it is empty then set as new block and continue searching for new next adjacent block
                    if (next.empty) {
                        if (!movingTileList.Contains(orderedTiles[i]))
                            movingTileList.Add(orderedTiles[i]);
                        orderedTiles[i].block.Empty();
                        orderedTiles[i].block = next;
                        next.Fill(orderedTiles[i]);
                    }

                    // If it is not empty, but values are same and tile inside that block is not merging with another tile, merge with that block
                    else if (orderedTiles[i].value == next.tile.value && !next.tile.merging) {

                        // Add garbage to movingTileList for indicating end game
                        if (isEndGameQuery) {
                            movingTileList.Add(tiles[0]);
                            return;
                        }

                        if (!movingTileList.Contains(orderedTiles[i]))
                            movingTileList.Add(orderedTiles[i]);
                        orderedTiles[i].block.Empty();
                        orderedTiles[i].block = next;
                        orderedTiles[i].mergingTile = next.tile;
                        next.tile.merging = true;
                        break;
                    }
                    // Else break while because adjacent is not empty
                    else
                        break;
                }
                else
                    break;
            }
        }

    }

    public void Swipe(Vector2 direction) {
        if (gameState != GameState.Ready)
            return;
        List<Tile> orderedTiles = null;

        // Order tiles according to direction
        if(direction == Vector2.left) 
            orderedTiles = tiles.OrderBy(tile => tile.block.pos.x).ToList();
        else if (direction == Vector2.right)
            orderedTiles = tiles.OrderByDescending(tile => tile.block.pos.x).ToList();
        else if (direction == Vector2.down) 
            orderedTiles = tiles.OrderBy(tile => tile.block.pos.y).ToList();
        else if (direction == Vector2.up)
            orderedTiles = tiles.OrderByDescending(tile => tile.block.pos.y).ToList();

        // Store all moves to movingTileList
        QueryMoves(orderedTiles, direction);

        // If there are any moves, do it with Coroutine
        if (movingTileList.Count != 0)
            StartCoroutine(Move());
    }


    private void Merge(Tile stillTile, Tile mergingTile) {
        // Set stillTile as merged tile
        stillTile.value *= 2;
        if (stillTile.value == 2048)
            EndGame(true);
        stillTile.merging = false;

        // Remove from list and release to pooling manager
        tiles.Remove(mergingTile);
        poolingManager.Release(mergingTile);
    }

    private IEnumerator Move() {
        ChangeState(GameState.Moving);
        float timer = 0f;

        // Lerp between starting and destination positions
        List<Vector3> startingPositions = new();
        for (int i = 0;i < movingTileList.Count;++i) 
            startingPositions.Add(movingTileList[i].transform.position);

        while (timer < 1) {
            for (int i = 0;i < movingTileList.Count;++i) 
                movingTileList[i].transform.position = Vector3.Lerp(startingPositions[i], movingTileList[i].block.pos, timer);
            timer += Time.deltaTime / movementTime;
            yield return null;
        }

        // If lerp ends before reaching exactly "1", directly equalize to destination point
        // and check if any merge occurs
        for (int i = 0;i < movingTileList.Count;++i) {
            movingTileList[i].transform.position = movingTileList[i].block.pos;
            if(movingTileList[i].mergingTile != null)
                Merge(movingTileList[i].mergingTile, movingTileList[i]);

        }

        // Add new tile
        AddTile();

        ChangeState(GameState.Ready);
    }


    
    private bool CheckMoves() {
        // Try to query a move for all directions
        // If there isn't any move then end game
        List<Tile> orderedTiles = null;

        orderedTiles = tiles.OrderBy(tile => tile.block.pos.x).ToList();
        QueryMoves(orderedTiles, Vector2.left, true);
        if (movingTileList.Count != 0)
            return true;

        orderedTiles = tiles.OrderByDescending(tile => tile.block.pos.x).ToList();
        QueryMoves(orderedTiles, Vector2.right, true);
        if (movingTileList.Count != 0)
            return true;

        orderedTiles = tiles.OrderBy(tile => tile.block.pos.y).ToList();
        QueryMoves(orderedTiles, Vector2.down, true);
        if (movingTileList.Count != 0)
            return true;

        orderedTiles = tiles.OrderByDescending(tile => tile.block.pos.y).ToList();
        QueryMoves(orderedTiles, Vector2.up, true);
        if (movingTileList.Count != 0)
            return true;

        return false;
    }


    private void AddTile() {
        // Select all empty blocks
        List<Block> emptyBlocks = blocks.Where(block => block.empty == true).ToList();

        int randomValue = Random.Range(0, emptyBlocks.Count);

        // Get Tile from pool to tiles List
        tiles.Add(poolingManager.Get());

        // Initialize values
        tiles[^1].transform.position = emptyBlocks[randomValue].pos;
        tiles[^1].value = Random.value > 0.9f ? 4 : 2;

        // Add tile to empty block
        emptyBlocks[randomValue].Fill(tiles[^1]);

        if(emptyBlocks.Count == 1 && !CheckMoves())
            EndGame(false);
    }

    #endregion

    #region Start/End Game

    private void CreateGame() {
        ChangeState(GameState.CreateGame);

        // Create Background Image
        Instantiate(backgroundPrefab, Vector3.zero, Quaternion.identity);

        // Create Blocks
        blocks = new();

        // Starts from -1.5 - Center points between different block sizes (4x4 and 5x5 have different size of blocks)
        // Ends at 2.5 + Center points between different block sizes
        // Increments according to grid length
        for (float i = -1.5f - (0.5f * (1 - (4 / (float)gridLength)));i < 2.5f - (0.5f * (1 - (4 / (float)gridLength))) - 0.1;i += 1f * (4 / (float)gridLength)) {
            for (float j = 1.5f + (0.5f * (1 - (4 / (float)gridLength)));j > -2.5f + (0.5f * (1 - (4 / (float)gridLength))) + 0.1;j -= 1f * (4 / (float)gridLength)) {
                blocks.Add(Instantiate(blockPrefab, new Vector3(i, j, 0f), Quaternion.identity, blockParent));
                blocks[^1].transform.localScale *= (float)4 / gridLength;
            }
        }

        // Add random Tiles
        tiles = new();
        AddTile();
        AddTile();

        ChangeState(GameState.Ready);
    }

    private void EndGame(bool isWin) {
        // Close mouse/touch input
        touchManager.gameObject.SetActive(false);
        if (isWin)
            winPanel.SetActive(true);
        else
            losePanel.SetActive(true);
    }
    #endregion

    #region Buttons/UI


    public void StartGame() {
        poolingManager.StartGame();
        CreateGame();
        touchManager.gameObject.SetActive(true);
    }

    public void RestartGame() => SceneManager.LoadScene(0);


    public void ChangeGridLength() {
        gridLength = (int)gridLengthSlider.value;
        gridLengthText.text = gridLength.ToString() + "x" + gridLength.ToString();
    }

    #endregion
}
