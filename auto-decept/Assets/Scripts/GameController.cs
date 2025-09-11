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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip audioClip;
    public AudioClip matchClip;
    public AudioClip flipClip;
    public AudioClip mismatchClip;
    public AudioClip gameOverClip;

    private List<CardView> cards = new List<CardView>();
    private List<CardView> flippedCards = new List<CardView>();

    private int score = 0;
    private int combo = 0;
    private int seed;
    private List<int> deckOrder = new List<int>();
    private HashSet<int> matched = new HashSet<int>();

    void Start()
    {
        newGameButton?.onClick.AddListener(NewGame);
        layoutButton?.onClick.AddListener(ChangeLayout);

        //Try to load, if not start new

        if (GameBoot.RequestedMode == GameBoot.Mode.LoadGame && TryLoad())
        {
            BuildBoard();
        }
        else
        {
            NewGame();
        }

        UpdateUI();
    }

    void NewGame()
    {
        ClearBoard();

        score = 0;
        combo = 0;
        matched.Clear();

        //Creat a shuffled deck

        var layout = boardConfig.Current;
        int total = layout.rows * layout.cols;
        if (total % 2 != 0) total--; //Makes even

        deckOrder.Clear();
        for (int i = 0; i < total / 2; i++) // fix - pairs, not total
        {
            deckOrder.Add(i);
            deckOrder.Add(i);
        }

        //Simple shuffling

        for(int i = 0; i < deckOrder.Count; i++)
        {
            int j = Random.Range(i, deckOrder.Count);
            int temp = deckOrder[i];
            deckOrder[i] = deckOrder[j];
            deckOrder[j] = temp;
        }

        seed = Random.Range(0, 1000000); //This for the save system
        BuildBoard();
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
            GameObject cardObj = Instantiate(cardPrefab, responsiveGrid.transform);
            CardView card = cardObj.GetComponent<CardView>();

            int cardID = deckOrder[i];
            char letter = (char)('A' + (cardID % 26)); // fix - modulo

            card.Setup(cardID, letter, OnCardClicked);

            //Restore state if it matches

            if (matched.Contains(cardID))
            {
                card.SetFaceInstant(true); // fix - shows face immeadiatley
                card.SetMatched();
            }

            cards.Add(card);
        }

    }
    private bool previewActive = false;
    void OnCardClicked(CardView card)

    {
        if (card.isMatched || previewActive) return;

        PlaySound(flipClip);

        bool willReveal = !card.isRevealed;
        card.Flip();

        if (willReveal) flippedCards.Add(card);

        else flippedCards.Remove(card);

        //Checking for matches when multiple cards are present

        if(flippedCards.Count >= 2)
        {
            CheckMatches();
        }
    }

    private IEnumerator PreviewThenHideAll(float seconds = 5f)
    {
        previewActive = true;

        //To reveal all of it

        for (int i = 0; i < cards.Count; i++)
        {
            if (!cards[i].isMatched) cards[i].SetFaceInstant(true);
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

    void CheckMatches()
    {

        //Finding any matching pair

        for(int i =0; i < flippedCards.Count - 1; i++)
        {
            for(int j = i + 1; j < flippedCards.Count; j++) // fix j=i+1
            {
                var card1 = flippedCards[i];
                var card2 = flippedCards[j];

                if (card1.cardID == card2.cardID)
                {

                    //When Match is found

                    card1.SetMatched();
                    card2.SetMatched();
                    matched.Add(card1.cardID);

                    flippedCards.Remove(card1);
                    flippedCards.Remove(card2);

                    combo++;
                    int mult = 1 + (combo / 3);
                    score += 100 * mult;

                    PlaySound(matchClip);
                    UpdateUI();
                    Save();
                    CheckWin();
                    return;
                }
            }
        }

        // When no matches - handling third card 

        if (flippedCards.Count > 2)
        {
            var oldestCard = flippedCards[0];
            flippedCards.RemoveAt(0);

            if (oldestCard.isRevealed)
                oldestCard.Flip();

            combo = 0;
            score = Mathf.Max(0, score - 10);

            PlaySound(mismatchClip);
            UpdateUI();
            Save();
        }
    }

    void CheckWin()
    {
        var layout = boardConfig.Current;
        int totalPairs = (layout.rows * layout.cols) / 2;

        if (matched.Count >= totalPairs)
        {
            PlaySound(gameOverClip);
            Debug.Log($"You Win! Final Score : {score}");
        }
    }

    void UpdateUI()
    {

        scoreText.text = $"Score : {score}";
        comboText.text = $"Combo : {combo}";
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
