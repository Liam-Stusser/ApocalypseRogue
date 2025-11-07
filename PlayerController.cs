using System.Runtime.Serialization;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    private BoardManager m_boardManager;
    public Animator animator;
    public CharacterAudio audioController;
    public SpriteRenderer playerSprite;
    private Vector2Int m_cellPosition; 
    [SerializeField] private float m_moveSpeed = 4.5f;
    [SerializeField] private float stunDuration = 0.5f;
    private float stepCooldown = 0.5f; 
    private float stepTimer = 0f;
    private bool hasActedThisFrame = false;

    public bool isStunned = false;
    private bool m_isGameOver = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void Spawn(BoardManager boardManager, Vector2Int cell) 
    { 
        m_boardManager = boardManager; 
        m_cellPosition = cell; 

        transform.position = m_boardManager.CellToWorld(cell); 
    }

    public void Init()
    {
        m_isGameOver = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        playerSprite = GetComponent<SpriteRenderer>();
        audioController = GetComponent<CharacterAudio>();
    } 
    // Update is called once per frame
    void Update() 
    {
        hasActedThisFrame = false; 

        if (m_isGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
                GameManager.Instance.StartNewGame();
            return;
        }

        if (!hasActedThisFrame && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            audioController.PlayAttack();
            playerAttack();
            hasActedThisFrame = true;
        }

        if (!hasActedThisFrame)
            MovePlayer();
    } 

    void MovePlayer() 
    {
        if (isStunned)
            return;

        BoardManager.CellData currentCell = m_boardManager.GetCellDataAtWorldPosition(transform.position); 
        float horizontal = Input.GetAxisRaw("Horizontal"); 
        float vertical = Input.GetAxisRaw("Vertical"); 
        float moveAmount = Mathf.Abs(horizontal) + Mathf.Abs(vertical); //for animator movement trigger

        if (horizontal == 0 && vertical == 0)
        {
            animator.SetFloat("Moving", 0);
            return;
        }

        if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
            vertical = 0f;
        else
            horizontal = 0f;

        Vector3 movement = new Vector3(horizontal, vertical, 0f).normalized * m_moveSpeed * Time.deltaTime; 
        Vector3 targetPos = transform.position + movement; 
        BoardManager.CellData targetCell = m_boardManager.GetCellDataAtWorldPosition(targetPos); 

        if (targetCell.ContainedObject != null) 
        { 
            if (targetCell != null && targetCell.isPassable && targetCell.ContainedObject.PlayerWantsToEnter()) 
            {
                //animation handeling
                if (horizontal != 0)
                    playerSprite.flipX = horizontal < 0;
                animator.SetFloat("Moving", moveAmount);

                //audio handeling
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    audioController.PlayStep();
                    stepTimer = stepCooldown;
                }

                //movement handeling
                transform.position = targetPos; 
                BoardManager.CellData newCell = m_boardManager.GetCellDataAtWorldPosition(transform.position);

                if (newCell != currentCell)
                {
                    currentCell = newCell;
                    GameManager.Instance.turnManager.NextTurn();
                    Debug.Log("Player called tick");
                }

                targetCell.ContainedObject.PlayerEntered(); 
            }
        } 

        else 
        { 
            if (targetCell != null && targetCell.isPassable) 
            {
                //animation handeling
                if (horizontal != 0)
                    playerSprite.flipX = horizontal < 0;
                animator.SetFloat("Moving", moveAmount);

                //audio handeling
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    audioController.PlayStep();
                    stepTimer = stepCooldown;
                }

                //movement handeling
                transform.position = targetPos; 
                BoardManager.CellData newCell = m_boardManager.GetCellDataAtWorldPosition(transform.position);
                
                if (newCell != currentCell)
                {
                    currentCell = newCell;
                    GameManager.Instance.turnManager.NextTurn();
                    Debug.Log("Player called tick");
                }
            } 
        }
            
            hasActedThisFrame = true;
    }

    public void playerAttack()
    {
        animator.SetTrigger("Attack");
        if (!playerSprite.flipX)
        {
            Vector3 targetPosition = new Vector3(transform.position.x + 1, transform.position.y, 0);
            BoardManager.CellData targetCell = m_boardManager.GetCellDataAtWorldPosition(targetPosition);

            if (targetCell?.ContainedObject is IDamageable damageable)
                damageable.TakeDamage(2);         
        }
        else
        {
            Vector3 targetPosition = new Vector3(transform.position.x - 1, transform.position.y, 0);
            BoardManager.CellData targetCell = m_boardManager.GetCellDataAtWorldPosition(targetPosition);

            if (targetCell?.ContainedObject is IDamageable damageable)
                damageable.TakeDamage(2);
        }
        GameManager.Instance.turnManager.NextTurn();
    }

    private IEnumerator StunPlayer()
    {
        isStunned = true;
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        GameManager.Instance.turnManager.NextTurn(); 
    }

    public void Stun()
    {
        if(!isStunned)
            StartCoroutine(StunPlayer());
    }

    public void GameOver()
    {
        m_isGameOver = true;
    }
}
