using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectConfig))]
public class ObjectConfigDrawer : PropertyDrawer
{
    private static readonly float LineHeight = EditorGUIUtility.singleLineHeight; // Hauteur d'une ligne standard
    private const float VerticalSpacing = 4f; // Espace supplémentaire entre les lignes

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Indentation pour organiser les champs
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 1;

        // Calcul de la position des lignes
        Rect currentRect = new Rect(position.x, position.y, position.width, LineHeight);

        // Accéder aux propriétés enfant
        var prefabProp = property.FindPropertyRelative("prefab");
        var ingredientDataProp = property.FindPropertyRelative("ingredientData");
        var canReceiveChildrenProp = property.FindPropertyRelative("canReceiveChildren");
        var hasDoorProp = property.FindPropertyRelative("hasDoor");
        var isMovableProp = property.FindPropertyRelative("isMovable");
        var dropZoneColliderProp = property.FindPropertyRelative("dropZoneCollider");
        var doorTransformProp = property.FindPropertyRelative("doorTransform");
        var rotateDoorOnXProp = property.FindPropertyRelative("rotateDoorOnX");
        var rotateDoorOnYProp = property.FindPropertyRelative("rotateDoorOnY");
        var invertDoorRotationProp = property.FindPropertyRelative("invertDoorRotation");
        var doorRotationLimitProp = property.FindPropertyRelative("doorRotationLimit");

        // Dessiner les champs
        // 1. Prefab
        EditorGUI.PropertyField(currentRect, prefabProp);
        currentRect.y += LineHeight + VerticalSpacing;

        // 2. IngredientData
        EditorGUI.PropertyField(currentRect, ingredientDataProp);
        currentRect.y += LineHeight + VerticalSpacing;

        // 3. Paramètre "isMovable"
        EditorGUI.PropertyField(currentRect, isMovableProp);
        currentRect.y += LineHeight + VerticalSpacing;

        // 4. Paramètre "canReceiveChildren"
        EditorGUI.PropertyField(currentRect, canReceiveChildrenProp);
        currentRect.y += LineHeight + VerticalSpacing;

        if (canReceiveChildrenProp.boolValue)
        {
            EditorGUI.indentLevel = indent;
            // Afficher le champ "dropZoneCollider" uniquement si "canReceiveChildren" est activé
            EditorGUI.PropertyField(currentRect, dropZoneColliderProp);
            currentRect.y += LineHeight + VerticalSpacing;
        }

        // 5. Paramètre "hasDoor"
        EditorGUI.PropertyField(currentRect, hasDoorProp);
        currentRect.y += LineHeight + VerticalSpacing;

        if (hasDoorProp.boolValue)
        {
            EditorGUI.indentLevel = indent;
            // Afficher les paramètres liés à la porte si "hasDoor" est activé
            EditorGUI.PropertyField(currentRect, doorTransformProp);
            currentRect.y += LineHeight + VerticalSpacing;

            EditorGUI.PropertyField(currentRect, rotateDoorOnXProp);
            currentRect.y += LineHeight + VerticalSpacing;

            EditorGUI.PropertyField(currentRect, rotateDoorOnYProp);
            currentRect.y += LineHeight + VerticalSpacing;

            EditorGUI.PropertyField(currentRect, invertDoorRotationProp);
            currentRect.y += LineHeight + VerticalSpacing;

            EditorGUI.PropertyField(currentRect, doorRotationLimitProp);
            currentRect.y += LineHeight + VerticalSpacing;
        }

        // Réinitialiser l'indentation
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculez dynamiquement la hauteur totale
        float totalHeight = LineHeight + VerticalSpacing; // Pour le prefab
        totalHeight += LineHeight + VerticalSpacing;      // Pour isMovable
        totalHeight += LineHeight + VerticalSpacing;      // Pour canReceiveChildren

        if (property.FindPropertyRelative("canReceiveChildren").boolValue)
        {
            totalHeight += LineHeight + VerticalSpacing; // Pour dropZoneCollider
        }

        totalHeight += LineHeight + VerticalSpacing;      // Pour hasDoor

        if (property.FindPropertyRelative("hasDoor").boolValue)
        {
            totalHeight += LineHeight + VerticalSpacing * 15; // Pour les paramètres de porte
        }

        return totalHeight + 60;
    }
}
