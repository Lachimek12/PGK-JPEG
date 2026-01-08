using UnityEngine;
using UnityEngine.EventSystems;

public class ImageLensHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public JPEGCompressor pipeline;
    private Vector2Int currentBlock = new Vector2Int(-1, -1);

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (pipeline != null)
        {
            pipeline.HideLens();
            currentBlock = new Vector2Int(-1, -1);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
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

        int bx = Mathf.Clamp(px / JPEGCompressor.BLOCK_SIZE, 0, pipeline.Width / JPEGCompressor.BLOCK_SIZE - 1);
        int by = Mathf.Clamp(py / JPEGCompressor.BLOCK_SIZE, 0, pipeline.Height / JPEGCompressor.BLOCK_SIZE - 1);

        Vector2Int newBlock = new Vector2Int(bx, by);
        if (newBlock != currentBlock)
        {
            currentBlock = newBlock;
            pipeline.UpdateLensBlock(bx, by, eventData.position);
        }
    }
}

