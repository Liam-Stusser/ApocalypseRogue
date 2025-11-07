using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using UnityEngine;

public class EnemyObject : CellObject, IDamageable
{
    public int enemyHealth;
    public int enemyDamage;
    public float moveSpeed = 1f;
    public int internalTurnCount;
    private List<Vector2Int> path;
    private SpriteRenderer m_spriteRenderer;
    private CharacterAudio audioController;
    private Animator m_animator;
    private HealthBar healthBar;
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        MaxHealth = enemyHealth;
        CurrentHealth = MaxHealth;
        internalTurnCount = 0;
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
        audioController = GetComponent<CharacterAudio>();
        healthBar = Instantiate(Resources.Load<HealthBar>("HealthBar"));
        healthBar.Init(transform, new Vector3(0, 0.5f, 0));
        healthBar.SetValue(CurrentHealth, MaxHealth);
        if (GameManager.Instance?.turnManager != null)
            GameManager.Instance.turnManager.OnTurnAdvanced += OnTurnAdvanced;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance?.turnManager != null)
            GameManager.Instance.turnManager.OnTurnAdvanced -= OnTurnAdvanced;
    }

    private void OnTurnAdvanced(int turn)
    {
        if (turn <= internalTurnCount) return;
        internalTurnCount = turn;

        if (!GameManager.Instance || !GameManager.Instance.boardManager || !GameManager.Instance.playerController)
            return;

        if (IsPlayerAdjacent())
        {
            EnemyAttack(enemyDamage);
        }
        else
        {
            StartCoroutine(MoveEnemy());
        }
    }

    private IEnumerator MoveEnemy() //move enemy is an enumerator to fix issues with the moving animation for the enemies and the turn base conditions of the game
    {
        m_animator.SetBool("Walking", true);
        var gm = GameManager.Instance;
        if (gm == null || gm.boardManager == null || gm.playerController == null)
        {
            m_animator.SetBool("Walking", false);
            yield break;
        }

        var grid = gm.boardManager.GetComponentInChildren<Grid>();
        Vector2Int enemyPos = (Vector2Int)grid.WorldToCell(transform.position);
        Vector2Int playerPos = (Vector2Int)grid.WorldToCell(gm.playerController.transform.position);

        path = FindPlayer(enemyPos, playerPos);
        if (path == null || path.Count == 0)
            path = FindPlayerThroughWall(enemyPos, playerPos);

        if (path == null || path.Count == 0)
        {
            m_animator.SetBool("Walking", false);
            yield break;
        }

        Vector2Int nextCell = path[0];
        Vector3 nextWorldPos = gm.boardManager.CellToWorld(nextCell);
        var cellData = gm.boardManager.GetCellData(nextCell);
        var cellObject = cellData?.ContainedObject;

        if (cellObject != null)//I wonder if a switch would work better here rather than a big else if chain
        {
            if (cellObject.isWall)
            {
                EnemyAttack(enemyDamage);
                m_animator.SetBool("Walking", false);
                yield break;
            }
            else if (cellObject is ExitCellObject)
            {
                Destroy(gameObject);
                yield break;
            }
            else if (cellObject is FoodObject)
            {
                cellObject.PlayerEntered();
                if (cellData != null) cellData.ContainedObject = null;
            }
            else if (cellObject is EnemyObject)
            {
                m_animator.SetBool("Walking", false); //keep these guys from bumping into eachother
                yield break;
            }
        }

        float elapsed = 0f;
        float duration = 0.065f;
        Vector3 startPos = transform.position;
        audioController?.PlayStep();

        while (elapsed < duration)
        {
            if (gameObject == null) yield break;
            transform.position = Vector3.Lerp(startPos, nextWorldPos, elapsed / duration);
            elapsed += Time.deltaTime * moveSpeed;
            yield return null;
        }

        transform.position = nextWorldPos;
        if (cellData != null) cellData.ContainedObject = this;
        m_animator.SetBool("Walking", false);
    }

    private List<Vector2Int> FindPlayer(Vector2Int enemyPosition, Vector2Int playerPosition) //classic adjacency matrix grid BFS search
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.boardManager == null) return null; //you might notice alot of null checks in the enemy script and
                                                                //its because turn manager calls the enemies turn method through advance turn
        var queue = new Queue<Vector2Int>();                    //and sometimes these guys have a habbit of still being called by turn manager
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(); //even if the player killed them already.
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(enemyPosition);
        visited.Add(enemyPosition);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == playerPosition) break;

            foreach (var direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                var next = current + direction;
                var cell = gm.boardManager.GetCellData(next);
                if (cell == null || visited.Contains(next)) continue;
                if (cell.ContainedObject != null)
                {
                    if (!cell.isPassable || cell.ContainedObject.isWall) continue;
                }
                else
                {
                    if (!cell.isPassable) continue;
                }

                visited.Add(next);
                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(playerPosition))
            return null;

        var path = new List<Vector2Int>();
        var node = playerPosition;
        while (node != enemyPosition)
        {
            path.Add(node);
            node = cameFrom[node];
        }
        path.Reverse();
        return path;
    }

    private List<Vector2Int> FindPlayerThroughWall(Vector2Int enemyPosition, Vector2Int playerPosition)
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.boardManager == null) return null;

        var queue = new Queue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(enemyPosition);
        visited.Add(enemyPosition);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == playerPosition) break;
                                                    //same thing as above escept we ignore walls here
            foreach (var direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                var next = current + direction;
                var cell = gm.boardManager.GetCellData(next);
                if (cell == null || !cell.isPassable || visited.Contains(next)) continue;
                visited.Add(next);
                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(playerPosition)) return null;

        var path = new List<Vector2Int>();
        var node = playerPosition;
        while (node != enemyPosition)
        {
            path.Add(node);
            node = cameFrom[node];
        }
        path.Reverse();
        return path;
    }

    private bool IsPlayerAdjacent()
    {
        var grid = GameManager.Instance.boardManager.GetComponentInChildren<Grid>();
        Vector2Int enemyPos = (Vector2Int)grid.WorldToCell(transform.position);
        Vector2Int playerPos = (Vector2Int)grid.WorldToCell(GameManager.Instance.playerController.transform.position);
        int dx = playerPos.x - enemyPos.x;

        if (dx < 0)
            m_spriteRenderer.flipX = false;
        else if (dx > 0)
            m_spriteRenderer.flipX = true;

        if ((enemyPos.x - 1 == playerPos.x || enemyPos.x + 1 == playerPos.x) && (enemyPos.y == playerPos.y))
            return true;
        else
            return false;
    }

    public void TakeDamage(int amount)
    {
        audioController?.PlayHit();
        CurrentHealth -= amount;
        healthBar.SetValue(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            audioController?.PlayDeath();
            Destroy(healthBar.gameObject);
            Destroy(gameObject);
        }
    }

    public void EnemyAttack(int amount)
    {
        m_animator?.SetTrigger("Attack");
        var gm = GameManager.Instance;
        if (gm == null || gm.playerController == null) return;
        if (IsPlayerAdjacent())
        {
            gm.playerController.audioController.PlayHit();
            gm.playerController.animator.SetTrigger("Hit");
            gm.ChangeFood(-enemyDamage);
        }
        else
        {
            // try to hit any adjacent damageable walls
            var grid = gm.boardManager?.GetComponentInChildren<Grid>();
            if (grid == null) return;
            Vector2Int enemyPos = (Vector2Int)grid.WorldToCell(transform.position);
            foreach (var dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {                                   //similar to the find player method above we use a foreach loop just to check each direction around the enemy 
                var current = enemyPos + dir; 
                var cell = gm.boardManager.GetCellData(current);
                if (cell == null || cell.ContainedObject == null) continue;
                if (cell.ContainedObject is IDamageable damageable && !ReferenceEquals(damageable, this))
                {
                    audioController?.PlayAttack();
                    damageable.TakeDamage(2);
                    return;
                }
            }
        }
    }
}
    