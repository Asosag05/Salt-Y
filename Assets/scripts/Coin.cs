using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Moneda recogida!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}