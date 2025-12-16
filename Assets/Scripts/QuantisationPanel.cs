using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuantisationPanel : MonoBehaviour, IPanel
{
    public JPEGCompressor pipeline;
    public Slider QualitySlider;
    public GameObject qCellPrefab;

    public void OnShow()
    {
        if (pipeline.QMatrixParent.childCount == 0)
        {
            for (int i = 0; i < 64; i++)
            {
                GameObject cell = Instantiate(qCellPrefab, pipeline.QMatrixParent);
                pipeline.QMatrixCells[i] = cell.GetComponent<TMP_Text>();
            }
        }
        if (pipeline.QDCTParent.childCount == 0)
        {
            for (int i = 0; i < 64; i++)
            {
                GameObject cell = Instantiate(qCellPrefab, pipeline.QDCTParent);
                pipeline.QDCTCells[i] = cell.GetComponent<TMP_Text>();
            }
        }
        if (pipeline.QuantizedParent.childCount == 0)
        {
            for (int i = 0; i < 64; i++)
            {
                GameObject cell = Instantiate(qCellPrefab, pipeline.QuantizedParent);
                pipeline.QuantizedCells[i] = cell.GetComponent<TMP_Text>();
            }
        }
        QualitySlider.value = pipeline.JpegQuality;
        pipeline.UpdateQuantizationPanel();
    }

    public void OnQualityChanged(float value)
    {
        pipeline.JpegQuality = Mathf.RoundToInt(value);
        pipeline.UpdateQuantizationPanel();
    }
}
