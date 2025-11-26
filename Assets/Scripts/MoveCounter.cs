using UnityEngine;
using TMPro;

public class MoveCounter : MonoBehaviour
{
    public static MoveCounter Instance { get; private set; }

    public TMP_Text movesText;   // your existing UI text
    private int moves = 0;

    public int CurrentMoves => moves;   //  ADD THIS PROPERTY

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        UpdateText();
    }

    public void AddMove()
    {
        moves++;
        UpdateText();
    }

    public void ResetMoves()
    {
        moves = 0;
        UpdateText();
    }

    private void UpdateText()
    {
        if (movesText != null)
            movesText.text = "" + moves;
    }
}
