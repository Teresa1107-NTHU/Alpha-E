/*
 * Alpha-E 電源控制腳本：
 * 接收網頁傳來的 Power On / Off 指令。
 * Power On 時讓 Power_Control_Group 發亮，並使 Cooler 風扇緩慢旋轉。
 */

using System.Collections.Generic;
using UnityEngine;

public class AlphaEPowerController : MonoBehaviour
{
    [Header("Power Control 模型")]
    [SerializeField]
    private GameObject powerControlGroup;

    [Header("Cooler 風扇葉片")]
    [SerializeField]
    private Transform coolerFan;

    [Header("風扇旋轉設定")]
    [SerializeField]
    private Vector3 fanRotationAxis = Vector3.forward;

    [SerializeField]
    private float targetFanSpeed = 120f;

    [SerializeField]
    private float fanAcceleration = 45f;

    [Header("發光設定")]
    [SerializeField]
    private Color powerEmissionColor = new Color(0.1f, 0.8f, 1f);

    [SerializeField]
    private float emissionIntensity = 3f;

    private bool isPowerOn;
    private float currentFanSpeed;

    private readonly List<Material> powerMaterials = new List<Material>();
    private readonly List<Color> originalEmissionColors = new List<Color>();

    private static readonly int EmissionColorId =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        CachePowerMaterials();
        ApplyPowerEmission(false);
    }

    private void Update()
    {
        UpdateFanRotation();
    }

    /*
     * 由網頁呼叫：
     * 傳入 "on" 或 "off"。
     */
    public void SetPower(string command)
    {
        bool turnOn = command.Trim().ToLower() == "on";

        isPowerOn = turnOn;
        ApplyPowerEmission(turnOn);

        Debug.Log($"Alpha-E Power：{(turnOn ? "On" : "Off")}");
    }

    [ContextMenu("Test Power On")]
    private void TestPowerOn()
    {
        SetPower("on");
    }

    [ContextMenu("Test Power Off")]
    private void TestPowerOff()
    {
        SetPower("off");
    }

    private void UpdateFanRotation()
    {
        float targetSpeed = isPowerOn ? targetFanSpeed : 0f;

        currentFanSpeed = Mathf.MoveTowards(
            currentFanSpeed,
            targetSpeed,
            fanAcceleration * Time.deltaTime
        );

        if (coolerFan == null || Mathf.Approximately(currentFanSpeed, 0f))
        {
            return;
        }

        coolerFan.Rotate(
            fanRotationAxis.normalized,
            currentFanSpeed * Time.deltaTime,
            Space.Self
        );
    }

    private void CachePowerMaterials()
    {
        if (powerControlGroup == null)
        {
            Debug.LogError("尚未指定 Power_Control_Group。");
            return;
        }

        Renderer[] renderers =
            powerControlGroup.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer targetRenderer in renderers)
        {
            foreach (Material material in targetRenderer.materials)
            {
                if (!material.HasProperty(EmissionColorId))
                {
                    continue;
                }

                powerMaterials.Add(material);
                originalEmissionColors.Add(
                    material.GetColor(EmissionColorId)
                );
            }
        }
    }

    private void ApplyPowerEmission(bool enabled)
    {
        for (int i = 0; i < powerMaterials.Count; i++)
        {
            Material material = powerMaterials[i];

            if (enabled)
            {
                material.EnableKeyword("_EMISSION");

                Color emission =
                    powerEmissionColor * emissionIntensity;

                material.SetColor(EmissionColorId, emission);
            }
            else
            {
                material.SetColor(
                    EmissionColorId,
                    originalEmissionColors[i]
                );
            }
        }
    }
}