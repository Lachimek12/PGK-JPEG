using UnityEngine;

public class DCTPanel : MonoBehaviour, IPanel
{
    public JPEGCompressor pipeline;

    public void OnShow()
    {
        pipeline.SelectorImage.sprite = pipeline.SourceImage.sprite;
        pipeline.SetSelectedChannel(pipeline.SelectedChannel);
        pipeline.RefreshBlockImage();
        pipeline.RefreshDCTImage();
    }
}
