using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Game Setup")]
    public BoardConfig boardConfig;
    public GameObject cardPrefab;
    public ResponsiveGrid responsiveGrid;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public Button newGameButton;
    public Button layoutButton;
    public Button saveButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip audioClip;
    public AudioClip matchClip;
    public AudioClip flipClip;
    public AudioClip mismatchClip;
    public AudioClip gameOverClip;

    private List<CardView> cards = new List<CardView>();
    private List<CardView> flippedCards = new List<CardView>();
    private HashSet<int> matched = new HashSet<int>();
    private List<int> deckOrder = new List<int>();

    private int score = 0;
    private int combo = 0;
    private int seed;
    private bool previewActive = false;

    void Start()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(() => NewGame(false));
        if (layoutButton != null) layoutButton.onClick.AddListener(ChangeLayout);
        if (saveButton != null) saveButton.onClick.AddListener(Save);

        //Try to load, if not start new

        if (GameBoot.RequestedMode == GameBoot.Mode.LoadGame && TryLoad())
        {
            BuildBoard();
            UpdateUI();
        }
        else
        {
            NewGame(false);
        }

    }

    void NewGame(bool preserveScore = false)
    {
        ClearBoard();


        if (!preserveScore)
        {
            score = 0;
        }
        combo = 0;
        matched.Clear();

        //Creat a shuffled deck

        var layout = boardConfig.Current;
        int total = layout.rows * layout.cols;
        if ((total & 1) != 0) total--; //Makes even

        deckOrder.Clear();
        int pairs = total / 2;
        for (int i = 0; i < pairs; i++) // fix - pairs, not total
        {
            deckOrder.Add(i);
            deckOrder.Add(i);
        }

        //Simple shuffling

        for(int i = 0; i < deckOrder.Count; i++)
        {
            int j = Random.Range(i, deckOrder.Count);
            int tmp = deckOrder[i];
            deckOrder[i] = deckOrder[j];
            deckOrder[j] = tmp;
        }

        seed = Random.Range(0, 1000000); //This for the save system

        BuildBoard();
        UpdateUI();
        StartCoroutine(PreviewThenHideAll(5f)); // fix - forgot to add this earlier to preview cards
        Save();
    }

    void ChangeLayout()
    {
        boardConfig.Next();
        NewGame();
    }

    void BuildBoard()
    {

        ClearBoard();

        var layout = boardConfig.Current;
        responsiveGrid.Apply(layout.rows, layout.cols);

        for (int i = 0; i < deckOrder.Count; i++)
        {
            int cardID = deckOrder[i];

            if (matched.Contains(cardID))
                continue;

            GameObject cardObj = Instantiate(cardPrefab, responsiveGrid.transform);
            CardView card = cardObj.GetComponent<CardView>();

            char letter = (char)('A' + (cardID % 26));
            card.Setup(cardID, letter, OnCardClicked);

            card.SetFaceInstant(false);

            cards.Add(card);
        }

    }

    void ResolveTwoOpen(CardView a, CardView b)
    {
        if (a == null || b == null) 
        { 
            flippedCards.Clear(); 
            return; 
        }

        if (a == b)
        {
            return; //Same instance when clicked twice, does nothing
        }

        if (a.cardID == b.cardID && a != b)
        {
            a.SetMatched();
            b.SetMatched();
            matched.Add(a.cardID); //For Match

            flippedCards.Remove(a);
            flippedCards.Remove(b); 

            combo++;
            int mult = 1 + (combo / 3);
            score += 100 * mult;

            PlaySound(matchClip);
            UpdateUI();
            Save();

            StartCoroutine(DestroyMatchedAfterDelay(a, b, 2f));
        }

        else
        {
            flippedCards.Remove(a);
            flippedCards.Remove(b);

          
            combo = 0; 
            score = Mathf.Max(0, score - 10); //added penalty

            PlaySound(mismatchClip);
            UpdateUI();
            Save();

            StartCoroutine(FlipBackAfterDelay(a, b, 2f));
        }
    }

    private System.Collections.IEnumerator DestroyMatchedAfterDelay(CardView a, CardView b, float delayseconds)
    {
        yield return new WaitForSeconds(delayseconds);

        if (a != null) Destroy(a.gameObject);
        if (b != null) Destroy(b.gameObject);

        CheckWin();

        yield break;
    }

    private System.Collections.IEnumerator FlipBackAfterDelay(CardView a, CardView b, float delayseconds)
    {
        yield return new WaitForSeconds(delayseconds);

        if (a != null && a.isRevealed && !a.isMatched) a.Flip();
        if (b != null && b.isRevealed && !b.isMatched) b.Flip();

        yield break;
    }

    
    void OnCardClicked(CardView card)

    {
        if (previewActive || card.isMatched) return;

        PlaySound(flipClip);

        bool willReveal = !card.isRevealed;
        card.Flip();

        if (willReveal)
        {

            if (!flippedCards.Contains(card))
                flippedCards.Add(card);

            if (flippedCards.Count == 2)
            {
                ResolveTwoOpen(flippedCards[0], flippedCards[1]);
            }
        }
        else
        {
            flippedCards.Remove(card);
        }
        
    }

    private IEnumerator PreviewThenHideAll(float seconds = 5f)
    {
        previewActive = true;

        //To reveal all of it

        for (int i = 0; i < cards.Count; i++)
        {
            if (!cards[i].isMatched) 
                cards[i].SetFaceInstant(true);
        }

        UpdateUI();

        yield return new WaitForSeconds(seconds);

        //Flip down animation

        for (int i = 0; i < cards.Count; i++)
        {
            var c = cards[i];
            if (!c.isMatched && c.isRevealed)
            {
                flippedCards.Remove(c); // keeps flipped cards in sync
                c.Flip();
            }
        }

        previewActive = false;
    }

    

    void CheckWin()
    {
        var layout = boardConfig.Current;
        int totalPairs = (layout.rows * layout.cols) / 2;

        if (matched.Count >= totalPairs)
        {
            PlaySound(gameOverClip);

            StartCoroutine(AdvanceToNextLayoutAfter(1.5f));

            Debug.Log($"You Win! Final Score : {score}");
        }
    }

    private void AdvanceToNextLayout()
    {
        boardConfig.Next();
        NewGame(preserveScore: true);
    }

    private System.Collections.IEnumerator AdvanceToNextLayoutAfter(float delayseconds)
    {
        yield return new WaitForSeconds(delayseconds);
        boardConfig.Next();
        NewGame(preserveScore: true); //fix: keep scores across layouts

    }

    void UpdateUI()
    {

        if (scoreText != null) scoreText.text = "Score: " + score;
        if (comboText != null) comboText.text = "Combo: " + combo + " (x" + (1 + (combo / 3)) + ")";
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }

    void Save()
    {
        SaveData data = new SaveData
        {
            layoutIndex = boardConfig.CurrentIndex,
            score = score,
            combo = combo,
            seed = seed,
            deckIds = new List<int>(deckOrder),
            matchedIds = new List<int>(matched)
        };

        SaveSystem.Save(data);
    }

    bool TryLoad()
    {
        SaveData data;
        if (!SaveSystem.TryLoad(out data)) return false;

        boardConfig.SetIndex(data.layoutIndex);
        score = data.score;
        combo = data.combo;
        seed = data.seed;
        deckOrder = new List<int>(data.deckIds);
        matched = new HashSet<int>(data.matchedIds);

        return true;
    }

    void ClearBoard()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (cards[i] != null)
            {
                Destroy(cards[i].gameObject);
            }
        }

        //Extra check

        if (responsiveGrid != null)
        {
            Transform container = responsiveGrid.transform;
            for(int i = container.childCount -1; i >= 0; i --)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        cards.Clear();
        flippedCards.Clear();
    }

}
