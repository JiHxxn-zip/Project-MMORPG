/// <summary>
/// 대화 UI 패널이 구현해야 하는 View 인터페이스.
/// DialoguePresenter가 이 인터페이스만을 통해 View를 제어한다.
/// </summary>
using UnityEngine;

namespace MMORPG.Core
{
    public interface IDialogueView
    {
        /// <summary>화자 이름, 대사, 초상화를 표시하고 타이핑 연출을 시작한다.</summary>
        void ShowDialogue(string speakerName, string text, Sprite portrait);

        /// <summary>대화창을 숨긴다.</summary>
        void HideDialogue();

        /// <summary>타이핑 연출이 진행 중인지 여부.</summary>
        bool IsTyping { get; }

        /// <summary>타이핑 연출을 즉시 완료하고 전체 텍스트를 표시한다.</summary>
        void SkipTyping();
    }
}
