using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    public GameZygote gameZygote;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>())
        {
            Player p = collision.gameObject.GetComponent<Player>();
            foreach (KeyValuePair<int, Player> entry in gameZygote.ingame_characters[p.team_id])
            {
                entry.Value.is_dead = true;
            }
        }
    }
}
