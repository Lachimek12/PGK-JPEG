using UnityEngine;
using UnityEngine.EventSystems;

public class ImageBlockSelector : MonoBehaviour, IPointerClickHandler
{
    public JPEGCompressor pipeline;

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransform rt = pipeline.SelectorImage.rectTransform;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        Rect rect = rt.rect;

        float nx = (localPoint.x - rect.x) / rect.width;
        float ny = (localPoint.y - rect.y) / rect.height;

        int px = Mathf.FloorToInt(nx * pipeline.Width);
        int py = Mathf.FloorToInt(ny * pipeline.Height);

        pipeline.SelectBlockFromPixel(px, py);
        pipeline.RefreshBlockImage();
        pipeline.RefreshDCTImage();
    }
}
