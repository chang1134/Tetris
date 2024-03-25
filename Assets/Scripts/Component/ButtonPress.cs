using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPress : Button
{
    [Header("x秒后开始触发长按事件")]
    public float pressDurationTime = 0.5f;
    [Header("每隔x秒执行一次长按事件")]
    public float pressInterval = 0.2f;

    /// <summary>
    /// 长按的事件响应
    /// </summary>
    public ButtonClickedEvent onPress;
    public ButtonClickedEvent onRelease;

    /// <summary>
    /// 重置原始属性
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
        // 长按
        if (isDown)
        {
            // 长按时，间隔触发事件
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
                // 开始触发长按事件
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
