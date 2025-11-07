using UnityEngine;

public class FoodObject : CellObject
{
    public int foodAmount = 10;
    private AudioSource audioSource;
    public override void PlayerEntered()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
            AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);

        GameManager.Instance.ChangeFood(foodAmount);
        Destroy(gameObject);
    }
}
