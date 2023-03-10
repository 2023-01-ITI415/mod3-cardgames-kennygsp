using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(JsonParseLayout))]
public class Prospector : MonoBehaviour
{
    private static Prospector S;

    [Header("Dynamic")]
    public List<CardProspector> drawPile;
    public List<CardProspector> discardPile;
    public List<CardProspector> mine;
    public CardProspector target;

    private Transform layoutAnchor;
    private Deck deck;
    private JsonLayout jsonLayout;

    //A Dictionary to pair mine layout IDs and actual Cards
    private Dictionary<int, CardProspector> mineIdToCardDict;

    void Start() {
        //Set the private Singleton
        if( S != null ) Debug.LogError("Attempted to set S more than once");
        S = this;

        jsonLayout = GetComponent<JsonParseLayout>().layout;

        deck = GetComponent<Deck>();
        //These two lines replace the Start() call
        deck.InitDeck();
        Deck.Shuffle(ref deck.cards);

        drawPile = ConvertCardsToCardProspectors(deck.cards);

        LayoutMine();

        //Set up the initial target card
        MoveToTarget( Draw () );

        //Set up the draw pile
        UpdateDrawPile();
    }

    List<CardProspector> ConvertCardsToCardProspectors(List<Card> listCard) {
        List<CardProspector> listCP = new List<CardProspector>();
        CardProspector cp;
        foreach ( Card card in listCard ) {
            cp = card as CardProspector;
            listCP.Add(cp);
        }
        return(listCP);
    }
    // Pulls a single card from the beginning of the drawPile and returns it
    CardProspector Draw() {
        CardProspector cp = drawPile[0];    //Pull the 0th CardProspector
        drawPile.RemoveAt(0);
        return(cp);
    }

    //Positions the initial; tableau of cards
    void LayoutMine() {
        //Create an empty GameObject to serve as an anchor for the tableau
        if(layoutAnchor == null) {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
        }

        CardProspector cp;

        mineIdToCardDict = new Dictionary<int, CardProspector>();

        //Iterate through the JsonLayoutSlots pulled from the JSON_Layout
        foreach(JsonLayoutSlot slot in jsonLayout.slots){
            cp = Draw();    //Pull a card from the top of the draw pile
            cp.faceUp = slot.faceUp;    //Set its faceUp to the vaule in SlotDef
            //Make the CardProspector a child of layoutAnchor
            cp.transform.SetParent(layoutAnchor);

            //Convert the last char of the layer string to an int
            int z = int.Parse(slot.layer[slot.layer.Length - 1].ToString());

            //Set the localPosition of the card based on the slot information
            cp.SetLocalPos( new Vector3 (
                jsonLayout.multiplier.x * slot.x,
                jsonLayout.multiplier.y * slot.y,
                -z ) );
            
            cp.layoutID = slot.id;
            cp.layoutSlot = slot;
            //CardProspectors in the mine have the state CardState.mine
            cp.state = eCardState.mine;

            cp.SetSpriteSortingLayer(slot.layer);

            mine.Add(cp);

            mineIdToCardDict.Add(slot.id, cp);
        }
    }

    //Moves the current target card to the discardPile
    void MoveToDiscard(CardProspector cp) {
        //Set the state of the card to discard
        cp.state = eCardState.discard;
        discardPile.Add(cp);
        cp.transform.SetParent(layoutAnchor);

        //Position it on the discardPile
        cp.SetLocalPos(new Vector3 (
            jsonLayout.multiplier.x * jsonLayout.discardPile.x,
            jsonLayout.multiplier.y * jsonLayout.discardPile.y,
            0));

        cp.faceUp = true;

        //Place it on top of the pile for depth sorting
        cp.SetSpriteSortingLayer(jsonLayout.discardPile.layer);
        cp.SetSortingOrder(-200 + (discardPile.Count * 3));
    }

    //Make cp the new target card
    void MoveToTarget(CardProspector cp) {
        //if there is currently a target card, move it to discardPile
        if(target != null) MoveToDiscard(target);

        //Use MoveToDiscard to move the target card to the correct location
        MoveToDiscard(cp);

        //Set a few additional things to make cp the new target
        target = cp;
        cp.state = eCardState.target;

        //Set the depth sorting so that cp is on top of the discardPile
        cp.SetSpriteSortingLayer("Target");
        cp.SetSortingOrder(0);
    }

    //Arrange all the cards of the drawPile to show how many are left
    void UpdateDrawPile(){
        CardProspector cp;
        //Go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++) {
            cp = drawPile[i];
            cp.transform.SetParent(layoutAnchor);

            //Position it correctly with the layout.drawPile.stagger
            Vector3 cpPos = new Vector3();
            cpPos.x = jsonLayout.multiplier.x * jsonLayout.drawPile.x;
            //Add the staggering for the drawPile
            cpPos.x += jsonLayout.drawPile.xStagger * i;
            cpPos.y = jsonLayout.multiplier.y * jsonLayout.drawPile.y;
            cpPos.z = 0.1f * i;
            cp.SetLocalPos(cpPos);

            cp.faceUp = false;  //DrawPile Cards are all face down
            cp.state = eCardState.drawpile;
            //Set depth sorting
            cp.SetSpriteSortingLayer(jsonLayout.drawPile.layer);
            cp.SetSortingOrder(-10 * i);
        }
    }

    //This turns cards in the Mine faceup and facedown
    public void SetMineFaceUps() {
        CardProspector coverCP;
        foreach(CardProspector cp in mine) {
            bool faceUp = true;

            foreach(int coverID in cp.layoutSlot.hiddenBy) {
                coverCP = mineIdToCardDict[coverID];

                if(coverCP == null || coverCP.state == eCardState.mine){
                    faceUp = false;
                }
            }
            cp.faceUp = faceUp;
        }
    }

    void CheckForGameOver() {
        if ( mine.Count == 0 ) {
            GameOver(true);
            return;
        }

        //If there are still cards in the mine & draw pile, the games not over
        if(drawPile.Count > 0 ) return;

        //Check for remaining valid plays
        foreach( CardProspector cp in mine ){
            //if there is a valid play the games not over
            if(target.AdjacentTo(cp)) return;
        }

        //Since there are no valid plays the game is over
        GameOver(false);
    }

    void GameOver( bool won ) {
        if(won) {
            Debug.Log("Game Over. You Win!");
        } else {
            Debug.Log("Game Over. You Lost.");
        }

        CardSpritesSO.RESET();

        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    //Handler for any time a card in the game is clicked
    static public void CARD_CLICKED(CardProspector cp) {
        //The reaction is determined by the state of the clicked card'
        switch (cp.state) {
            case eCardState.target:
                //Clicking the target card does nothing
                break;
            
            case eCardState.drawpile:
                //clicking any card in the drawpile will draw the next card
                //call two methods on the Prospector Singleton S
                S.MoveToTarget( S.Draw());
                S.UpdateDrawPile();
                break;
            
            case eCardState.mine:
                bool validMatch = true;

                //If the card is face-down its not valid
                if(!cp.faceUp) validMatch = false;

                //If its not an adjacent rank, its not valid
                if(!cp.AdjacentTo(S.target)) validMatch = false;

                if(validMatch) {
                    S.mine.Remove(cp);
                    S.MoveToTarget(cp);

                    S.SetMineFaceUps();
                }
                break;
        }
        S.CheckForGameOver();
    }
}
