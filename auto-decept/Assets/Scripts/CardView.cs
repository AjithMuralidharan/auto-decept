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

    public void Setup(int id, char cardLetter, System.Action<CardView> clickCallback)
    {
        cardID = id;
        letter = cardLetter;
        onClicked = clickCallback;

        textFace.text = cardLetter.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMatched) return;
        onClicked?.Invoke(this);
    }

    public void Flip()
    {
        if (isMatched) return;
        StartCoroutine(FlipAnimation());
    }

    public void SetMatched()
    {
        isMatched = true;
        GetComponent<Button>().interactable = false; 
    }

    private void ShowBack()
    {
        textFace.gameObject.SetActive(false);
        imageBack.gameObject.SetActive(true);
        isRevealed = false;
    }

    private void ShowFace()
    {
        textFace.gameObject.SetActive(true);
        textFace.gameObject.SetActive(false);
        isRevealed = true;
    }

    private IEnumerator FlipAnimation()
    {
        //Squash it to center

        for (float t = 0; t < 1; t += Time.deltaTime * 8f)
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
    }
}
