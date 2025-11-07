using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject, IDamageable
{
    public Tile obstacleTile;
    public Tile damagedTile;
    public int MaxHealth {get; set;} = 3;
    public int CurrentHealth { get; set;}

    private Tile m_originalTile;
    [SerializeField] private float bounceStrength = 1.0f;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        isWall = true;
        CurrentHealth = MaxHealth;

        m_originalTile = GameManager.Instance.boardManager.GetCellTile(cell);
        GameManager.Instance.boardManager.SetCellTile(cell, obstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        CurrentHealth -= 1;
        
        if (CurrentHealth > 0)
        {
            //knock player back on contact if wall hp is > 0
            Vector3 playerPosition = GameManager.Instance.playerController.transform.position;
            Vector3 wallPosition = transform.position;

            Vector3 deltaV = wallPosition - playerPosition;
            Vector3 bounceDirection = deltaV.normalized;

            //Need to move player directionally along one axis to help prevent OOB. this finds if X or Y axis should be targeted
            if (Mathf.Abs(bounceDirection.x) > Mathf.Abs(bounceDirection.y))
                bounceDirection = new Vector3(Mathf.Sign(bounceDirection.x), 0f, 0f);
            else
                bounceDirection = new Vector3(0, Mathf.Sign(bounceDirection.y), 0f);

            Vector3 bounceTarget = wallPosition - bounceDirection * bounceStrength;
            GameManager.Instance.playerController.transform.position = bounceTarget;

            //Snap player to center of current tile to prevent OOB accidents from bounce off wall
            Vector2Int wallCell = m_Cell;
            Vector2Int bounceCell = wallCell - new Vector2Int((int)Mathf.Round(bounceDirection.x), (int)Mathf.Round(bounceDirection.y));
            Vector3 playerCell = GameManager.Instance.boardManager.CellToWorld(bounceCell);
            GameManager.Instance.playerController.transform.position = playerCell;

            //Stun player
            GameManager.Instance.playerController.Stun();

            if (CurrentHealth == 1)
                GameManager.Instance.boardManager.SetCellTile(m_Cell, damagedTile);

            return false;
        }

        GameManager.Instance.boardManager.SetCellTile(m_Cell, m_originalTile);
        Destroy(gameObject);
        return true;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        if(CurrentHealth <= 0)
        {
            GameManager.Instance.boardManager.SetCellTile(m_Cell, m_originalTile);
            Destroy(gameObject);
        }

        if(CurrentHealth == 1)
                GameManager.Instance.boardManager.SetCellTile(m_Cell, damagedTile);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
