/*
 * Alpha-E 教學流程管理器：
 * 記錄目前操作步驟、檢查操作順序，
 * 並在完成設備操作後推進到下一個步驟。
 */

using UnityEngine;

public class AlphaEFlowManager : MonoBehaviour
{
    public enum AlphaEStep
    {
        Power,
        RoughPump,
        TurboPump,
        GasSupply,
        MFC,
        Cooler,
        HighVoltage,
        Microwave,
        Beam,
        Fusion,
        Completed
    }

    [Header("流程設定")]
    [SerializeField]
    private bool enforceOrder = true;

    [SerializeField]
    private AlphaEStep currentStep = AlphaEStep.Power;

    public AlphaEStep CurrentStep => currentStep;

    /*
     * 判斷目前是否允許操作指定設備。
     *
     * enforceOrder 關閉時，所有設備都可自由測試。
     */
    public bool CanOperate(AlphaEStep requestedStep)
    {
        if (!enforceOrder)
        {
            return true;
        }

        if (requestedStep == currentStep)
        {
            return true;
        }

        Debug.LogWarning(
            $"目前應操作：{GetStepDisplayName(currentStep)}，" +
            $"不能先操作：{GetStepDisplayName(requestedStep)}"
        );

        return false;
    }

    /*
     * 完成目前步驟後，自動推進至下一步。
     */
    public void CompleteStep(AlphaEStep completedStep)
    {
        if (enforceOrder && completedStep != currentStep)
        {
            Debug.LogWarning(
                $"無法完成 {GetStepDisplayName(completedStep)}，" +
                $"目前步驟是 {GetStepDisplayName(currentStep)}。"
            );

            return;
        }

        currentStep = GetNextStep(completedStep);

        Debug.Log(
            $"已完成：{GetStepDisplayName(completedStep)}；" +
            $"下一步：{GetStepDisplayName(currentStep)}"
        );
    }

    /*
     * 回到流程起點。
     */
    public void ResetFlow()
    {
        currentStep = AlphaEStep.Power;

        Debug.Log("Alpha-E 教學流程已重設。");
    }

    /*
     * 暫時手動跳到指定步驟，方便 Unity 內測試。
     */
    public void SetCurrentStep(AlphaEStep step)
    {
        currentStep = step;

        Debug.Log(
            $"目前流程步驟已設為：{GetStepDisplayName(currentStep)}"
        );
    }

    private AlphaEStep GetNextStep(AlphaEStep step)
    {
        switch (step)
        {
            case AlphaEStep.Power:
                return AlphaEStep.RoughPump;

            case AlphaEStep.RoughPump:
                return AlphaEStep.TurboPump;

            case AlphaEStep.TurboPump:
                return AlphaEStep.GasSupply;

            case AlphaEStep.GasSupply:
                return AlphaEStep.MFC;

            case AlphaEStep.MFC:
                return AlphaEStep.Cooler;

            case AlphaEStep.Cooler:
                return AlphaEStep.HighVoltage;

            case AlphaEStep.HighVoltage:
                return AlphaEStep.Microwave;

            case AlphaEStep.Microwave:
                return AlphaEStep.Beam;

            case AlphaEStep.Beam:
                return AlphaEStep.Fusion;

            case AlphaEStep.Fusion:
                return AlphaEStep.Completed;

            default:
                return AlphaEStep.Completed;
        }
    }

    public string GetStepDisplayName(AlphaEStep step)
    {
        switch (step)
        {
            case AlphaEStep.Power:
                return "Power On";

            case AlphaEStep.RoughPump:
                return "Rough Pump";

            case AlphaEStep.TurboPump:
                return "Turbo Pump";

            case AlphaEStep.GasSupply:
                return "Gas Supply";

            case AlphaEStep.MFC:
                return "MFC";

            case AlphaEStep.Cooler:
                return "Cooler";

            case AlphaEStep.HighVoltage:
                return "High Voltage";

            case AlphaEStep.Microwave:
                return "Microwave";

            case AlphaEStep.Beam:
                return "Beam On";

            case AlphaEStep.Fusion:
                return "核融合反應";

            case AlphaEStep.Completed:
                return "流程完成";

            default:
                return step.ToString();
        }
    }

    [ContextMenu("Reset Flow")]
    private void TestResetFlow()
    {
        ResetFlow();
    }

    [ContextMenu("Show Current Step")]
    private void ShowCurrentStep()
    {
        Debug.Log(
            $"目前步驟：{GetStepDisplayName(currentStep)}"
        );
    }
}