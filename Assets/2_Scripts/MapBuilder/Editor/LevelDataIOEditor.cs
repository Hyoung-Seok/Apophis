using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelDataIOEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset root;
    [SerializeField] private VisualTreeAsset levelListItem;

    private TextField _levelNameInputField;
    private Button _saveLevelBtn;
    private VisualElement _levelList;

    private MapBuilder _mapBuilder;
    
    public static void ShowWindow()
    {
        var window = GetWindow<LevelDataIOEditor>();
        
        window.titleContent = new GUIContent("레벨 데이터 관리");
        window.minSize = window.maxSize = new Vector2(450, 450);
    }

    private void CreateGUI()
    {
        root.CloneTree(rootVisualElement);
        _mapBuilder = FindFirstObjectByType<MapBuilder>();

        if (_mapBuilder == null)
        {
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("실패",
                "현재 씬에 MapBuilder가 존재하지 않거나 비활성화 되었습니다!", "닫기");
            Close();
            return;
        }
        
        GetElements();
        RefreshLevelList();
    }

    private void GetElements()
    {
        _levelNameInputField = rootVisualElement.Q<TextField>("LevelNameInputField");
        _levelNameInputField.RegisterValueChangedCallback(OnValueChangedLevelName);
        
        _saveLevelBtn  = rootVisualElement.Q<Button>("SaveLevelBtn");
        _saveLevelBtn.clicked += OnClickedSaveLevelBtn;
        
        _levelList = rootVisualElement.Q<VisualElement>("LevelList");
    }

    private void RefreshLevelList()
    {
        _levelList.Clear();
        
        var guids = AssetDatabase.FindAssets("t:LevelData", new[] { LevelDataIO.DEFAULT_PATH });
        if (guids == null || guids.Length <= 0) return;

        foreach (var guid in guids)
        {
            var item = CreateLevelListItem(guid);
            if(item != null) _levelList.Add(item);
        }
    }

    private TemplateContainer CreateLevelListItem(string guid)
    {
        if(TryGetPathFromGuid(guid, out var path) == false) return null;
        
        var uxml = levelListItem.CloneTree();
        var levelName = Path.GetFileNameWithoutExtension(path);

        uxml.style.height = uxml.style.width = 80; 
        uxml.name = guid;
        uxml.Q<Label>("LevelName").text = levelName;
        
        var btn = uxml.Q<Button>("LevelSelectBtn");
        btn.clicked += () =>
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("불러오기"), false, () =>
            {
                EditorApplication.Beep();
                if (EditorUtility.DisplayDialog("불러오기",
                        "레벨 정보를 불러오시겠습니까?",
                        "불러오기", "취소") == true)
                {
                    LevelDataIO.Load(AssetDatabase.LoadAssetAtPath<LevelData>(path));
                }
            });
            menu.AddItem(new GUIContent("편집하기"), false, () =>
            {
                EditorApplication.Beep();
                if (EditorUtility.DisplayDialog("편집하기", 
                        "레벨 정보를 불러오시겠습니까?(주의 : 현재 배치중인 레벨 정보가 사라집니다.)", 
                        "확인", "취소") == true)
                {
                    LevelDataIO.LoadAsEditingMode(_mapBuilder, AssetDatabase.LoadAssetAtPath<LevelData>(path));   
                }
            });
            menu.AddItem(new GUIContent("삭제"), false, () => DeleteLevel(uxml));
            menu.AddItem(new GUIContent("이름 변경"), false, () => RenameLevel(uxml));
            menu.ShowAsContext();
        };

        return uxml;
    }

    private void OnClickedSaveLevelBtn()
    {
        var levelName = _levelNameInputField.text;
        EditorApplication.Beep();
        
        if (levelName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("저장 실패", "파일명에 사용할 수 없는 문자가 포함되어 있습니다.", "확인");
            return;
        }

        var success = LevelDataIO.Save(_mapBuilder, levelName);
        switch (success)
        {
            case ESaveResult.Success:
                EditorApplication.Beep();
                RefreshLevelList();
                EditorUtility.DisplayDialog("저장",$"{levelName} 저장 성공!","확인");
                break;
            
            case ESaveResult.Cancelled:
            default:
                break;
        }
    }

    private void DeleteLevel(TemplateContainer uxml)
    {
        EditorApplication.Beep();
        
        if (TryGetPathFromGuid(uxml.name, out var path) == false)
        {
            EditorUtility.DisplayDialog("삭제 실패", "해당 에셋을 찾을 수 없습니다.", "확인");
            return;
        }

        if (EditorUtility.DisplayDialog("삭제", "정말 레벨 데이터를 삭제하시겠습니까?", "삭제", "취소") == false)
        {
            return;
        }
        
        var success = AssetDatabase.DeleteAsset(path);
        if (success == true)
        {
            AssetDatabase.SaveAssets();
            _levelList.Remove(uxml);
            return;
        }
        
        EditorUtility.DisplayDialog("삭제 실패", "삭제 실패", "확인");
    }

    private void RenameLevel(TemplateContainer uxml)
    {
        if (TryGetPathFromGuid(uxml.name, out var path) == false)
        {
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("이름 변경", "해당 에셋을 찾을 수 없습니다.", "확인");
            return;
        }

        if (CheckFileNameValidation() == false)
        {
            return;
        }
        
        var levelName = _levelNameInputField.text;
        var dir = Path.GetDirectoryName(path);
        var targetPath = $"{dir}/{levelName}.asset".Replace("\\", "/");
        
        if (AssetDatabase.LoadAssetAtPath<LevelData>(targetPath) != null)
        {
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("이름 변경 실패",
                $"'{levelName}'이 이미 존재합니다.", "확인");

            return;
        }
        
        var error = AssetDatabase.RenameAsset(path, levelName);
        if (!string.IsNullOrEmpty(error))
        {
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("이름 변경 실패", error, "확인");
            return;
        }

        AssetDatabase.SaveAssets();
        uxml.Q<Label>("LevelName").text = levelName;
    }

    private void OnValueChangedLevelName(ChangeEvent<string> evt)
    {
        var isEmpty = string.IsNullOrEmpty(_levelNameInputField.text);
        _saveLevelBtn.SetEnabled(!isEmpty);
    }

    private bool TryGetPathFromGuid(string guid, out string path)
    {
        path = AssetDatabase.GUIDToAssetPath(guid);
        return !string.IsNullOrEmpty(path);
    }

    private bool CheckFileNameValidation()
    {
        if (_levelNameInputField.text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("실패", "사용 불가능한 이름입니다.", "확인");
            return false;
        }

        return true;
    }
}
