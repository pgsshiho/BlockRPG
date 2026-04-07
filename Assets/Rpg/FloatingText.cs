using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1.2f;    // 위로 올라가는 속도
    public float fadeSpeed = 1.0f;    // 사라지는 속도
    public float lifeTime = 1.5f;     // 파괴될 때까지의 시간

    private TextMeshProUGUI text;
    private Color color;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        color = text.color;

        // 생성 시 약간의 좌우 랜덤 위치를 주어 글자가 겹치지 않게 합니다.
        transform.localPosition += new Vector3(Random.Range(-30f, 30f), Random.Range(-10f, 10f), 0);

        // 지정된 시간 후 오브젝트 삭제
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. 위로 이동
        transform.localPosition += Vector3.up * moveSpeed * Time.deltaTime * 100f;

        // 2. 투명도 감소
        color.a -= fadeSpeed * Time.deltaTime;
        text.color = color;

        // 3. 서서히 크기가 커지는 연출 (선택 사항)
        transform.localScale += Vector3.one * 0.2f * Time.deltaTime;
    }

    // 외부에서 텍스트 설정 시 호출할 함수
    public void Setup(string message, float fontSize, Color textColor)
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = fontSize;
        text.color = textColor;
        color = textColor; // 초기 컬러 동기화
    }
}