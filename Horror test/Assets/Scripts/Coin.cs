using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public int scoreValue = 100;
    public AudioClip pickupSound;

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectCoin(other.GetComponent<PlayerController>());
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            CollectCoin(hit.gameObject.GetComponent<PlayerController>());
        }
    }

    private void CollectCoin(PlayerController player)
    {
        if (player != null)
        {
            player.AddScore(scoreValue);

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}
