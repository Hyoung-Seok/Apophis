using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<Magazine> Magazines => magazines;
    [SerializeField] private List<Magazine> magazines;
}
