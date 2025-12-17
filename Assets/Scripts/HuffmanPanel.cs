using TMPro;
using UnityEngine;

public class HuffmanPanel : MonoBehaviour, IPanel
{
    public JPEGCompressor pipeline;
    public GameObject huffmanCellPrefab;

    public void OnShow()
    {
        if (pipeline.HuffmanParent != null && pipeline.HuffmanParent.childCount == 0)
        {
            for (int i = 0; i < 64; i++)
            {
                GameObject cell = Instantiate(huffmanCellPrefab, pipeline.HuffmanParent);
                pipeline.HuffmanCells[i] = cell.GetComponent<TMP_Text>();
            }
        }
        
        pipeline.UpdateHuffmanPanel();
    }
}
