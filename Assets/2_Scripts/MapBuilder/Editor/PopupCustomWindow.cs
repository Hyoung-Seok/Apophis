using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupCustomWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset uxml;

    private Label _message;
    private Button _acceptBtn;
    private Button _cancelBtn;

    public static void ShowWindow()
    {
        var window = GetWindow<PopupCustomWindow>();
        
        window.titleContent = new GUIContent("Message");
        window.minSize = window.maxSize = new Vector2(350, 150);
        
        EditorApplication.Beep();
    }

    public void CreateGUI()
    {
        uxml.CloneTree(rootVisualElement);
        
        _message = rootVisualElement.Q<Label>("Message");
        _acceptBtn = rootVisualElement.Q<Button>("AcceptBtn");
        _cancelBtn = rootVisualElement.Q<Button>("CancelBtn");

        _cancelBtn.clicked += Close;
    }

    public void SetMessage(string message)
    {
        _message.text = message;
    }

    public void AddAcceptBtnAction(Action action)
    {
        _acceptBtn.clicked += action;
    }
    
}
