using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class LevelMenuManager : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";
    [SerializeField] private Button[] levelButtons;

    void Start()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
        // Tüm dinamik level isimlerini toplayacağımız liste
        List<string> allLevelNames = new List<string>();

        // 1. ADIM: Resources/Levels klasörünün içindeki gömülü text dosyalarını çek (Build içi)
        TextAsset[] embeddedLevels = Resources.LoadAll<TextAsset>("Levels");
        foreach (TextAsset levelAsset in embeddedLevels)
        {
            if (!allLevelNames.Contains(levelAsset.name))
            {
                allLevelNames.Add(levelAsset.name);
            }
        }

        // 2. ADIM: Cihazın yerel hafızasındaki JSON dosyalarını çek (Kullanıcı ürettiyse)
        string folderPath = Application.persistentDataPath;
        if (Directory.Exists(folderPath))
        {
            string[] filePaths = Directory.GetFiles(folderPath, "*.json");
            foreach (string filePath in filePaths)
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                if (!allLevelNames.Contains(name))
                {
                    allLevelNames.Add(name);
                }
            }
        }

        // 3. ADIM: Butonları hiyerarşiye göre eşleştir ve aktif et
        for (int i = 0; i < levelButtons.Length; i++)
        {
            Button currentButton = levelButtons[i];
            if (currentButton == null) continue;

            if (i < allLevelNames.Count)
            {
                currentButton.gameObject.SetActive(true);
                currentButton.interactable = true;

                string levelName = allLevelNames[i];

                currentButton.onClick.RemoveAllListeners();
                currentButton.onClick.AddListener(() => OnLevelSelected(levelName));
            }
            else
            {
                currentButton.gameObject.SetActive(false);
            }
        }
    }

    void OnLevelSelected(string levelName)
    {
        PlayerPrefs.SetString("SelectedLevelToLoad", levelName);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameplaySceneName);
    }
}