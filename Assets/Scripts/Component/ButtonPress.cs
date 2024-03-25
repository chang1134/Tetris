using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPress : Button
{
    [Header("x���ʼ���������¼�")]
    public float pressDurationTime = 0.5f;
    [Header("ÿ��x��ִ��һ�γ����¼�")]
    public float pressInterval = 0.2f;

    /// <summary>
    /// �������¼���Ӧ
    /// </summary>
    public ButtonClickedEvent onPress;
    public ButtonClickedEvent onRelease;

    /// <summary>
    /// ����ԭʼ����
    /// </summary>
    public void ResetAttribute()
    {
        isDown = false;
        isPress = false;

        pressTime = 0;
        downTime = 0;
    }

    public void Update()
    {
        // ����
        if (isDown)
        {
            // ����ʱ����������¼�
            if (isPress)
            {
                pressTime += Time.deltaTime;
                if (pressTime > pressInterval)
                {
                    pressTime = 0;
                    onPress?.Invoke();

                }
            }
            else
            {
                downTime += Time.deltaTime;
                // ��ʼ���������¼�
                if (downTime > pressDurationTime)
                {
                    isPress = true;
                    pressTime = 0;
                    onPress?.Invoke();
                }
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        downTime = 0;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
        onRelease?.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        isDown = false;
        isPress = false;
        onRelease?.Invoke();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (isPress)
        {
            isPress = false;
            onClick?.Invoke();
        }
        else
        {
            base.OnPointerClick(eventData);
        }
    }

    public new void OnDisable()
    {
        base.OnDisable();
        ResetAttribute();
    }

    private bool isDown = false;
    private bool isPress = false;

    private float downTime = 0;
    private float pressTime = 0;
}
