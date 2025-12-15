using UnityEngine;
using static JPEGCompressor;

public class ChannelSelector : MonoBehaviour
{
    public JPEGCompressor pipeline;

    public void OnChanged(int index)
    {
        Debug.Log(index);
        pipeline.SetSelectedChannel((JpegChannel)index);
        pipeline.RefreshBlockImage();
        pipeline.RefreshDCTImage();
    }
}
