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
        private readonly IDialogueView  _view;
        private readonly DialogueSystem _model;

        public DialoguePresenter(IDialogueView view, DialogueSystem model)
        {
            _view  = view;
            _model = model;

            _model.OnNodeChanged   += HandleNodeChanged;
            _model.OnDialogueEnded += HandleDialogueEnded;
        }

        public void Dispose()
        {
            _model.OnNodeChanged   -= HandleNodeChanged;
            _model.OnDialogueEnded -= HandleDialogueEnded;
        }

        /// <summary>
        /// View의 Space/클릭 입력을 받아 처리한다.
        /// 타이핑 중이면 View에서 스킵하고, 완료 상태면 Model에 다음 노드를 요청한다.
        /// </summary>
        public void OnNextRequested()
        {
            if (_view.IsTyping)
                _view.SkipTyping();
            else
                _model.Next();
        }

        private void HandleNodeChanged(DialogueNode node)
        {
            _view.ShowDialogue(node.speakerName, node.text, _model.CurrentPortrait);
        }

        private void HandleDialogueEnded()
        {
            _view.HideDialogue();
        }
    }
}
