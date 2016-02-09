using UnityEngine;

namespace Assets.Examples.Breakout
{
    public class Brick : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            Destroy(gameObject);
        }
    }
}