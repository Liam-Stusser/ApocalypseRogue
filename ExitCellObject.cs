using UnityEngine;
using UnityEngine.Tilemaps;

public class ExitCellObject : CellObject
{
    public Tile endTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        GameManager.Instance.boardManager.SetCellTile(cell, endTile);
    }

    public override void PlayerEntered()
    {
        GameManager.Instance.NewLevel();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
