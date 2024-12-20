using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DragAndDropManager))]
public class DragAndDropManagerEditor : Editor
{
    private SerializedProperty objectConfigsProp;

    private bool showGeneralSettings = true;
    private bool showMapProps = true;
    private bool showIngredients = true;
    private bool showAppliances = true;

    private void OnEnable()
    {
        objectConfigsProp = serializedObject.FindProperty("objectConfigs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Titre principal
        EditorGUILayout.LabelField("Drag and Drop Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Section des paramètres généraux
        showGeneralSettings = EditorGUILayout.Foldout(showGeneralSettings, "Paramètres Généraux", true);
        if (showGeneralSettings)
        {
            DrawDefaultInspectorExcept("objectConfigs");
        }

        // Diviser par catégories
        EditorGUILayout.Space();
        DrawCategory("MapProps", "Props de la Map", ref showMapProps);
        EditorGUILayout.Space();
        DrawCategory("Ingredients", "Ingrédients", ref showIngredients);
        EditorGUILayout.Space();
        DrawCategory("Appliances", "Objets du Ménager", ref showAppliances);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefaultInspectorExcept(string excludedProperty)
    {
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            if (prop.name != excludedProperty)
            {
                EditorGUILayout.PropertyField(prop, true);
            }
            enterChildren = false;
        }
    }

    private void DrawCategory(string categoryName, string title, ref bool foldoutState)
    {
        foldoutState = EditorGUILayout.Foldout(foldoutState, title, true);
        if (!foldoutState) return;

        EditorGUILayout.BeginVertical("box");

        // Ajouter un bouton pour insérer un nouvel élément
        if (GUILayout.Button($"Ajouter un élément à {title}", GUILayout.Height(30)))
        {
            AddNewItemToCategory(categoryName);
        }

        // Parcourir les éléments
        for (int i = objectConfigsProp.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty config = objectConfigsProp.GetArrayElementAtIndex(i);
            SerializedProperty categoryProp = config.FindPropertyRelative("category");

            if (categoryProp != null && categoryProp.stringValue == categoryName)
            {
                // Bloc de l'élément
                EditorGUILayout.BeginVertical("box");

                // Section des boutons
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace(); // Décaler les boutons à droite

                if (GUILayout.Button("▲", GUILayout.Width(30), GUILayout.Height(20))) MoveItem(i, -1);
                if (GUILayout.Button("▼", GUILayout.Width(30), GUILayout.Height(20))) MoveItem(i, 1);
                if (GUILayout.Button("✖", GUILayout.Width(30), GUILayout.Height(20)))
                {
                    RemoveItem(i);
                    break; // Arrêter l'itération pour éviter les accès invalides
                }

                EditorGUILayout.EndHorizontal();

                // Afficher les propriétés
                EditorGUILayout.PropertyField(config, GUIContent.none, true);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void AddNewItemToCategory(string categoryName)
    {
        objectConfigsProp.arraySize++;
        serializedObject.ApplyModifiedProperties(); // Appliquer immédiatement pour éviter les références obsolètes

        SerializedProperty newItem = objectConfigsProp.GetArrayElementAtIndex(objectConfigsProp.arraySize - 1);
        SerializedProperty categoryProp = newItem.FindPropertyRelative("category");
        if (categoryProp != null)
        {
            categoryProp.stringValue = categoryName;
        }
    }

    private void RemoveItem(int index)
    {
        objectConfigsProp.DeleteArrayElementAtIndex(index);
        serializedObject.ApplyModifiedProperties(); // Appliquer immédiatement pour éviter les erreurs
    }

    private void MoveItem(int index, int direction)
    {
        int newIndex = index + direction;
        if (newIndex < 0 || newIndex >= objectConfigsProp.arraySize) return;

        objectConfigsProp.MoveArrayElement(index, newIndex);
        serializedObject.ApplyModifiedProperties(); // Appliquer immédiatement pour éviter les erreurs
    }
}
