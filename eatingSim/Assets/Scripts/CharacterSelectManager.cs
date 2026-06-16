// ─────────────────────────────────────────────────────────────────────────────
// Save as: Assets/Scripts/CharacterSelectManager.cs
// ─────────────────────────────────────────────────────────────────────────────
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName = "Goldfish";
        [TextArea] public string description = "";
        public Sprite portrait;
        public float  startSize     = 0.4f;
        public float  baseSpeed     = 5f;
        public float  massPerDouble = 50f;
    }

    public CharacterData[] characters = new CharacterData[]
    {
        new CharacterData { characterName="Goldfish",   description="Tiny but unstoppable.",  startSize=0.3f, baseSpeed=6f,   massPerDouble=40f },
        new CharacterData { characterName="Dolphin",    description="Balanced and swift.",     startSize=0.6f, baseSpeed=5f,   massPerDouble=55f },
        new CharacterData { characterName="Nano-Shark", description="Engineered to devour.",   startSize=0.2f, baseSpeed=4.5f, massPerDouble=35f },
    };

    public TMP_Text  characterNameLabel;
    public TMP_Text  descriptionLabel;
    public Image     portraitImage;
    public Button[]  characterButtons;
    public string    gameSceneName = "GameScene";

    private int selectedIndex = 0;

    void Start()
    {
        for (int i = 0; i < characterButtons.Length && i < characters.Length; i++)
        {
            int idx = i;
            characterButtons[i].onClick.AddListener(() => SelectCharacter(idx));
        }
        SelectCharacter(0);
    }

    public void SelectCharacter(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, characters.Length - 1);
        var c = characters[selectedIndex];
        if (characterNameLabel) characterNameLabel.text = c.characterName;
        if (descriptionLabel)   descriptionLabel.text   = c.description;
        if (portraitImage && c.portrait) portraitImage.sprite = c.portrait;
        for (int i = 0; i < characterButtons.Length; i++)
        {
            var cb = characterButtons[i].colors;
            cb.normalColor = (i == selectedIndex) ? Color.yellow : Color.white;
            characterButtons[i].colors = cb;
        }
    }

    public void StartGame()
    {
        var c = characters[selectedIndex];
        PlayerPrefs.SetInt   ("SelectedCharacter", selectedIndex);
        PlayerPrefs.SetFloat ("CharStartSize",     c.startSize);
        PlayerPrefs.SetFloat ("CharBaseSpeed",     c.baseSpeed);
        PlayerPrefs.SetFloat ("CharMassPerDouble", c.massPerDouble);
        PlayerPrefs.SetString("CharName",          c.characterName);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }
}


// ─────────────────────────────────────────────────────────────────────────────
// Save as: Assets/Scripts/GameBootstrap.cs
// ─────────────────────────────────────────────────────────────────────────────
// (separate file in your project — split at the class boundary)