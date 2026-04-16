/// <summary>
/// Model(DialogueSystem)과 View(IDialogueView) 사이를 중재하는 Presenter.
/// MonoBehaviour가 아닌 순수 C# 클래스이며, View는 인터페이스로만 참조한다.
/// </summary>
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    public class DialoguePresenter
    {
        private readonly IDialogueView _view;
        private readonly DialogueSystem _model;

        public DialoguePresenter(IDialogueView view, DialogueSystem model)
        {
            _view = view;
            _model = model;

            _model.OnNodeChanged    += HandleNodeChanged;
            _model.OnTypingComplete += HandleTypingComplete;
            _model.OnDialogueEnded  += HandleDialogueEnded;
        }

        public void Dispose()
        {
            _model.OnNodeChanged    -= HandleNodeChanged;
            _model.OnTypingComplete -= HandleTypingComplete;
            _model.OnDialogueEnded  -= HandleDialogueEnded;
        }

        /// <summary>View의 타이핑 애니메이션이 자연 완료되면 View가 호출한다.</summary>
        public void OnViewTypingCompleted()
        {
            _model.NotifyTypingFinished();
        }

        private void HandleNodeChanged(DialogueNode node)
        {
            // CurrentPortrait는 StartDialogue 시 Model에 저장된 NPC 초상화
            _view.ShowDialogue(node.speakerName, node.text, _model.CurrentPortrait);
        }

        private void HandleTypingComplete()
        {
            _view.SetTypingComplete();
        }

        private void HandleDialogueEnded()
        {
            _view.HideDialogue();
        }
    }
}
