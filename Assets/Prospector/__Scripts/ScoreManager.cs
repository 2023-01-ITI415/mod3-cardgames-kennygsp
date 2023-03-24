using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//An enum to handle all the possible scoring events
//for gold and silver cards, you would add mineGold and mineSilver to this enum
public enum eScoreEvent{
    draw,
    mine,
    gameWin,
    gameLoss
}

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int SCORE_THIS_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Inscribed")]
    [Tooltip("If true, then score events are logged to the Console")]
    public bool logScoreEvents = true;

    [Header("Dynamic")]
    //Fields to track score info
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    [Header("Check this box to reset the ProspectorHighScore to 100")]
    public bool checkToResetHighScore = false;

    void Awake() {
        S = this;   //Set the private singleton

        //Check for a high score in PlayerPrefs
        if (PlayerPrefs.HasKey ("ProspectorHighScore")){
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        //Add the score from last round, which will not be 0 if it was a win
        score += SCORE_FROM_PREV_ROUND;

        //And reset it to 0
        SCORE_THIS_ROUND = 0;
    }

    static public void TALLY(eScoreEvent evt) {
        S.Tally(evt);
    }

    ///<summary>
    ///Handle eScoreEvents (mostly sent by the Prospector class)
    ///</summary>
    void Tally( eScoreEvent evt ){
        switch( evt ) {
            //When a mine card is clicked
            case eScoreEvent.mine:      //remove a mine card
                chain++;                    //increase the score chain
                scoreRun += chain;          //add score for this card to run
                break;
            
            //These same things need to happen whether its a draw, win, or loss
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;
        }

        string scoreStr = score.ToString( "#,##0" );
        //This second switch statement handles round wins and losses
        switch( evt ) {
            case eScoreEvent.gameWin:
                SCORE_THIS_ROUND = score - SCORE_FROM_PREV_ROUND;

                //if its a win add the score to the next round
                SCORE_FROM_PREV_ROUND = score;

                //if its higher than the high score update it
                if( HIGH_SCORE <= score ){
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }  
                break;
            
            case eScoreEvent.gameLoss:
                //if its a loss check the high score
                if(HIGH_SCORE <= score) {
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                } else {
                    Log($"Game Over. Your finalk score was: {scoreStr}");
                }

                SCORE_FROM_PREV_ROUND = 0;
                break;
            
            default:
                Log($"score:{scoreStr} scoreRun:{scoreRun} chain:{chain}");
                break;
        }
        
    }

    void Log(string str) {
        if(logScoreEvents) Debug.Log(str);
    }

    void OnDrawGizmos() {
        if ( checkToResetHighScore){
            checkToResetHighScore = false;
            PlayerPrefs.SetInt("ProspectorHighScore", 100);
            Debug.LogWarning("PlayerPrefs.ProspectorHighScore reset to 100");
        }
    }

    static public int CHAIN {get{return S.chain;}}
    static public int SCORE {get{return S.score;}}
    static public int SCORE_RUN {get{return S.scoreRun;}}
}
