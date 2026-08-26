using UnityEngine;

public class TextBoxHandler : MonoBehaviour
{
    public void EnableTextBox(string content)
    {
        UnitManager.Instance.textBoxText.text = content;
        UnitManager.Instance.textBoxObj.SetActive(true);
    }
}
