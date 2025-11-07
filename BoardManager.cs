using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{

    public class CellData
    {
        public bool isPassable;
        public CellObject ContainedObject;
    }

    private Tilemap m_tilemap;
    private Grid m_grid;

    public int width;
    public int height;
    public int wallOffset = 4;
    
    public Tile[] groundTiles;
    public Tile[] wallTiles;
    private CellData[,] m_boardData;
    private List<Vector2Int> m_emptyCells;
    private List<Vector2Int> m_allTiles = new List<Vector2Int>();

    public PlayerController player;

    public FoodObject[] foodPrefabs;
    public WallObject[] wallPrefabs;
    public EnemyObject[] enemyPrefabs;
    public ExitCellObject exitCellPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(int xWidth, int yHeight, int xMinWalls, int xMaxWalls, int xMinFood, int xMaxFood, int xFoodLevel, int xMinEnemy, int xMaxEnemy, int xEnemyTypes)
    {
        width = xWidth;
        height = yHeight;

        m_tilemap = GetComponentInChildren<Tilemap>();
        m_grid = GetComponentInChildren<Grid>();

        //Reset transforms and tilemap state to make sure player spawns correctly each time
        m_grid.transform.position = Vector3.zero;
        m_grid.transform.localScale = Vector3.one;
        m_tilemap.transform.localPosition = Vector3.zero;
        m_tilemap.transform.localScale = Vector3.one;

        // Clear all previous tiles to reset bounds
        m_tilemap.ClearAllTiles();

        m_boardData = new CellData[width, height];
        m_emptyCells = new List<Vector2Int>();
        m_allTiles = new List<Vector2Int>();

        for (int y =0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                Tile tile;
                m_boardData[x, y] = new CellData();

                // Create a border wall inset by wallOffset
                if (x < wallOffset || y < wallOffset || x >= width - wallOffset || y >= height - wallOffset)
                {
                    tile = groundTiles[Random.Range(0, groundTiles.Length)];
                    m_boardData[x, y].isPassable = true; // outer border floor background
                }
                else if (x == wallOffset || y == wallOffset ||x == width - wallOffset - 1 || y == height - wallOffset - 1)
                {
                    tile = wallTiles[Random.Range(0, wallTiles.Length)];
                    m_boardData[x, y].isPassable = false; // actual playable area border walls
                }
                else
                {
                    tile = groundTiles[Random.Range(0, groundTiles.Length)];
                    m_boardData[x, y].isPassable = true;
                    m_emptyCells.Add(new Vector2Int(x, y));
                    m_allTiles.Add(new Vector2Int(x, y));
                }

                m_tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        //Removes player spawn from empty cells list
        m_emptyCells.Remove(new Vector2Int(6, 6));

        //Use Manhattan distances from player to get a random tile far away from player for exit tile
        var distances = m_allTiles.Where(tile => //Had to go nuclear with the in bounds check for the exit tile
            m_boardData[tile.x, tile.y].isPassable && //as the dynamic grid size was causing issues with it spawning OOB
            tile.x > wallOffset + 1 && tile.x < width - wallOffset - 2 &&
            tile.y > wallOffset + 1 && tile.y < height - wallOffset - 2)
        .Select(tile => new
        {
            Tile = tile,
            Distance = Mathf.Abs(tile.x - 6) + Mathf.Abs(tile.y - 6)
        });
        int maxDistance = distances.Max(d => d.Distance);
        var farthestTiles = distances.Where(d => d.Distance == maxDistance).Select(d => d.Tile).ToList();
        Vector2Int farthestTile = farthestTiles[Random.Range(0, farthestTiles.Count)];

        AddObject(Instantiate(exitCellPrefab), farthestTile);
        m_emptyCells.Remove(farthestTile);

        GenerateEnimies(xMinEnemy, xMaxEnemy, xEnemyTypes);
        GenerateWall(xMinWalls, xMaxWalls);
        GenerateFood(xMinFood, xMaxFood, xFoodLevel);
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.y < 0 || cellIndex.x >= width || cellIndex.y >= height)
        {
            return null;
        }

        return m_boardData[cellIndex.x, cellIndex.y];
    }

    public CellData GetCellDataAtWorldPosition(Vector3 worldPosition)
    {
        Vector3Int cellIndex = m_grid.WorldToCell(worldPosition);
        return GetCellData((Vector2Int)cellIndex);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_boardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    void GenerateFood(int minFood, int maxFood, int foodLevel)
    {
        int foodCount = Random.Range(minFood,maxFood);
        for(int i = 0; i < foodCount; i++)
        {
            int RandomIndex = Random.Range(0, m_emptyCells.Count);
            int foodIndex = Random.Range(0, foodLevel);
            Vector2Int cellPos = m_emptyCells[RandomIndex];

            m_emptyCells.RemoveAt(RandomIndex);
            FoodObject food = Instantiate(foodPrefabs[foodIndex]);
            AddObject(food, cellPos);
           
        }
    }

    void GenerateWall(int minWalls, int maxWalls)
    {
        int wallCount = Random.Range(minWalls, maxWalls);
        for (int i = 0; i < wallCount; i++)
        {
            int randomIndex = Random.Range(0, m_emptyCells.Count);
            int randomWall = Random.Range(0, wallPrefabs.Length);
            Vector2Int coord = m_emptyCells[randomIndex];

            m_emptyCells.RemoveAt(randomIndex);
            WallObject newWall = Instantiate(wallPrefabs[randomWall]);
            AddObject(newWall, coord);
        }
    }

    void GenerateEnimies(int minEnemy, int maxEnemy, int enemyTypes)
    {
        int enemyAmount = Random.Range(minEnemy, maxEnemy);
        for (int i = 0; i < enemyAmount; i++)
        {
            int randomIndex = Random.Range(0, m_emptyCells.Count);
            int randomEnemy = Random.Range(0, enemyTypes);
            Vector2Int coord = m_emptyCells[randomIndex];

            m_emptyCells.RemoveAt(randomIndex);
            EnemyObject newEnemy = Instantiate(enemyPrefabs[randomEnemy]);
            AddObject(newEnemy, coord);
        }
    }

    public List<EnemyObject> GetAllEnemies()
    {
        List<EnemyObject> enemies = new List<EnemyObject>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = m_boardData[x, y];
                if (cell.ContainedObject is EnemyObject enemy)
                {
                    enemies.Add(enemy);
                }
            }
        }

        return enemies;
    }

    public void Clean()
    {
        if (m_boardData == null)
            return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cellData = m_boardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    //Make sure to destory the game object not just ContainedObject 
                    //otherwise sprite data in gameObject will persist (memory leak)
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }

        GameObject[] bars = GameObject.FindGameObjectsWithTag("HealthBar");
        foreach (var bar in bars)
        {
            Destroy(bar); //clear leftover health bars
        }
    }
}
