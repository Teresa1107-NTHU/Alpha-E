/*
 * Alpha-E 電源、Rough Pump 與 Cooler 控制腳本：
 * Power On 時讓 Power_Control_Group 發亮；
 * Rough Pump On 時讓 VacuumChamber_Group 發亮；
 * Cooler On 時讓 Cooler_Group 發亮，並使冷卻風扇旋轉。
 */

using System.Collections.Generic;
using UnityEngine;

public class AlphaEPowerController : MonoBehaviour
{
    [Header("Power Control 模型")]
    [SerializeField]
    private GameObject powerControlGroup;

    [Header("Rough Pump 對應區域")]
    [SerializeField]
    private GameObject vacuumChamberGroup;

    [Header("Cooler 對應區域")]
    [SerializeField]
    private GameObject coolerGroup;

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

    [Header("Power 發光設定")]
    [SerializeField]
    private Color powerEmissionColor =
        new Color(0.1f, 0.8f, 1f);

    [SerializeField]
    private float powerEmissionIntensity = 1f;

    [Header("Rough Pump 發光設定")]
    [SerializeField]
    private Color roughPumpEmissionColor =
        new Color(0.15f, 0.65f, 1f);

    [SerializeField]
    private float roughPumpEmissionIntensity = 1f;

    [Header("Cooler 發光設定")]
    [SerializeField]
    private Color coolerEmissionColor =
    new Color(0.2f, 0.9f, 1f);

    [SerializeField]
    private float coolerEmissionIntensity = 1f;

    [Header("已完成步驟發光設定")]
    [SerializeField]
    private Color completedEmissionColor =
    new Color(0.2f, 1.0f, 0.2f);

    [SerializeField]
    private float completedEmissionIntensity = 1f;



    private bool isPowerOn;
    private bool isRoughPumpOn;
    private bool isCoolerOn;
    private float currentFanSpeed;

    private readonly List<Material> powerMaterials =
        new List<Material>();

    private readonly List<Color> powerOriginalEmissionColors =
        new List<Color>();

    private readonly List<Material> vacuumMaterials =
        new List<Material>();

    private readonly List<Color> vacuumOriginalEmissionColors =
        new List<Color>();

    private readonly List<Material> coolerMaterials =
    new List<Material>();

    private readonly List<Color> coolerOriginalEmissionColors =
        new List<Color>();

    private static readonly int EmissionColorId =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        CacheMaterials(
            powerControlGroup,
            powerMaterials,
            powerOriginalEmissionColors,
            "Power_Control_Group"
        );

        CacheMaterials(
            vacuumChamberGroup,
            vacuumMaterials,
            vacuumOriginalEmissionColors,
            "VacuumChamber_Group"
        );

        CacheMaterials(
            coolerGroup,
            coolerMaterials,
            coolerOriginalEmissionColors,
            "Cooler_Group"
        );

        ApplyEmission(
            powerMaterials,
            powerOriginalEmissionColors,
            false,
            powerEmissionColor,
            powerEmissionIntensity
        );

        ApplyEmission(
            vacuumMaterials,
            vacuumOriginalEmissionColors,
            false,
            roughPumpEmissionColor,
            roughPumpEmissionIntensity
        );

        isCoolerOn = false;

        ApplyEmission(
            coolerMaterials,
            coolerOriginalEmissionColors,
            false,
            coolerEmissionColor,
            coolerEmissionIntensity
        );
    }

    private void Update()
    {
        UpdateFanRotation();
    }

    /*
    * 控制 Alpha-E 總電源。
    *
    * Power On：
    * Power_Control_Group 以青藍色發亮。
    *
     * Power Off：
    * 關閉 Power、Rough Pump、Cooler 的視覺狀態，
    * 並讓 Cooler 風扇逐漸停止。
    */
    public void SetPower(string command)
    {
        bool turnOn =
            command.Trim().ToLower() == "on";

        isPowerOn = turnOn;

        if (turnOn)
        {
            ApplyEmission(
                powerMaterials,
                powerOriginalEmissionColors,
                true,
                powerEmissionColor,
                powerEmissionIntensity
            );
        }
        else
        {
            // 關閉 Power 發光
            ApplyEmission(
                powerMaterials,
                powerOriginalEmissionColors,
                false,
                powerEmissionColor,
                powerEmissionIntensity
            );

            // 關閉 Rough Pump 對應區域
            ApplyEmission(
                vacuumMaterials,
                vacuumOriginalEmissionColors,
                false,
                roughPumpEmissionColor,
                roughPumpEmissionIntensity
            );

            // 關閉 Cooler 對應區域
            ApplyEmission(
                coolerMaterials,
                coolerOriginalEmissionColors,
                false,
                coolerEmissionColor,
                coolerEmissionIntensity
            );

            // 重設所有子系統狀態
            isRoughPumpOn = false;
            isCoolerOn = false;
        }

        Debug.Log(
            $"Alpha-E Power：{(turnOn ? "On" : "Off")}"
        );
    }

    /*
    * 控制 Rough Pump 教學步驟。
    *
    * Rough Pump On：
    * 1. Power_Control_Group 改成綠色，表示電源步驟已完成。
    * 2. VacuumChamber_Group 以青藍色發亮，表示目前正在抽真空。
    *
    * Rough Pump Off：
    * 1. VacuumChamber_Group 恢復原色。
    * 2. 若總電源仍開啟，Power_Control_Group 回到青藍色。
    */
    public void SetRoughPump(string command)
    {
        bool turnOn =
            command.Trim().ToLower() == "on";

        if (turnOn && !isPowerOn)
        {
            Debug.LogWarning(
                "無法啟動 Rough Pump：請先開啟 Power。"
            );

            return;
        }

        isRoughPumpOn = turnOn;

        if (turnOn)
        {
            // Power 已完成，但仍保持啟動
            ApplyEmission(
                powerMaterials,
                powerOriginalEmissionColors,
                true,
                completedEmissionColor,
                completedEmissionIntensity
            );

            // Rough Pump 是目前操作步驟
            ApplyEmission(
                vacuumMaterials,
                vacuumOriginalEmissionColors,
                true,
                roughPumpEmissionColor,
                roughPumpEmissionIntensity
            );
        }
        else
        {
            // Rough Pump 關閉
            ApplyEmission(
                vacuumMaterials,
                vacuumOriginalEmissionColors,
                false,
                roughPumpEmissionColor,
                roughPumpEmissionIntensity
            );

            // 總電源仍然開啟，回到 Power 當前步驟
            if (isPowerOn)
            {
                ApplyEmission(
                    powerMaterials,
                    powerOriginalEmissionColors,
                    true,
                    powerEmissionColor,
                    powerEmissionIntensity
                );
            }
        }

        Debug.Log(
            $"Rough Pump：{(turnOn ? "On" : "Off")}"
        );
    }

    /*
 * 控制 Cooler：
 * Cooler On 時讓 Cooler_Group 發亮，
 * 並讓 CoolingFan 慢慢加速旋轉。
 */
    public void SetCooler(string command)
    {
        bool turnOn =
            command.Trim().ToLower() == "on";

        if (turnOn && !isPowerOn)
        {
            Debug.LogWarning(
                "無法啟動 Cooler：請先開啟 Power。"
            );

            return;
        }

        isCoolerOn = turnOn;

        ApplyEmission(
            coolerMaterials,
            coolerOriginalEmissionColors,
            turnOn,
            coolerEmissionColor,
            coolerEmissionIntensity
        );

        Debug.Log(
            $"Cooler：{(turnOn ? "On" : "Off")}"
        );
    }

    private void UpdateFanRotation()
    {
        float targetSpeed =
            isCoolerOn ? targetFanSpeed : 0f;



        currentFanSpeed = Mathf.MoveTowards(
            currentFanSpeed,
            targetSpeed,
            fanAcceleration * Time.deltaTime
        );

        if (
            coolerFan == null ||
            Mathf.Approximately(currentFanSpeed, 0f)
        )
        {
            return;
        }

        coolerFan.Rotate(
            fanRotationAxis.normalized,
            currentFanSpeed * Time.deltaTime,
            Space.Self
        );
    }

    
    private void CacheMaterials(
        GameObject targetGroup,
        List<Material> materialList,
        List<Color> originalColorList,
        string groupName
    )
    {
        materialList.Clear();
        originalColorList.Clear();

        if (targetGroup == null)
        {
            Debug.LogError(
                $"尚未指定 {groupName}。"
            );

            return;
        }

        Renderer[] renderers =
            targetGroup.GetComponentsInChildren<Renderer>(
                true
            );

        foreach (Renderer targetRenderer in renderers)
        {
            foreach (
                Material material in targetRenderer.materials
            )
            {
                if (
                    !material.HasProperty(
                        EmissionColorId
                    )
                )
                {
                    continue;
                }

                materialList.Add(material);

                originalColorList.Add(
                    material.GetColor(
                        EmissionColorId
                    )
                );
            }
        }

        Debug.Log(
            $"{groupName} 找到 {materialList.Count} 個可發光材質。"
        );
    }

    private void ApplyEmission(
        List<Material> materials,
        List<Color> originalColors,
        bool enabled,
        Color emissionColor,
        float intensity
    )
    {
        for (int i = 0; i < materials.Count; i++)
        {
            Material material = materials[i];

            if (enabled)
            {
                material.EnableKeyword("_EMISSION");

                material.SetColor(
                    EmissionColorId,
                    emissionColor * intensity
                );
            }
            else
            {
                material.SetColor(
                    EmissionColorId,
                    originalColors[i]
                );
            }
        }
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

    [ContextMenu("Test Rough Pump On")]
    private void TestRoughPumpOn()
    {
        SetRoughPump("on");
    }

    [ContextMenu("Test Rough Pump Off")]
    private void TestRoughPumpOff()
    {
        SetRoughPump("off");
    }

    [ContextMenu("Test Cooler On")]
    private void TestCoolerOn()
    {
        SetCooler("on");
    }

    [ContextMenu("Test Cooler Off")]
    private void TestCoolerOff()
    {
        SetCooler("off");
    }
}