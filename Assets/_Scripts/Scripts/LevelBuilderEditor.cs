#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelBuilderPro))]
public class LevelBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelBuilderPro script = (LevelBuilderPro)target;

        GUILayout.Space(15);
        GUILayout.Label("LEVEL EDİTÖRÜ ARAÇLARI", EditorStyles.boldLabel);

        if (GUILayout.Button("Levelı JSON Olarak Kaydet", GUILayout.Height(35)))
        {
            script.SaveLevel();
        }

        if (GUILayout.Button("JSON'dan Levelı Yükle", GUILayout.Height(35)))
        {
            script.LoadLevel();
        }

        if (GUILayout.Button("Sahneyi Tamamen Temizle", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Emin misin?", "Sahnedeki tüm bloklar silinecek!", "Evet", "Hayır"))
            {
                script.ClearLevel();
            }
        }
    }
}
#endif