/// <summary>
/// NPC 상호작용, 대화 흐름, 퀘스트 액션 처리를 전담하는 컴포넌트.
/// PlayerController에서 분리되어 SRP를 준수하며, 같은 GameObject에 부착한다.
/// </summary>
using UnityEngine;
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    public class PlayerInteractionHandler : MonoBehaviour
    {
        private PlayerController  _owner;
        private NPCController     _currentNPC;
        private QuestContext      _pendingContext;
        private DialoguePanel     _activeDialoguePanel;
        private DialoguePresenter _dialoguePresenter;

        // ── NPC 등록 (NPCRangeTrigger → PlayerController 위임 경유) ────────

        public void SetCurrentNPC(NPCController npc) => _currentNPC = npc;
        public void ClearCurrentNPC()                => _currentNPC = null;

        // ── Unity 생명주기 ────────────────────────────────────────────────

        private void Awake()
        {
            _owner = GetComponent<PlayerController>();
        }

        private void Start()
        {
            DialogueSystem.Instance.OnDialogueEnded += OnDialogueEnded;
        }

        private void OnDestroy()
        {
            if (DialogueSystem.Instance != null)
                DialogueSystem.Instance.OnDialogueEnded -= OnDialogueEnded;
        }

        private void Update()
        {
            HandleInteractInput();
        }

        // ── 인터랙션 입력 ─────────────────────────────────────────────────

        private void HandleInteractInput()
        {
            if (_owner.StateMachine.CurrentState is PlayerInteractState) return;
            if (!Input.GetKeyDown(KeyCode.F)) return;
            if (_currentNPC == null) return;

            _pendingContext = _currentNPC.GetInteractionContext();
            if (_pendingContext.dialogue == null) return;

            _activeDialoguePanel = UIManager.Instance.OpenPanel<DialoguePanel>(UIPanelType.Dialogue, PanelOpenFlag.KeepPrevious);
            _dialoguePresenter   = new DialoguePresenter(_activeDialoguePanel, DialogueSystem.Instance);
            _activeDialoguePanel.SetPresenter(_dialoguePresenter);
            _owner.StateMachine.ChangeState(new PlayerInteractState(_owner.StateMachine));
            DialogueSystem.Instance.StartDialogue(_pendingContext.dialogue, _currentNPC.Data.portrait, _pendingContext.dialogueState);
        }

        // ── 대화 종료 처리 ────────────────────────────────────────────────

        private void OnDialogueEnded()
        {
            if (_pendingContext.action == QuestAction.AcceptQuest && _activeDialoguePanel != null)
            {
                var quest = _pendingContext.quest;
                _pendingContext = default;

                _activeDialoguePanel.ShowQuestChoice(
                    description: quest.description,
                    onAccept: () =>
                    {
                        QuestManager.Instance.AcceptQuest(quest);
                        _activeDialoguePanel.HideQuestChoice();
                        if (quest.dialogue != null)
                            DialogueSystem.Instance.StartDialogue(quest.dialogue, _currentNPC?.Data.portrait, QuestProgressState.Available);
                        else
                            FinishDialogue();
                    },
                    onDecline: () => FinishDialogue()
                );
            }
            else
            {
                if (_pendingContext.action == QuestAction.TalkToNPC)
                    GameEventPublisher.NpcTalked(_currentNPC.Data.npcId);

                _pendingContext = default;
                FinishDialogue();
            }
        }

        private void FinishDialogue()
        {
            _dialoguePresenter?.Dispose();
            _dialoguePresenter   = null;
            _activeDialoguePanel = null;
            UIManager.Instance.ClosePanel(UIPanelType.Dialogue);
            if (_owner.StateMachine.CurrentState is PlayerInteractState interactState)
                interactState.EndInteract();
        }
    }
}
