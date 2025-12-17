using TMPro;
using UnityEngine;

public class ZigZagPanel : MonoBehaviour, IPanel
{
    public JPEGCompressor pipeline;
    public GameObject qCellPrefab;
    public GameObject qArrayCellPrefab;

    public void OnShow()
    {
        if (pipeline.ZigZagMatrixNumberCells[0] == null)
        {
            for (int i = 0; i < 64; i++)
            {
                GameObject cell = Instantiate(qCellPrefab, pipeline.ZigZagMatrixNumberParent);
                pipeline.ZigZagMatrixNumberCells[i] = cell.GetComponent<TMP_Text>();
            }
        }
        if (pipeline.ZigZagArrayNumberCells[0] == null)
        {
            for (int i = 0; i < 64; i++)
            {
                GameObject cell = Instantiate(qArrayCellPrefab, pipeline.ZigZagArrayNumberParent);
                pipeline.ZigZagArrayNumberCells[i] = cell.GetComponent<TMP_Text>();
            }
        }
        
        pipeline.UpdateZigZagPanel();
    }
}
