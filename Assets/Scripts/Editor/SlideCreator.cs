using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SlideCreator : EditorWindow
{
    private string slideName = "NewSlide";
    private Color backgroundColor = Color.white;
    
    [MenuItem("Tools/Create Slide (1920x1080)")]
    public static void ShowWindow()
    {
        GetWindow<SlideCreator>("Create Slide");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Create 1920x1080 Slide", EditorStyles.boldLabel);
        
        slideName = EditorGUILayout.TextField("Slide Name:", slideName);
        backgroundColor = EditorGUILayout.ColorField("Background Color:", backgroundColor);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Slide"))
        {
            CreateSlide();
        }
    }
    
    private void CreateSlide()
    {
        GameObject slideObject = new GameObject(slideName);
        
        Canvas canvas = slideObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        
        CanvasScaler scaler = slideObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        slideObject.AddComponent<GraphicRaycaster>();
        
        CanvasGroup canvasGroup = slideObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        Slide slideComponent = slideObject.AddComponent<Slide>();
        
        EditorUtility.SetDirty(slideObject);
        
        using (SerializedObject serializedSlide = new SerializedObject(slideComponent))
        {
            SerializedProperty titleProperty = serializedSlide.FindProperty("slideTitle");
            if (titleProperty != null)
            {
                titleProperty.stringValue = slideName;
                serializedSlide.ApplyModifiedProperties();
            }
        }
        
        GameObject background = new GameObject("Background");
        background.transform.SetParent(slideObject.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        if (bgRect != null)
        {
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
        }
        
        Image bgImage = background.AddComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = backgroundColor;
        }
        
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(slideObject.transform, false);
        
        RectTransform titleRect = titleObject.AddComponent<RectTransform>();
        if (titleRect != null)
        {
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(0, 1);
            titleRect.pivot = new Vector2(0, 1);
            titleRect.anchoredPosition = new Vector2(50, -50);
            titleRect.sizeDelta = new Vector2(800, 100);
        }
        
        UnityEngine.UI.Text titleText = titleObject.AddComponent<UnityEngine.UI.Text>();
        if (titleText != null)
        {
            titleText.text = slideName;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 48;
            titleText.color = Color.black;
            titleText.alignment = TextAnchor.UpperLeft;
        }
        
        GameObject imageObject = new GameObject("Image");
        imageObject.transform.SetParent(slideObject.transform, false);
        
        RectTransform imageRect = imageObject.AddComponent<RectTransform>();
        if (imageRect != null)
        {
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = Vector2.zero;
            imageRect.sizeDelta = new Vector2(1200, 800);
        }
        
        Image imageComponent = imageObject.AddComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        }
        
        SmartImageDisplay smartDisplay = imageObject.AddComponent<SmartImageDisplay>();
        EditorUtility.SetDirty(imageObject);
        
        using (SerializedObject smartSerialized = new SerializedObject(smartDisplay))
        {
            SerializedProperty enableZoom = smartSerialized.FindProperty("enablePixelZoom");
            if (enableZoom != null)
            {
                enableZoom.boolValue = true;
                smartSerialized.ApplyModifiedProperties();
            }
        }
        
        CreateNavigationButtons(slideObject);
        
        Undo.RegisterCreatedObjectUndo(slideObject, "Create Slide");
        Selection.activeGameObject = slideObject;
        EditorUtility.FocusProjectWindow();
        
        Debug.Log($"Created slide: {slideName} with 1920x1080 resolution, title, image placeholder, and navigation arrows");
    }
    
    private void CreateNavigationButtons(GameObject slideObject)
    {
        SlideController controller = FindFirstObjectByType<SlideController>();
        if (controller == null)
        {
            Debug.LogWarning("No SlideController found in scene. Navigation buttons will be created but may not work until SlideController is added.");
        }
        
        GameObject leftArrow = new GameObject("LeftArrow");
        leftArrow.transform.SetParent(slideObject.transform, false);
        
        RectTransform leftRect = leftArrow.AddComponent<RectTransform>();
        if (leftRect != null)
        {
            leftRect.anchorMin = new Vector2(0, 0.5f);
            leftRect.anchorMax = new Vector2(0, 0.5f);
            leftRect.pivot = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(50, 0);
            leftRect.sizeDelta = new Vector2(100, 100);
        }
        
        Image leftImage = leftArrow.AddComponent<Image>();
        if (leftImage != null)
        {
            leftImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
        
        Button leftButton = leftArrow.AddComponent<Button>();
        ColorBlock leftColors = leftButton.colors;
        leftColors.normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        leftColors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 0.9f);
        leftColors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        leftButton.colors = leftColors;
        
        SlideNavigationButton leftNav = leftArrow.AddComponent<SlideNavigationButton>();
        EditorUtility.SetDirty(leftArrow);
        
        using (SerializedObject leftNavSerialized = new SerializedObject(leftNav))
        {
            SerializedProperty leftIsNext = leftNavSerialized.FindProperty("isNextButton");
            if (leftIsNext != null)
            {
                leftIsNext.boolValue = false;
                leftNavSerialized.ApplyModifiedProperties();
            }
        }
        
        GameObject leftArrowText = new GameObject("Text");
        leftArrowText.transform.SetParent(leftArrow.transform, false);
        
        RectTransform leftTextRect = leftArrowText.AddComponent<RectTransform>();
        if (leftTextRect != null)
        {
            leftTextRect.anchorMin = Vector2.zero;
            leftTextRect.anchorMax = Vector2.one;
            leftTextRect.sizeDelta = Vector2.zero;
            leftTextRect.anchoredPosition = Vector2.zero;
        }
        
        UnityEngine.UI.Text leftText = leftArrowText.AddComponent<UnityEngine.UI.Text>();
        if (leftText != null)
        {
            leftText.text = "◄";
            leftText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            leftText.fontSize = 60;
            leftText.color = Color.white;
            leftText.alignment = TextAnchor.MiddleCenter;
        }
        
        GameObject rightArrow = new GameObject("RightArrow");
        rightArrow.transform.SetParent(slideObject.transform, false);
        
        RectTransform rightRect = rightArrow.AddComponent<RectTransform>();
        if (rightRect != null)
        {
            rightRect.anchorMin = new Vector2(1, 0.5f);
            rightRect.anchorMax = new Vector2(1, 0.5f);
            rightRect.pivot = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(-50, 0);
            rightRect.sizeDelta = new Vector2(100, 100);
        }
        
        Image rightImage = rightArrow.AddComponent<Image>();
        if (rightImage != null)
        {
            rightImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
        
        Button rightButton = rightArrow.AddComponent<Button>();
        ColorBlock rightColors = rightButton.colors;
        rightColors.normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        rightColors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 0.9f);
        rightColors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        rightButton.colors = rightColors;
        
        SlideNavigationButton rightNav = rightArrow.AddComponent<SlideNavigationButton>();
        EditorUtility.SetDirty(rightArrow);
        
        using (SerializedObject rightNavSerialized = new SerializedObject(rightNav))
        {
            SerializedProperty rightIsNext = rightNavSerialized.FindProperty("isNextButton");
            if (rightIsNext != null)
            {
                rightIsNext.boolValue = true;
                rightNavSerialized.ApplyModifiedProperties();
            }
        }
        
        GameObject rightArrowText = new GameObject("Text");
        rightArrowText.transform.SetParent(rightArrow.transform, false);
        
        RectTransform rightTextRect = rightArrowText.AddComponent<RectTransform>();
        if (rightTextRect != null)
        {
            rightTextRect.anchorMin = Vector2.zero;
            rightTextRect.anchorMax = Vector2.one;
            rightTextRect.sizeDelta = Vector2.zero;
            rightTextRect.anchoredPosition = Vector2.zero;
        }
        
        UnityEngine.UI.Text rightText = rightArrowText.AddComponent<UnityEngine.UI.Text>();
        if (rightText != null)
        {
            rightText.text = "►";
            rightText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rightText.fontSize = 60;
            rightText.color = Color.white;
            rightText.alignment = TextAnchor.MiddleCenter;
        }
    }
}

