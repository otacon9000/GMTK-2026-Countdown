using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GmtkCountdown.EditorTools
{
    /// <summary>
    /// One-off batch-mode builder for the grey-box placeholder scene. Not part of runtime code.
    /// </summary>
    public static class ScenePlaceholderBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Tools/GMTK Countdown/Build Placeholder Scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Sprite squareSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("Sprites/Square.psd");
            Material spriteMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

            RenameExistingGameManager();
            BuildEnvironment(squareSprite, spriteMaterial);
            BuildUI();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ScenePlaceholderBuilder] Scene build complete.");
        }

        private static void RenameExistingGameManager()
        {
            var gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.gameObject.name = "GameManager";
            }
        }

        private static void BuildEnvironment(Sprite squareSprite, Material spriteMaterial)
        {
            var environment = new GameObject("Environment");

            GameObject corridor = CreateSpriteRect(
                "Corridor", environment.transform, squareSprite, spriteMaterial,
                position: new Vector3(0f, 2.2f, 0f),
                size: new Vector2(7f, 3.6f),
                color: new Color(0.15f, 0.15f, 0.18f, 1f),
                sortingOrder: 0);

            GameObject bossSilhouette = CreateSpriteRect(
                "BossSilhouette", environment.transform, squareSprite, spriteMaterial,
                position: new Vector3(0f, 2.5f, 0f),
                size: new Vector2(1.2f, 2.4f),
                color: new Color(0.05f, 0.05f, 0.05f, 1f),
                sortingOrder: 1);

            GameObject desk = CreateSpriteRect(
                "Desk", environment.transform, squareSprite, spriteMaterial,
                position: new Vector3(0f, -2.5f, 0f),
                size: new Vector2(6f, 2.2f),
                color: new Color(0.42f, 0.33f, 0.27f, 1f),
                sortingOrder: 2);

            _ = corridor;
            _ = bossSilhouette;
            _ = desk;
        }

        private static GameObject CreateSpriteRect(
            string name, Transform parent, Sprite sprite, Material material,
            Vector3 position, Vector2 size, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            return go;
        }

        private static void BuildUI()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            GameObject canvasObject;
            if (canvas == null)
            {
                canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }
            else
            {
                canvasObject = canvas.gameObject;
            }

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                _ = eventSystemObject;
            }

            var uiRoot = new GameObject("UI", typeof(RectTransform));
            uiRoot.transform.SetParent(canvasObject.transform, false);
            var uiRootRect = uiRoot.GetComponent<RectTransform>();
            uiRootRect.anchorMin = Vector2.zero;
            uiRootRect.anchorMax = Vector2.one;
            uiRootRect.offsetMin = Vector2.zero;
            uiRootRect.offsetMax = Vector2.zero;

            BuildPromptArea(uiRoot.transform);
            BuildCardHandArea(uiRoot.transform);
        }

        private static void BuildPromptArea(Transform parent)
        {
            var promptArea = new GameObject("PromptArea", typeof(RectTransform));
            promptArea.transform.SetParent(parent, false);
            var promptAreaRect = promptArea.GetComponent<RectTransform>();
            promptAreaRect.anchorMin = new Vector2(0.5f, 1f);
            promptAreaRect.anchorMax = new Vector2(0.5f, 1f);
            promptAreaRect.pivot = new Vector2(0.5f, 1f);
            promptAreaRect.anchoredPosition = new Vector2(0f, -40f);
            promptAreaRect.sizeDelta = new Vector2(900f, 120f);

            var promptText = new GameObject("PromptText", typeof(RectTransform));
            promptText.transform.SetParent(promptArea.transform, false);
            var promptTextRect = promptText.GetComponent<RectTransform>();
            promptTextRect.anchorMin = Vector2.zero;
            promptTextRect.anchorMax = Vector2.one;
            promptTextRect.offsetMin = Vector2.zero;
            promptTextRect.offsetMax = Vector2.zero;

            var tmp = promptText.AddComponent<TextMeshProUGUI>();
            tmp.text = "Prompt placeholder text goes here";
            tmp.alignment = TextAlignmentOptions.Top;
            tmp.fontSize = 36f;
            tmp.color = Color.white;
        }

        private static void BuildCardHandArea(Transform parent)
        {
            var cardHandArea = new GameObject("CardHandArea", typeof(RectTransform));
            cardHandArea.transform.SetParent(parent, false);
            var cardHandAreaRect = cardHandArea.GetComponent<RectTransform>();
            cardHandAreaRect.anchorMin = new Vector2(0.5f, 0f);
            cardHandAreaRect.anchorMax = new Vector2(0.5f, 0f);
            cardHandAreaRect.pivot = new Vector2(0.5f, 0f);
            cardHandAreaRect.anchoredPosition = new Vector2(0f, 40f);
            cardHandAreaRect.sizeDelta = new Vector2(1000f, 300f);
        }
    }
}
