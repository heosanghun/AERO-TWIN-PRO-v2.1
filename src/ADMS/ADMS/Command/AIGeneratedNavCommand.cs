using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IVR.UWP.Unity3D;
using IVR.UWP.Common.TreeView;

namespace ADMS
{
    /// <summary>
    /// AI가 자율적으로 생성한 네비게이션(동선/위치) 변경 명령을 캡슐화한 커맨드
    /// Phase 2: Agentic AI Command Pattern 확장
    /// </summary>
    class AIGeneratedNavCommand : IVRCommand
    {
        private Unity3DViewModel m_player;
        private string m_objID;
        
        // 이동할 대상 좌표 (예: AI가 추천한 새로운 파이프 위치)
        private float[] m_targetPosition;
        // 기존 위치 (Undo를 위함)
        private float[] m_originalPosition;

        public AIGeneratedNavCommand(Unity3DViewModel player, string objID, float[] originPos, float[] targetPos)
        {
            m_player = player;
            m_objID = objID;
            m_originalPosition = originPos;
            m_targetPosition = targetPos;
        }

        public void execute()
        {
            // Unity로 AI 추천 좌표 적용을 명령
            if(m_targetPosition != null && m_targetPosition.Length >= 3)
            {
                object value = m_targetPosition;
                // Unity의 AppBridge를 통해 위치 변경 이벤트 발송 (여기서는 SetTransformFromUI 재활용 가능)
                m_player.SendToUnity(SendMessage.Req, "SetMove", new string[] { m_objID, m_targetPosition[0].ToString(), m_targetPosition[1].ToString(), m_targetPosition[2].ToString() });
            }
        }

        public void undo()
        {
            // 사용자가 AI 추천을 거부하거나 원래 상태로 되돌림
            if(m_originalPosition != null && m_originalPosition.Length >= 3)
            {
                object value = m_originalPosition;
                m_player.SendToUnity(SendMessage.Req, "SetMove", new string[] { m_objID, m_originalPosition[0].ToString(), m_originalPosition[1].ToString(), m_originalPosition[2].ToString() });
            }
        }
    }
}
