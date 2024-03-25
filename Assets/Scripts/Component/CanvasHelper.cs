using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class CanvasHelper : MonoBehaviour
{
    // Thank you to Adriaan de Jongh https://twitter.com/adriaandejongh
    // author of Hidden Folks
    // for the bulk of the script below
    public static UnityEvent onOrientationChange = new UnityEvent();
    public static UnityEvent onResolutionChange = new UnityEvent();
    public static bool isLandscape { get; private set; }

    public static List<CanvasHelper> helpers = new List<CanvasHelper>();

    public static bool screenChangeVarsInitialized = false;
    public static ScreenOrientation lastOrientation = ScreenOrientation.Portrait;
    public static Vector2 lastResolution = Vector2.zero;
    public static Rect lastSafeArea = Rect.zero;

    public static float HeightOffset = -100;

    public Canvas canvas;
    public CanvasScaler canvasScaler;
    public RectTransform rectTransform;
    public RectTransform safeAreaTransform;

    public Func<Rect> jsGetSafeArea;
    public Action jsApplySafeArea;

    void Awake()
    {
        if (!helpers.Contains(this))
            helpers.Add(this);
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        rectTransform = GetComponent<RectTransform>();
        // the canvas/panel you want to react to the SafeArea needs to be called "SafeArea",
        // and be a child of the top canvas where this script is attached
        safeAreaTransform = transform.Find("SafeArea") as RectTransform;
        if (!screenChangeVarsInitialized)
        {
            lastOrientation = Screen.orientation;
            lastResolution.x = Screen.width;
            lastResolution.y = Screen.height;
            lastSafeArea = Screen.safeArea;
            screenChangeVarsInitialized = true;
        }
        cacheScreenData.x = Screen.width;
        cacheScreenData.y = Screen.height;
        //jsGetSafeArea = JSApplication.inst().env.Eval<Func<Rect>>("globalThis.GetSafeArea");
        //jsApplySafeArea = JSApplication.inst().env.Eval<Action>("globalThis.ApplySafeArea");
    }

    void Start()
    {
        ApplySafeArea();
    }

    // void Update()
    // {
    //     if (helpers[0] != this)
    //         return;
    //     if (Screen.orientation != lastOrientation)
    //         OrientationChanged();
    //     if (Screen.safeArea != lastSafeArea)
    //         SafeAreaChanged();
    //     if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
    //             ResolutionChanged();
    // }


    Vector2 cacheScreenData = Vector2.zero;

    void Update()
    {
        if (cacheScreenData.x != Screen.width || cacheScreenData.y != Screen.height)
        {
            cacheScreenData.x = Screen.width;
            cacheScreenData.y = Screen.height;
            ApplySafeArea();
        }
    }

    // 确定是否是刘海屏设备
    public bool CheckIsNotchDevice()
    {
        var device = SystemInfo.deviceModel.ToUpper();
        var deviceList = GetNotchDeviceList();
        foreach (var devicesName in deviceList)
        {
            if (device.Contains(devicesName))
            {
                return true;
            }
        }
        return false;
    }
    //设备型号查询 https://www.gsmarena.com/
    public List<string> GetNotchDeviceList()
    {
        List<string> deviceList = new List<string>();

        #region XiaoMi
        deviceList.Add("MI 8");
        deviceList.Add("PBAT00");
        #endregion

        #region Vivo
        //BLU XL4
        deviceList.Add("BLU VIVO XL4");
        deviceList.Add("PBFM00");
        #endregion

        #region HUAWEI
        //P20
        deviceList.Add("EML-AL00");
        deviceList.Add("EML-TL00");
        deviceList.Add("EML-L09");
        deviceList.Add("EML-L29");
        //P20 Pro
        deviceList.Add("CLT-L09");
        deviceList.Add("CLT-L29");
        deviceList.Add("CLT-AL00");
        deviceList.Add("CLT-AL01");
        deviceList.Add("CLT-AL00l");
        deviceList.Add("CLT-TL00");
        deviceList.Add("CLT-TL01");
        //P20 Lite / Nova 3e
        deviceList.Add("ANE-LX1");
        deviceList.Add("ANE-LX2");
        deviceList.Add("ANE-LX3");
        deviceList.Add("ANE-LX2J");
        deviceList.Add("ANE-AL00");
        //P smart+ 2019 / Nova 3i
        deviceList.Add("INE-LX1r");
        deviceList.Add("INE-LX2");
        deviceList.Add("INE-LX2r");
        //P smart+ / Nova Lite 3
        deviceList.Add("POT-LX1");
        deviceList.Add("POT-LX3");
        // nova5Pro
        deviceList.Add("SEA-AL10");
        #endregion

        #region Honor
        //Honor 10 Lite
        deviceList.Add("HRY-LX1");
        deviceList.Add("HRY-LX2");
        deviceList.Add("HRY-LX1MEB");
        deviceList.Add("HRY-AL00 IN");
        deviceList.Add("HRY-AL00a");
        deviceList.Add("HRY-AL00");
        deviceList.Add("HRY-TL00");
        // Honor CX 9
        deviceList.Add("DUB-AL00a");
        //Honor 8C
        deviceList.Add("BKK-LX2");
        deviceList.Add("BKK-LX1");
        deviceList.Add("BKK-L21");
        deviceList.Add("BKK-AL00");
        deviceList.Add("BKK-TL00");
        deviceList.Add("BKK-AL10");
        //Honor 10
        deviceList.Add("COL-AL10");
        deviceList.Add("COL-L29");
        deviceList.Add("COL-L19");
        deviceList.Add("COL-TL10");
        //Honor 8X Max
        deviceList.Add("ARE-AL00");
        deviceList.Add("ARE-L22HN");
        deviceList.Add("ARE-AL10");
        //Honor 8X
        deviceList.Add("JSN-L22");
        deviceList.Add("JSN-L42");
        deviceList.Add("JSN-L11");
        deviceList.Add("JSN-L21");
        deviceList.Add("JSN-L23");
        deviceList.Add("JSN-AL00");
        deviceList.Add("JSN-TL00");
        deviceList.Add("JSN-AL00a");
        //Honor 9N (9i)
        deviceList.Add("LLD-AL20");
        deviceList.Add("LLD-AL30");
        //Honor Play
        deviceList.Add("COR-AL00");
        deviceList.Add("COR-AL10");
        deviceList.Add("COR-L29");
        deviceList.Add("COR-L09");
        #endregion

        return deviceList;
    }

    Rect GetSafeArea()
    {
        if (jsGetSafeArea != null)
        {
            var ret = jsGetSafeArea();
            if (ret != null)
            {
                return ret;
            }
        }
        var safeArea = Screen.safeArea;
#if UNITY_ANDROID
        if (SystemInfo.operatingSystem.Contains("Android OS 8"))
        {
            if (CheckIsNotchDevice())
            {
                safeArea.height += HeightOffset;
            }
        }
#endif
        return safeArea;
    }


    void ApplySafeArea(int fixedWidth = 0, int fixedHeight = 0)
    {
        var safeArea = GetSafeArea();
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;

        var curDpi = safeArea.height / safeArea.width;

        if (curDpi < 1334 / 750f)  // 平板方案，按高度适配
        {
            canvasScaler.matchWidthOrHeight = 1f;
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 0f;
        }

        if (safeAreaTransform == null)
            return;

        if (curDpi < 1334 / 750f)
        {
            // 按照平板方案进行适配
            safeAreaTransform.offsetMax = Vector2.zero;
            safeAreaTransform.offsetMin = Vector2.zero;
            safeAreaTransform.sizeDelta = new Vector2(750, 1334);
            safeAreaTransform.anchorMin = Vector2.one * 0.5f;
            safeAreaTransform.anchorMax = Vector2.one * 0.5f;
            safeAreaTransform.pivot = Vector2.one * 0.5f;
        }
        else
        {
            // 按照手机方案进行适配
            var maxDpi = 1624 / 750f;

            if (fixedWidth > 0 || fixedHeight > 0)
            {
                maxDpi = fixedHeight * 1.0f / fixedWidth;
            }

            var maxHeight = safeArea.width * maxDpi;
            var subHeight = 0f;
            if (safeArea.height > maxHeight)
            {
                subHeight = (safeArea.height - maxHeight) * 0.5f;
            }
            anchorMin.y += subHeight;
            anchorMax.y -= subHeight;

            anchorMin.x /= canvas.pixelRect.width;
            anchorMin.y /= canvas.pixelRect.height;
            anchorMax.x /= canvas.pixelRect.width;
            anchorMax.y /= canvas.pixelRect.height;
            safeAreaTransform.anchorMin = anchorMin;
            safeAreaTransform.anchorMax = anchorMax;
            safeAreaTransform.localPosition = Vector3.zero;
            safeAreaTransform.sizeDelta = Vector2.zero;
            safeAreaTransform.anchoredPosition = Vector2.zero;
        }
        jsApplySafeArea?.Invoke();


        string[] msgs = new string[] { "Applied SafeArea:",
        "\n Screen.orientation: ", Screen.orientation.ToString(),
        "\n Screen.safeArea: ", Screen.safeArea.ToString(),
        "\n Screen.width / height: (", Screen.width.ToString(), ", ", Screen.height.ToString(), ")",
        "\n canvas.pixelRect.size: ", canvas.pixelRect.size.ToString(),
        "\n safeArea: ", safeArea.ToString(),
        "\n safeTr.localPos: ", safeAreaTransform.localPosition.ToString(),
        "\n anchorMin: ", safeAreaTransform.anchorMin.ToString(),
        "\n anchorMax: ", safeAreaTransform.anchorMax.ToString(),
        "\n operatingSystem: ", SystemInfo.operatingSystem.ToString(),
        "\n deviceModel: ", SystemInfo.deviceModel.ToString(),
        "\n jsGetSafeArea: ", jsGetSafeArea?.ToString(),
        "\n jsApplySafeArea: ", jsApplySafeArea?.ToString(),
        };
        Debug.Log(string.Join(string.Empty, msgs));
    }

    void OnDestroy()
    {
        if (helpers != null && helpers.Contains(this))
            helpers.Remove(this);
        this.canvas = null;
        this.canvasScaler = null;
        this.rectTransform = null;
        this.safeAreaTransform = null;
    }

    private static void OrientationChanged()
    {
        Debug.Log("Orientation changed from " + lastOrientation + " to " + Screen.orientation + " at " + Time.time);

        lastOrientation = Screen.orientation;
        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;

        isLandscape = lastOrientation == ScreenOrientation.LandscapeLeft
#if UNITY_2021_1_OR_NEWER
            || lastOrientation == ScreenOrientation.LandscapeRight;
#else
            || lastOrientation == ScreenOrientation.LandscapeRight || lastOrientation == ScreenOrientation.Landscape;
#endif
        onOrientationChange.Invoke();

    }

    private static void ResolutionChanged()
    {
        if (lastResolution.x == Screen.width && lastResolution.y == Screen.height)
            return;

        Debug.Log("Resolution changed from " + lastResolution + " to (" + Screen.width + ", " + Screen.height + ") at " + Time.time);

        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;

        isLandscape = Screen.width > Screen.height;
        onResolutionChange.Invoke();
    }

    public static void SafeAreaChanged(int fixedWidth = 0, int fixedHeight = 0)
    {
        if (lastSafeArea == Screen.safeArea && fixedHeight == 0 && fixedHeight == 0)
            return;

        Debug.Log("Safe Area changed from " + lastSafeArea + " to " + Screen.safeArea.size + " at " + Time.time);

        lastSafeArea = Screen.safeArea;

        for (int i = 0; i < helpers.Count; i++)
        {
            helpers[i].ApplySafeArea(fixedWidth, fixedHeight);
        }
    }

    public static Vector2 GetCanvasSize()
    {
        return helpers[0].rectTransform.rect.size;
    }

    public static Vector2 GetSafeAreaSize()
    {
        for (int i = 0; i < helpers.Count; i++)
        {
            if (helpers[i].safeAreaTransform != null)
            {
                return helpers[i].safeAreaTransform.rect.size;
            }
        }

        return GetCanvasSize();
    }
}
