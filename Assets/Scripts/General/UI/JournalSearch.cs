using UnityEngine;

public class JournalSearch : MonoBehaviour
{
    public SpriteText spritetext;
    public void Search(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            JournalManager.Instance.ClearSearch();
            spritetext.input = "";
            spritetext.Refresh();
        }
        else{
            JournalManager.Instance.SearchEntries(input);
            spritetext.input = input;
            spritetext.Refresh();
        }
    }
}
