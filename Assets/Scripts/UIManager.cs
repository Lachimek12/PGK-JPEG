using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject[] panels;
    private int index = 0;

    void Start()
    {
        Show(0);
    }

    public void Show(int i)
    {
        if (i > index)
        {
            panels[i].GetComponent<IPanel>().OnShow();
        }

        panels[index].SetActive(false);
        panels[i].SetActive(true);
        index = i;
    }

    public void Next()
    {
        if (index < panels.Length - 1)
            Show(index + 1);
    }

    public void Prev()
    {
        if (index > 0)
            Show(index - 1);
    }
}
