using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("MonoBehaviour")] 
    [SerializeField] private List<MonoBehaviour> monoSystem;
    private List<IGameSystem> _allSystems;
    
    public T Get<T>() where T : class, IGameSystem
        => _allSystems.OfType<T>().FirstOrDefault();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        _allSystems = new List<IGameSystem>();
        
        InitNonMonoSystems();

        foreach (var mono in monoSystem)
        {
            _allSystems.Add((IGameSystem)mono);
        }
        
        _allSystems.ForEach(x => x.Init());
    }

    private void InitNonMonoSystems()
    {
        
    }
    
}
