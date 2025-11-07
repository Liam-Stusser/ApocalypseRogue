using UnityEngine;
using System.Collections;

public class TurnManager
{
    public int currentTurn;
    public event System.Action OnTick;           
    public event System.Action<int> OnTurnAdvanced; 

    public TurnManager()
    {
        currentTurn = 0;
    }

    public void NextTurn()
    {
        currentTurn++;
        OnTick?.Invoke();               
        OnTurnAdvanced?.Invoke(currentTurn); 
        Debug.Log($"Turn advanced to {currentTurn}");
    }
}
