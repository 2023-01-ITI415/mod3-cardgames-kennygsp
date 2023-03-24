using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Dynamic")]
    public char suit;
    public int rank;
    public Color color = Color.black;
    public string colS = "Black";
    public GameObject back;
    public JsonCard def;

    //this list holds all of the decorator GameObject
    public List<GameObject> decoGOs = new List<GameObject>();

    //this list holds all of the pip GameObjects
    public List<GameObject> pipGOs = new List<GameObject>();

    public void Init( char eSuit, int eRank, bool startFaceUp = true)
    {
        //assign basc values to the card
        gameObject.name = name = eSuit.ToString() + eRank;
        suit = eSuit;
        rank = eRank;
        //if this is a diamond or Heart, change the default black color to red
        if ( suit == 'D' || suit == 'H' )
        {
            colS = "Red";
            color = Color.red;
        }

        def = JsonParseDeck.GET_CARD_DEF(rank);

        //Build the card from Sprites
        AddDecorators();
        AddPips();
        AddFace();
        AddBack();
        faceUp = startFaceUp;
    }

    public virtual void SetLocalPos(Vector3 v)
    {
        transform.localPosition = v;
    }

    //These private variables that will be reused several times
    private Sprite _tSprite = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSRend = null;
    //An Euler rotation of 180 around the Z axis will flip sprites upside down
    private Quaternion _flipRot = Quaternion.Euler(0, 0, 180);

    //Adds the decrators to the top left and bottom right of each card
    private void AddDecorators()
    {
        //Add Decorators
        foreach (JsonPip pip in JsonParseDeck.DECORATORS)
        {
            if ( pip.type == "suit" )
            {
                //Instantiate a Sprite GameObject
                _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
                //Get the SpriteRenderer Component
                _tSRend = _tGO.GetComponent<SpriteRenderer>();
                //Get the suit Sprite from the CardSpritesSO.SUIT static field
                _tSRend.sprite = CardSpritesSO.SUITS[suit];
            } else
            {
                _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
                _tSRend = _tGO.GetComponent<SpriteRenderer>();
                //Get the rank Sprite from the CardSpritesSO.RANK static field
                _tSRend.sprite = CardSpritesSO.RANKS[rank];
                //Set the color of the rank to match the suit
                _tSRend.color = color;
            }

            //Make the Decorator Sprites render above the card
            _tSRend.sortingOrder = 1;
            //Set the localPosition based on the location from DeckXML
            _tGO.transform.localPosition = pip.loc;
            //Flip the decorator if needed
            if (pip.flip) _tGO.transform.rotation = _flipRot;
            //Set the scale to keep the decorators from being too big
            if ( pip.scale != 1 )
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }

            //Name this GameObject so its easy to find in the Hierarchy
            _tGO.name = pip.type;

            //Add this decorator GameObject to the List card.decoGOs
            decoGOs.Add(_tGO);
        }
    }

    private void AddPips()
    {
        int pipNum = 0;

        // For each of the pips in the definition
        foreach (JsonPip pip in def.pips)
        {
            //Instantiate a GameObject from the Deck.SPRITE_PREFAB static field
            _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);

            //Set the position to that specified in the XML
            _tGO.transform.localPosition = pip.loc;

            //Flip if necessary
            if (pip.flip) _tGO.transform.rotation = _flipRot;

            //Scale if necessary (only for ACE)
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }

            //Give this GameObject a name
            _tGO.name = "pip" + pipNum++;

            //Get the SpriteRenderer Component
            _tSRend = _tGO.GetComponent<SpriteRenderer>();

            // Set the Sprite to the proper suit
            _tSRend.sprite = CardSpritesSO.SUITS[suit];

            //sortingOrder = 1 renders this pip above the Card_Front
            _tSRend.sortingOrder = 1;

            //Add this tp the Card's list of pips
            pipGOs.Add(_tGO);
        }
    }

    private void AddFace()
    {
        if (def.face == "")
            return; // no need to run if this isnt a face card

        //Find a face sprite in CardSpritesSO with the right name
        string faceName = def.face + suit;
        _tSprite = CardSpritesSO.GET_FACE(faceName);
        if (_tSprite == null)
        {
            Debug.LogError("Face Sprite " + faceName + " not found");
            return;
        }

        _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
        _tSRend = _tGO.GetComponent<SpriteRenderer>();
        _tSRend.sprite = _tSprite;  //Assign the face sprite to tsrend
        _tSRend.sortingOrder = 1;   //set the sortingorder
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = faceName;
    }

    public bool faceUp
    {
        get { return (!back.activeSelf); }
        set { back.SetActive(!value); }
    }

    //Adds a back to the card so that renders on top of everything else
    private void AddBack()
    {
        _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
        _tSRend = _tGO.GetComponent<SpriteRenderer>();
        _tSRend.sprite = CardSpritesSO.BACK;
        _tGO.transform.localPosition = Vector3.zero;

        //2 is a higher sorting order than anything else
        _tSRend.sortingOrder = 2;
        _tGO.name = "back";
        back = _tGO;
    }

    private SpriteRenderer[] spriteRenderers;

    //Gather all SpriteRenderers on this and its children into an array
    void PopulateSpriteRenderers() {
        //If we've already populated spriteRenderers, just return
        if (spriteRenderers != null) return;
        
        //Get Componen is slow but we're only doing it once per card
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    //Moves the Sprites of this Card into a specified sorting layer
    public void SetSpriteSortingLayer(string layerName) {
        PopulateSpriteRenderers();

        foreach( SpriteRenderer srend in spriteRenderers) {
            srend.sortingLayerName = layerName;
        }
    }

    //Sets the sortingOrder of the Sprites on this Card. This allows
    //Mutliple Cards to be in the same sorting layer and still overlap properly, and
    //it is used by both draw and discard piles
    public void SetSortingOrder(int sOrd) {
        PopulateSpriteRenderers();

        foreach(SpriteRenderer srend in spriteRenderers) {
            if(srend.gameObject == this.gameObject) {
                //If the gameObject is this.gameObject its the card face
                srend.sortingOrder = sOrd;  //Set its order to sOrd
            } else if (srend.gameObject.name == "back") {
                srend.sortingOrder = sOrd + 2;
            } else {
                srend.sortingOrder = sOrd + 1;
            }
        }
    }

    //Virtual methods can be overridden by subclass methods with the same name
    virtual public void OnMouseUpAsButton() {
        print(name);
    }

    public bool AdjacentTo(Card otherCard, bool wrap = true) {
        //if either card is face down its not a valid match
        if(!faceUp || !otherCard.faceUp) return (false);

        //if the ranks are 1 apart, they are adjacent
        if(Mathf.Abs(rank - otherCard.rank) == 1) return (true);

        if(wrap) {
            if (rank == 1 && otherCard.rank == 13) return(true);
            if (rank == 13 && otherCard.rank == 1) return(true);
        }

        return (false);
    }
}
