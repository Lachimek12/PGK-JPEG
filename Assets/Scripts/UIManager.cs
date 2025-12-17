using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text PrevButtonText;
    public GameObject NextButton;
    public GameObject[] panels;
    public GameObject[] infoPanels;
    private int index = 0;
    private bool isInfoVisible = false;

    void Start()
    {
        Show(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Prev(true);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Next();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (GameObject pan in infoPanels)
            {
                pan.SetActive(false);
            }
            isInfoVisible = false;
        }
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

        if (index == 0)
        {
            PrevButtonText.text = "Wyjdü";
        }
        else
        {
            PrevButtonText.text = "Wstecz";
        }

        if (index == panels.Length - 1)
        {
            NextButton.SetActive(false);
        }
        else
        {
            NextButton.SetActive(true);
        }
    }

    public void Next()
    {
        if (index < panels.Length - 1)
            Show(index + 1);
    }

    public void Prev(bool isLeftArrowPress)
    {
        if (index > 0)
            Show(index - 1);
        else if (isLeftArrowPress == false)
            Application.Quit();
    }

    public void ToggleInfoBox()
    { 
        isInfoVisible = !isInfoVisible;
        foreach (GameObject pan in infoPanels)
        {
            pan.SetActive(isInfoVisible);
        }
    }
}
