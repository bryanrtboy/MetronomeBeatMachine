using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beat : MonoBehaviour
{

}

public class PatternSet
{
    public string soundBank = "Chimes";
    public double tempo = 144;
    public int signatureHi = 4;
    public int measures = 4;
    public List<BeatPattern> patterns = new List<BeatPattern>();
}

[System.Serializable]
public class BeatPattern
{
    //public GameObject note;
    public int m_beatID = 0;
    public List<bool> m_beatPattern = new List<bool>();
}
