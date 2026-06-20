using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CreditsData", menuName = "Tower Defense/Credits Data")]
public class CreditsData : ScriptableObject
{
    [System.Serializable]
    public class CreditEntry
    {
        public string role;
        public string name;
    }

    [System.Serializable]
    public class CreditSection
    {
        public string sectionTitle;
        public List<CreditEntry> entries = new();
    }

    public List<CreditSection> sections = new();
}