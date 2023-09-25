using System.Collections;
using UnityEngine.Pool;
using UnityEngine;

public class PoolingManager : MonoBehaviour {

    [SerializeField] private Transform tileParent;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameManager gameManager;

    private ObjectPool<Tile> pool;
    public enum ChibiType { Default = 0 };

    public void StartGame() {
        // Initialize pool
        pool = new ObjectPool<Tile>(Create, ActionOnGet, ActionOnRelease, null, true, gameManager.gridLength * gameManager.gridLength, gameManager.gridLength * gameManager.gridLength);

        // Initialize 16 - 25 - 36.. according to grid length.
        // This way there won't be any Create inside the game, there will be only Get.
        Tile[] tempRelease = new Tile[gameManager.gridLength * gameManager.gridLength];
        for (int i = 0;i < gameManager.gridLength * gameManager.gridLength;++i)
            tempRelease[i] = pool.Get();
        for (int i = 0;i < gameManager.gridLength * gameManager.gridLength;++i)
            Release(tempRelease[i]);
    }

    public Tile Create() {
        Tile tile = Instantiate(tilePrefab, new Vector3(-100, -100, -100), Quaternion.identity, tileParent);
        tile.transform.localScale *= (float)4 / gameManager.gridLength;
        return tile;
    }

    private void ActionOnGet(Tile tile) {
        tile.gameObject.SetActive(true);
    }

    private void ActionOnRelease(Tile tile) {
        tile.gameObject.SetActive(false);
        tile.ResetTile();
    }

    public void Release(Tile tile) => pool.Release(tile);

    public Tile Get() => pool.Get();


}