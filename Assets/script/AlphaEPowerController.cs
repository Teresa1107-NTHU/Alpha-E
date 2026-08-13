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
    [Header("教學流程管理器")]
    [SerializeField]
    private AlphaEFlowManager flowManager;


    [Header("Power Control 模型")]
    [SerializeField]
    private GameObject powerControlGroup;

    [Header("Rough Pump 對應區域")]
    [SerializeField]
    private GameObject vacuumChamberGroup;

    [Header("Cooler 對應區域")]
    [SerializeField]
    private GameObject coolerGroup;

    [Header("Gas Supply 對應區域")]
    [SerializeField]
    private GameObject gasSupplyGroup;

    [Header("Cooler 風扇葉片")]
    [SerializeField]
    private Transform coolerFan;

    [Header("High Voltage 對應區域")]
    [SerializeField]
    private GameObject highVoltageGroup;

    [Header("Microwave 對應區域")]
    [SerializeField]
    private GameObject microwaveGroup;

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

    [Header("Turbo Pump 發光設定")]
    [SerializeField]
    private Color turboPumpEmissionColor =
    new Color(0.25f, 0.25f, 0.85f);

    [SerializeField]
    private float turboPumpEmissionIntensity = 1f;

    [Header("Gas Supply 發光設定")]
    [SerializeField]
    private Color gasSupplyEmissionColor =
    new Color(1.0f, 0.55f, 0.10f);

    [SerializeField]
    private float gasSupplyEmissionIntensity = 1f;

    [Header("MFC 發光設定")]
    [SerializeField]
    private Color mfcEmissionColor =
    new Color(0.35f, 0.8f, 0.15f);

    [SerializeField]
    private float mfcEmissionIntensity = 1f;

    [Header("Cooler 發光設定")]
    [SerializeField]
    private Color coolerEmissionColor =
    new Color(0.2f, 0.9f, 1f);

    [SerializeField]
    private float coolerEmissionIntensity = 1f;

    [Header("High Voltage 發光設定")]
    [SerializeField]
    private Color highVoltageEmissionColor =
    new Color(1.0f, 0.55f, 0.05f);

    [SerializeField]
    private float highVoltageEmissionIntensity = 1f;

    [Header("Microwave 發光設定")]
    [SerializeField]
    private Color microwaveEmissionColor =
    new Color(
        0.75f,
        0.2f,
        1f
    );

    [SerializeField]
    private float microwaveEmissionIntensity = 1f;

    [Header("已完成步驟發光設定")]
    [SerializeField]
    private Color completedEmissionColor =
    new Color(0.05f, 0.18f, 0.40f);

    [SerializeField]
    private float completedEmissionIntensity = 0.3f;


    private bool isPowerOn;
    private bool isRoughPumpOn;
    private bool isTurboPumpOn;
    private bool isGasSupplyOn;
    private bool isMfcOn;
    private bool isCoolerOn;
    private bool isHighVoltageOn;
    private bool isMicrowaveOn;

    private string currentGasType = "";
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

    private readonly List<Material> gasSupplyMaterials =
    new List<Material>();

    private readonly List<Color> gasSupplyOriginalEmissionColors =
        new List<Color>();

    private readonly List<Material> highVoltageMaterials =
    new List<Material>();

    private readonly List<Color> highVoltageOriginalEmissionColors =
        new List<Color>();

    private readonly List<Material> microwaveMaterials =
    new List<Material>();

    private readonly List<Color> microwaveOriginalEmissionColors =
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

        CacheMaterials(
            gasSupplyGroup,
            gasSupplyMaterials,
            gasSupplyOriginalEmissionColors,
            "Gas Supply Group"
        );

        CacheMaterials(
            highVoltageGroup,
            highVoltageMaterials,
            highVoltageOriginalEmissionColors,
            "High Voltage Group"
        );

        CacheMaterials(
            microwaveGroup,
            microwaveMaterials,
            microwaveOriginalEmissionColors,
            "Microwave Group"
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

        ApplyEmission(
            gasSupplyMaterials,
            gasSupplyOriginalEmissionColors,
            false,
            gasSupplyEmissionColor,
            gasSupplyEmissionIntensity
        );

        ApplyEmission(
            highVoltageMaterials,
            highVoltageOriginalEmissionColors,
            false,
            highVoltageEmissionColor,
            highVoltageEmissionIntensity
        );

        ApplyEmission(
            microwaveMaterials,
            microwaveOriginalEmissionColors,
            false,
            microwaveEmissionColor,
            microwaveEmissionIntensity
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

        if (
        turnOn &&
        flowManager != null &&
        !flowManager.CanOperate(
            AlphaEFlowManager.AlphaEStep.Power
        )
    )
        {
            return;
        }

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
            if (flowManager != null)
            {
                flowManager.CompleteStep(
                    AlphaEFlowManager.AlphaEStep.Power
                );
            }
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

            ApplyEmission(
                gasSupplyMaterials,
                gasSupplyOriginalEmissionColors,
                false,
                gasSupplyEmissionColor,
                gasSupplyEmissionIntensity
            );

            ApplyEmission(
            highVoltageMaterials,
            highVoltageOriginalEmissionColors,
            false,
            highVoltageEmissionColor,
            highVoltageEmissionIntensity
            );

            ApplyEmission(
                microwaveMaterials,
                microwaveOriginalEmissionColors,
                false,
                microwaveEmissionColor,
                microwaveEmissionIntensity
            );

            // 重設所有子系統狀態
            isRoughPumpOn = false;
            isTurboPumpOn = false;
            isGasSupplyOn = false;
            isMfcOn = false;
            isCoolerOn = false;
            isHighVoltageOn = false;
            isMicrowaveOn = false;

            currentGasType = "";

            if (flowManager != null)
            {
                flowManager.ResetFlow();
            }
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

        if (
            turnOn &&
            flowManager != null &&
            !flowManager.CanOperate(
            AlphaEFlowManager.AlphaEStep.RoughPump
            )
)
        {
            return;
        }

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

            if (flowManager != null)
            {
                flowManager.CompleteStep(
                    AlphaEFlowManager.AlphaEStep.RoughPump
                );
            }
        }
        else
        {
            // Rough Pump 關閉時，Turbo Pump 也必須停止
            isTurboPumpOn = false;

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
    * 控制 Turbo Pump 高真空步驟。
    *
    * Turbo Pump On：
    * 1. 必須先完成 Rough Pump。
    * 2. VacuumChamber_Group 改成藍紫色，
    *    表示系統進入高真空階段。
    * 3. 流程推進到 Gas Supply。
    *
    * Turbo Pump Off：
    * 若 Rough Pump 仍在運作，
    * VacuumChamber_Group 改成淡暗藍色完成狀態。
    */
    public void SetTurboPump(string command)
    {
        bool turnOn =
            command.Trim().ToLower() == "on";

        if (
            turnOn &&
            flowManager != null &&
            !flowManager.CanOperate(
                AlphaEFlowManager.AlphaEStep.TurboPump
            )
        )
        {
            return;
        }

        if (turnOn && !isPowerOn)
        {
            Debug.LogWarning(
                "無法啟動 Turbo Pump：請先開啟 Power。"
            );

            return;
        }

        if (turnOn && !isRoughPumpOn)
        {
            Debug.LogWarning(
                "無法啟動 Turbo Pump：請先啟動 Rough Pump。"
            );

            return;
        }

        isTurboPumpOn = turnOn;

        if (turnOn)
        {
            // Vacuum Chamber 切換成高真空階段顏色
            ApplyEmission(
                vacuumMaterials,
                vacuumOriginalEmissionColors,
                true,
                turboPumpEmissionColor,
                turboPumpEmissionIntensity
            );

            if (flowManager != null)
            {
                flowManager.CompleteStep(
                    AlphaEFlowManager.AlphaEStep.TurboPump
                );
            }
        }
        else
        {
            if (isRoughPumpOn)
            {
                // Rough Pump 仍在運作，
                // 真空系統改成淡暗藍色完成狀態
                ApplyEmission(
                    vacuumMaterials,
                    vacuumOriginalEmissionColors,
                    true,
                    completedEmissionColor,
                    completedEmissionIntensity
                );
            }
            else
            {
                // Rough Pump 也已關閉，恢復原材質
                ApplyEmission(
                    vacuumMaterials,
                    vacuumOriginalEmissionColors,
                    false,
                    turboPumpEmissionColor,
                    turboPumpEmissionIntensity
                );
            }
        }

        Debug.Log(
            $"Turbo Pump：{(turnOn ? "On" : "Off")}"
        );
    }

    /*
    * Gas Supply 控制：
    * 接收網頁傳來的氣體種類，例如 Deuterium、Hydrogen 或 Argon。
    *
    * 傳入 "off" 時關閉；
    * 傳入氣體名稱時代表完成氣體設定並啟動 Gas Supply。
    */
    public void SetGasSupply(string command)
    {
        string value = command.Trim();

        bool turnOn =
            !string.IsNullOrEmpty(value) &&
            value.ToLower() != "off";

        if (
            turnOn &&
            flowManager != null &&
            !flowManager.CanOperate(
                AlphaEFlowManager.AlphaEStep.GasSupply
            )
        )
        {
            return;
        }

        if (turnOn && !isPowerOn)
        {
            Debug.LogWarning(
                "無法設定 Gas Supply：請先開啟 Power。"
            );

            return;
        }

        if (turnOn && !isTurboPumpOn)
        {
            Debug.LogWarning(
                "無法設定 Gas Supply：請先完成 Turbo Pump 高真空步驟。"
            );

            return;
        }

        isGasSupplyOn = turnOn;

        if (turnOn)
        {
            currentGasType = value;

            // 真空系統已完成，保留淡暗藍色
            ApplyEmission(
                vacuumMaterials,
                vacuumOriginalEmissionColors,
                true,
                completedEmissionColor,
                completedEmissionIntensity
            );

            // Gas Supply 是目前步驟，使用橘黃色高亮
            ApplyEmission(
                gasSupplyMaterials,
                gasSupplyOriginalEmissionColors,
                true,
                gasSupplyEmissionColor,
                gasSupplyEmissionIntensity
            );

            if (flowManager != null)
            {
                flowManager.CompleteStep(
                    AlphaEFlowManager.AlphaEStep.GasSupply
                );
            }
        }
        else
        {
            currentGasType = "";
            isMfcOn = false;

            ApplyEmission(
                gasSupplyMaterials,
                gasSupplyOriginalEmissionColors,
                false,
                gasSupplyEmissionColor,
                gasSupplyEmissionIntensity
            );
        }

        Debug.Log(
            turnOn
                ? $"Gas Supply：On，氣體種類：{currentGasType}"
                : "Gas Supply：Off"
        );
    }

    /*
     * 控制 MFC（質量流量控制器）。
     *
     * MFC On：
     * 1. 必須先完成 Gas Supply。
     * 2. Gas_MFC_Group 改成黃綠色高亮，
     *    表示正在精確控制氣體流量。
     * 3. 流程推進到 Cooler。
     *
     * MFC Off：
     * 若 Gas Supply 仍維持設定，
     * 回到 Gas Supply 的橘黃色狀態。
     */
    public void SetMFC(string command)
    {
        bool turnOn =
            command.Trim().ToLower() == "on";

        if (
            turnOn &&
            flowManager != null &&
            !flowManager.CanOperate(
                AlphaEFlowManager.AlphaEStep.MFC
            )
        )
        {
            return;
        }

        if (turnOn && !isPowerOn)
        {
            Debug.LogWarning(
                "無法啟動 MFC：請先開啟 Power。"
            );

            return;
        }

        if (turnOn && !isGasSupplyOn)
        {
            Debug.LogWarning(
                "無法啟動 MFC：請先完成 Gas Supply。"
            );

            return;
        }

        isMfcOn = turnOn;

        if (turnOn)
        {
            // MFC 為目前操作步驟
            ApplyEmission(
                gasSupplyMaterials,
                gasSupplyOriginalEmissionColors,
                true,
                mfcEmissionColor,
                mfcEmissionIntensity
            );

            if (flowManager != null)
            {
                flowManager.CompleteStep(
                    AlphaEFlowManager.AlphaEStep.MFC
                );
            }
        }
        else
        {
            if (isGasSupplyOn)
            {
                // MFC 關閉，但氣體仍已設定
                ApplyEmission(
                    gasSupplyMaterials,
                    gasSupplyOriginalEmissionColors,
                    true,
                    gasSupplyEmissionColor,
                    gasSupplyEmissionIntensity
                );
            }
            else
            {
                // Gas Supply 也關閉，恢復原材質
                ApplyEmission(
                    gasSupplyMaterials,
                    gasSupplyOriginalEmissionColors,
                    false,
                    mfcEmissionColor,
                    mfcEmissionIntensity
                );
            }
        }

        Debug.Log(
            $"MFC：{(turnOn ? "On" : "Off")}"
        );
    }

    /*
     * 控制 Cooler：
     * Cooler On 時讓 Cooler_Group 發亮，
     * 並讓 CoolingFan 慢慢加速旋轉。
     *
     * 流程順序：
     * 必須先完成 MFC，完成後推進到 High Voltage。
     */
    public void SetCooler(string command)
    {
        bool turnOn =
            command.Trim().ToLower() == "on";

        // 只有開啟時才檢查目前流程步驟
        if (
            turnOn &&
            flowManager != null &&
            !flowManager.CanOperate(
                AlphaEFlowManager.AlphaEStep.Cooler
            )
        )
        {
            return;
        }

        if (turnOn && !isPowerOn)
        {
            Debug.LogWarning(
                "無法啟動 Cooler：請先開啟 Power。"
            );

            return;
        }

        if (turnOn && !isMfcOn)
        {
            Debug.LogWarning(
                "無法啟動 Cooler：請先啟動 MFC。"
            );

            return;
        }

        isCoolerOn = turnOn;

        if (turnOn)
        {
            // MFC 已完成，但仍持續控制流量
            ApplyEmission(
                gasSupplyMaterials,
                gasSupplyOriginalEmissionColors,
                true,
                completedEmissionColor,
                completedEmissionIntensity
            );

            // Cooler 是目前操作步驟
            ApplyEmission(
                coolerMaterials,
                coolerOriginalEmissionColors,
                true,
                coolerEmissionColor,
                coolerEmissionIntensity
            );

            if (flowManager != null)
            {
                flowManager.CompleteStep(
                    AlphaEFlowManager.AlphaEStep.Cooler
                );
            }
        }
        else
        {
            isHighVoltageOn = false;

            ApplyEmission(
                coolerMaterials,
                coolerOriginalEmissionColors,
                false,
                coolerEmissionColor,
                coolerEmissionIntensity
            );

            ApplyEmission(
                highVoltageMaterials,
                highVoltageOriginalEmissionColors,
                false,
                highVoltageEmissionColor,
                highVoltageEmissionIntensity
            );
        }

        Debug.Log(
            $"Cooler：{(turnOn ? "On" : "Off")}"
        );
    }

    /*
     * 控制 High Voltage：
     * 必須先完成 MFC 與 Cooler。
     *
     * High Voltage On：
     * 1. Cooler_Group 改成淡暗藍色完成狀態。
     * 2. IonSource_Group 使用紅橘色發亮。
     * 3. 流程推進到 Microwave。
     */
    public void SetHighVoltage(string command)
    {
        bool turnOn =
            command.Trim().ToLower() == "on";

        if (
            turnOn &&
            flowManager != null &&
            !flowManager.CanOperate(
                AlphaEFlowManager.AlphaEStep.HighVoltage
            )
        )
        {
            return;
        }

        if (turnOn && !isPowerOn)
        {
            Debug.LogWarning(
                "無法啟動 High Voltage：請先開啟 Power。"
            );

            return;
        }

        if (turnOn && !isMfcOn)
        {
            Debug.LogWarning(
                "無法啟動 High Voltage：請先啟動 MFC。"
            );

            return;
        }

        if (turnOn && !isCoolerOn)
        {
            Debug.LogWarning(
                "無法啟動 High Voltage：請先啟動 Cooler。"
            );

            return;
        }

        isHighVoltageOn = turnOn;

        if (turnOn)
        {
            // Cooler 已完成，保留淡暗藍色
            
            ApplyEmission(
                coolerMaterials,
                coolerOriginalEmissionColors,
                true,
                completedEmissionColor,
                completedEmissionIntensity
            );
            

            // High Voltage 為目前操作步驟
            ApplyEmission(
                highVoltageMaterials,
                highVoltageOriginalEmissionColors,
                true,
                highVoltageEmissionColor,
                highVoltageEmissionIntensity
            );

            if (flowManager != null)
            {
                flowManager.CompleteStep(
                    AlphaEFlowManager.AlphaEStep.HighVoltage
                );
            }
        }
        else
        {
            ApplyEmission(
                highVoltageMaterials,
                highVoltageOriginalEmissionColors,
                false,
                highVoltageEmissionColor,
                highVoltageEmissionIntensity
            );

            // Cooler 仍在運作，恢復成目前設備顏色
            
            if (isCoolerOn)
            {
                ApplyEmission(
                    coolerMaterials,
                    coolerOriginalEmissionColors,
                    true,
                    coolerEmissionColor,
                    coolerEmissionIntensity
                );
            }
            
        }

        Debug.Log(
            $"High Voltage：{(turnOn ? "On" : "Off")}"
        );
    }

    /*
     * 控制 Microwave：
     *
     * 必須先完成 High Voltage。
     *
     * Microwave On：
     * 1. High Voltage 維持原本發光，表示仍持續運作。
     * 2. Microwave_RF_Group 紫色發亮。
     * 3. 流程推進到 Beam。
     */
    public void SetMicrowave(string command)
    {
        bool turnOn =
            command.Trim().ToLower() == "on";

        if (
            turnOn &&
            flowManager != null &&
            !flowManager.CanOperate(
                AlphaEFlowManager.AlphaEStep.Microwave
            )
        )
        {
            return;
        }

        if (turnOn && !isPowerOn)
        {
            Debug.LogWarning(
                "請先開啟 Power。"
            );

            return;
        }

        if (turnOn && !isHighVoltageOn)
        {
            Debug.LogWarning(
                "請先完成 High Voltage。"
            );

            return;
        }

        isMicrowaveOn = turnOn;

        if (turnOn)
        {
            // High Voltage 保持原本亮色，不修改

            ApplyEmission(
                microwaveMaterials,
                microwaveOriginalEmissionColors,
                true,
                microwaveEmissionColor,
                microwaveEmissionIntensity
            );

            if (flowManager != null)
            {
                flowManager.CompleteStep(
                    AlphaEFlowManager.AlphaEStep.Microwave
                );
            }
        }
        else
        {
            ApplyEmission(
                microwaveMaterials,
                microwaveOriginalEmissionColors,
                false,
                microwaveEmissionColor,
                microwaveEmissionIntensity
            );

            // High Voltage 仍然是 On，所以繼續亮
            if (isHighVoltageOn)
            {
                ApplyEmission(
                    highVoltageMaterials,
                    highVoltageOriginalEmissionColors,
                    true,
                    highVoltageEmissionColor,
                    highVoltageEmissionIntensity
                );
            }
        }

        Debug.Log(
            $"Microwave：{(turnOn ? "On" : "Off")}"
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

    [ContextMenu("Test Turbo Pump On")]
    private void TestTurboPumpOn()
    {
        SetTurboPump("on");
    }

    [ContextMenu("Test Turbo Pump Off")]
    private void TestTurboPumpOff()
    {
        SetTurboPump("off");
    }

    [ContextMenu("Test Gas Supply On")]
    private void TestGasSupplyOn()
    {
        SetGasSupply("Deuterium");
    }

    [ContextMenu("Test Gas Supply Off")]
    private void TestGasSupplyOff()
    {
        SetGasSupply("off");
    }

    [ContextMenu("Test MFC On")]
    private void TestMFCOn()
    {
        SetMFC("on");
    }

    [ContextMenu("Test MFC Off")]
    private void TestMFCOff()
    {
        SetMFC("off");
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

    [ContextMenu("Test High Voltage On")]
    private void TestHighVoltageOn()
    {
        SetHighVoltage("on");
    }

    [ContextMenu("Test High Voltage Off")]
    private void TestHighVoltageOff()
    {
        SetHighVoltage("off");
    }

    [ContextMenu("Test Microwave On")]
    private void TestMicrowaveOn()
    {
        SetMicrowave("on");
    }

    [ContextMenu("Test Microwave Off")]
    private void TestMicrowaveOff()
    {
        SetMicrowave("off");
    }

    /*
     * 模擬網頁使用 SendMessage 呼叫 SetPower，
     * 用來確認物件名稱與腳本掛載是否正確。
     */
    [ContextMenu("Test Web SendMessage")]
    private void TestWebSendMessage()
    {
        GameObject target =
            GameObject.Find("AlphaEWebController");

        if (target == null)
        {
            Debug.LogError(
                "找不到名為 AlphaEWebController 的物件。"
            );
            return;
        }

        AlphaEPowerController controller =
            target.GetComponent<AlphaEPowerController>();

        if (controller == null)
        {
            Debug.LogError(
                "找到了 AlphaEWebController，" +
                "但上面沒有 AlphaEPowerController 元件。"
            );
            return;
        }

        target.SendMessage(
            "SetPower",
            "on",
            SendMessageOptions.RequireReceiver
        );

        Debug.Log(
            "SendMessage 測試成功。"
        );
    }
}