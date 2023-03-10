using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Stores information about the layout of the Prospector mine
[System.Serializable]
public class JsonLayout
{
    public Vector2 multiplier;
    public List<JsonLayoutSlot> slots;
    public JsonLayoutPile drawPile, discardPile;

}

//Stores information for each slot in the layout\
[System.Serializable]
public class JsonLayoutSlot : ISerializationCallbackReceiver
{
    public int id;
    public int x;
    public int y;
    public bool faceUp;
    public string layer;
    public string hiddenByString;

    [System.NonSerialized]
    public List<int> hiddenBy;

    //pulls data from hiddenByString and places it into the hiddenBy list
    public void OnAfterDeserialize()
    {
        hiddenBy = new List<int>();
        if (hiddenByString.Length == 0) return;

        string[] bits = hiddenByString.Split(",");
        for( int i = 0; i < bits.Length; i++)
        {
            hiddenBy.Add(int.Parse(bits[i]));
        }
    }

    //Required by ISerializationCallbackReceiver, but empty in this class
    public void OnBeforeSerialize() { }
}

//Stores the information for the draw and discard piles
[System.Serializable]
public class JsonLayoutPile
{
    public int x, y;
    public string layer;
    public float xStagger; //xStaggers fans card to the side for the draw pile
}

public class JsonParseLayout : MonoBehaviour
{
    public static JsonParseLayout S { get; private set; }

    [Header("Inscribed")]
    public TextAsset jsonLayoutFile;

    [Header("Dynamic")]
    public JsonLayout layout;
    
    void Awake()
    {
        layout = JsonUtility.FromJson<JsonLayout>(jsonLayoutFile.text);
        S = this;
    }
}