using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof(JsonParseDeck) )]
public class Deck : MonoBehaviour
{
    [Header("Inscribed")]
    public CardSpritesSO cardSprites;
    public GameObject prefabCard;
    public GameObject prefabSprite;
    public bool startFaceUp = true;

    [Header("Dynamic")]
    public Transform deckAnchor;
    public List<Card> cards;

    private JsonParseDeck jsonDeck;

    static public GameObject SPRITE_PREFAB { get; private set; }

    /*
    void Start()
    {
        InitDeck();
        Shuffle(ref cards);
    }
    */

    public void InitDeck()
    {
        //Create a static reference to spritePrefab for the Card class to use
        SPRITE_PREFAB = prefabSprite;

        //call init method on the CardSpriteSO instance assigned to cardSprites
        cardSprites.Init();

        //Get a reference to the JsonParseDeck component
        jsonDeck = GetComponent<JsonParseDeck>();

        //Create an anchor for all the Card GameObjects in the Hierarchy
        if ( GameObject.Find("_Deck") == null )
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        MakeCards();
    }

    void MakeCards()
    {
        cards = new List<Card>();
        Card c;

        //Generate 13 cards for each suit
        string suits = "CDHS";
        for ( int i = 0; i < 4; i++ )
        {
            for( int j = 1; j <= 13; j++ )
            {
                c = MakeCard(suits[i], j);
                cards.Add(c);

                //this aligns the cards in nice rows for testing
                c.transform.position = new Vector3((j - 7) * 3, (i - 1.5f) * 4, 0);
            }
        }
    }

    //Creates a card GameObject based on suit and rank
    //Note that this method assumes it will be passed a valid suit and rank

    Card MakeCard( char suit, int rank )
    {
        GameObject go = Instantiate<GameObject>(prefabCard, deckAnchor);

        Card card = go.GetComponent<Card>();

        card.Init( suit, rank, startFaceUp);
        return card;
    }

    //Shuffle a List(Card) and return the result to the original list
    static public void Shuffle(ref List<Card> refCards)
    {
        //Create a temporary list to hold the index of the card to be moved
        List<Card> tCards = new List<Card>();

        int ndx; // this will hold the index of the card
        //Repeat as long as there are cards in the original list
        while (refCards.Count > 0)
        {
            //Pick the index of a random card
            ndx = Random.Range(0, refCards.Count);
            //Add that card to the temporary List
            tCards.Add(refCards[ndx]);
            //And remove that card from the original list
            refCards.RemoveAt(ndx);
        }

        //Replace the original list with the temporary list
        refCards = tCards;
    }
}
