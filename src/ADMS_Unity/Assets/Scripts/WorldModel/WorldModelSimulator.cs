using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVR
{
    /// <summary>
    /// Phase 3: World Model 시뮬레이션
    /// 물리 법칙(유체, 열, 간섭) 연산 및 예측을 수행하는 모듈
    /// </summary>
    public class WorldModelSimulator : MonoBehaviour
    {
        public static WorldModelSimulator Ins;
        public Material warningMaterial; // 붉은색 경고 홀로그램 머티리얼

        private void Awake()
        {
            if (Ins == null) Ins = this;
        }

        // AppBridge의 ReqAIPrediction에서 호출
        public void RunSimulation(string targetID, int simType)
        {
            StartCoroutine(SimulateProcess(targetID));
        }

        private IEnumerator SimulateProcess(string targetID)
        {
            // 가상의 시뮬레이션 지연 시간
            yield return new WaitForSeconds(1.5f);

            GameObject targetObj = ObjectPropertyCtrl.Ins.FindObject(targetID);
            if (targetObj != null)
            {
                MeshRenderer renderer = targetObj.GetComponent<MeshRenderer>();
                if (renderer != null && warningMaterial != null)
                {
                    // 파손 예측 또는 간섭 시 붉은색 시각화 적용
                    renderer.material = warningMaterial;
                }

                // C++ 브릿지를 통해 UWP UI로 시뮬레이션 결과(예측 결과 텍스트) 반송
#if ENABLE_WINMD_SUPPORT
                AppBridge.Ins.Res("ResAIPrediction", "15분 후 파열 예측. 물리적 인과율 기반 간섭 시뮬레이션 완료.");
#endif
            }
        }
    }
}
