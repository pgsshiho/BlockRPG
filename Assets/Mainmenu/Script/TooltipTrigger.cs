using UnityEngine;
using UnityEngine.EventSystems; // 마우스 이벤트를 위해 필수!

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipWindow; // 띄울 설명창 오브젝트 (Inspector에서 등록)

    void Start()
    {
        if (tooltipWindow != null)
            tooltipWindow.SetActive(false); // 처음엔 꺼두기
    }

    // 마우스를 올렸을 때 실행
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipWindow != null)
            tooltipWindow.SetActive(true);
    }

    // 마우스가 벗어났을 때 실행
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipWindow != null)
            tooltipWindow.SetActive(false);
    }
}