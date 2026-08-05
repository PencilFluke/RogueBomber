using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private GameObject player;
    private TMP_Text gameOverText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = GameObject.Find("Player");
        gameOverText = GetComponentInChildren<TMP_Text>();
        gameOverText.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (!player)
        {
            gameOverText.gameObject.SetActive(true);
        }
    }
}
