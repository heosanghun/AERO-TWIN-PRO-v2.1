using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMS
{
    /// <summary>
    /// AI가 자율적으로 제안한 복합 변경 사항들을 한 번의 트랜잭션으로 묶어 처리하는 커맨드 클래스.
    /// 기존 LoopMacroCommand를 상속/확장하여 "설계 역제안 수락/거절(Undo/Redo)"을 지원합니다.
    /// </summary>
    class AIGeneratedMacroCommand : IVRCommand
    {
        private IVRCommand[] Commands;
        public string SuggestionTitle { get; private set; }
        public string SuggestionDetails { get; private set; }

        public AIGeneratedMacroCommand(IVRCommand[] commands, string title, string details)
        {
            this.Commands = commands;
            this.SuggestionTitle = title;
            this.SuggestionDetails = details;
        }

        public void execute()
        {
            // 여러 개의 명령(배관 이동, 해치 확장 등)을 한 번에 실행
            foreach (IVRCommand command in Commands)
                command.execute();
        }

        public void undo()
        {
            // 사용자가 AI 제안을 거절할 경우 한 번에 롤백 (순서를 거꾸로 실행하는 것이 안전)
            for (int i = Commands.Length - 1; i >= 0; i--)
            {
                Commands[i].undo();
            }
        }
    }
}
