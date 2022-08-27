using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ChecklistItem : UdonSharpBehaviour
{
    public string Prefix;
    public string Title;
    public CheckItem[] CheckItems;
}
