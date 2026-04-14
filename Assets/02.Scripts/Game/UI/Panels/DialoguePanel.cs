/// <summary>
/// NPC 대화를 표시하는 패널. Step 6에서 DialogueSystem과 연동 예정.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class DialoguePanel : GameUIPanel
    {
        // TODO(Step 6): DialogueSystem.OnDialogueStarted / OnDialogueEnded 이벤트 구독
        [SerializeField] private GameObject _dialogueContent;

        public override void OnOpen() { }
        public override void OnClose() { }
    }
}
