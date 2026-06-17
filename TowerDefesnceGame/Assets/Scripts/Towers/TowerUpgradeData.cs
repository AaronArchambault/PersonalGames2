using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Tower Defense/Tower Upgrade Data")]
public class TowerUpgradeData : ScriptableObject
{
    [System.Serializable]
    public class Tier
    {
        public string upgradeName;
        [TextArea(1,2)]
        public string description;
        public int   cost;
        public float damageBonus;
        public float rangeBonus;
        public float fireRateBonus;
        public Sprite icon;
    }

    public string towerName;
    public Tier[] pathA = new Tier[3];  // Damage path
    public Tier[] pathB = new Tier[3];  // Utility path
}