using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVR
{
    /// <summary>
    /// Phase 3: Physical AI 인체공학 한계 시뮬레이션
    /// Digital Human 의 손끝(End-Effector)에서 부품까지 거리 계산 및 충돌 감지
    /// </summary>
    public class DigitalHumanIK : MonoBehaviour
    {
        public static DigitalHumanIK Ins;
        
        [Header("디지털 휴먼 IK 설정")]
        public Transform humanRightHand; // 우측 손끝 트랜스폼
        public float maxReachability = 0.8f; // 최대 도달 가능 거리(m) - 80cm
        public float minTorqueSpace = 0.15f; // 렌치 회전 반경 확보 필요 공간(m) - 15cm

        private void Awake()
        {
            if (Ins == null) Ins = this;
        }

        public void CheckErgonomicLimits(string targetObjID)
        {
            if (humanRightHand == null) return;

            GameObject targetObj = ObjectPropertyCtrl.Ins.FindObject(targetObjID);
            if(targetObj == null) return;

            // 1. 도달 거리(Reachability) 검증
            float distance = Vector3.Distance(humanRightHand.position, targetObj.transform.position);

            if (distance > maxReachability)
            {
                float deficit = distance - maxReachability;
                string warningMsg = $"인체공학 제약 발생: 팔 도달 거리 {deficit * 100:F1}cm 부족.";

                // UWP (HUD Panel 4) 로 알림 발송
#if ENABLE_WINMD_SUPPORT
                AppBridge.Ins.Res("OnPhysicalLimitReached", new string[] { targetObjID, warningMsg });
#endif
                return;
            }

            // 2. 렌치 회전 반경(Torque space) 검증 (단순 BoxCast 또는 SphereCast로 근처 충돌체 검사)
            Collider[] colliders = Physics.OverlapSphere(targetObj.transform.position, minTorqueSpace);
            bool hasInterference = false;
            foreach (Collider col in colliders)
            {
                // 자기 자신을 제외한 다른 부품과 렌치가 닿는지 검사
                if (col.gameObject != targetObj && col.gameObject.layer != LayerMask.NameToLayer("Human"))
                {
                    hasInterference = true;
                    break;
                }
            }

            if (hasInterference)
            {
                string warningMsg = "렌치 회전 반경 미확보. 간섭 발생 예측.";
#if ENABLE_WINMD_SUPPORT
                AppBridge.Ins.Res("OnPhysicalLimitReached", new string[] { targetObjID, warningMsg });
#endif
            }
            else
            {
#if ENABLE_WINMD_SUPPORT
                AppBridge.Ins.Res("OnPhysicalLimitReached", new string[] { targetObjID, "목표 부품 정비 공간 확보 완료. 정상." });
#endif
            }
        }
    }
}
