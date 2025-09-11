using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public TextMeshProUGUI textFace;
    public Image imageBack;

    public int cardID;
    public char letter;
    public bool isRevealed;
    public bool isMatched;

    private System.Action<CardView> onClicked;
    private bool isAnimating;

    public void Setup(int id, char cardLetter, System.Action<CardView> clickCallback)
    {
        cardID = id;
        letter = cardLetter;
        onClicked = clickCallback;

        if (textFace != null)
            textFace.text = cardLetter.ToString();

        SetFaceInstant(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMatched || isAnimating) return;
        onClicked?.Invoke(this);
    }

    public void SetFaceInstant(bool faceUp)
    {
        if (textFace != null) textFace.gameObject.SetActive(faceUp);
        if (imageBack != null) imageBack.gameObject.SetActive(!faceUp);
        isRevealed = faceUp;
    }

    public void Flip()
    {
        if (isMatched || isAnimating ) return;
        StartCoroutine(FlipAnimation());
    }

    public void SetMatched()
    {
        isMatched = true;

        //disabling interaction on match : fix

        var btn = GetComponent<Button>();
        if (btn != null) btn.interactable = false;
    }

    private void ShowBack()
    {
        if (textFace != null) textFace.gameObject.SetActive(false);
        if (imageBack != null) imageBack.gameObject.SetActive(true);
        isRevealed = false;
    }

    private void ShowFace()
    {
        if (textFace != null) textFace.gameObject.SetActive(true);
        if (imageBack != null) imageBack.gameObject.SetActive(false); // was textface, made cards hide face twice
        isRevealed = true;
    }

    private IEnumerator FlipAnimation()
    {
        //Squash it to center

        for (float t = 0; t < 1f; t += Time.deltaTime * 8f)
        {
            float scale = 1f - t;
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        // Switch the face

        if (isRevealed) ShowBack();
        else ShowFace();

        //Expanding back to normal

        for (float t = 0; t < 1; t += Time.deltaTime * 8f)
        {
            transform.localScale = new Vector3(t, 1f, 1f);
            yield return null;
        }

        transform.localScale = Vector3.one;
        isAnimating = false;
    }

}
