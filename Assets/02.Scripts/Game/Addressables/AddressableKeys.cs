namespace MMORPG.Game
{
    /// <summary>
    /// Addressables 로드에 사용되는 키 문자열을 생성하는 정적 유틸리티 클래스.
    /// CLAUDE.md의 키 네이밍 규칙을 단일 위치에서 관리해 오타를 방지한다.
    /// </summary>
    public static class AddressableKeys
    {
        /// <summary>게임 시작 시 사전 로드되는 PlayerSO 키.</summary>
        public const string PRELOAD_PLAYER = "preload/player";

        /// <summary>QuestSO Addressable 키를 반환한다. → "quest/{questId}"</summary>
        /// <param name="questId">QuestSO.questId 값</param>
        public static string GetQuestKey(string questId) => $"quest/{questId}";

        /// <summary>퀘스트 런타임 상태 JSON 키를 반환한다. → "quest/{questId}_state"</summary>
        /// <param name="questId">QuestSO.questId 값</param>
        public static string GetQuestStateKey(string questId) => $"quest/{questId}_state";

        /// <summary>NPCSO Addressable 키를 반환한다. → "npc/{npcId}"</summary>
        /// <param name="npcId">NPCSO.npcId 값</param>
        public static string GetNpcKey(string npcId) => $"npc/{npcId}";

        /// <summary>DialogueSO Addressable 키를 반환한다. → "dialogue/{dialogueId}"</summary>
        /// <param name="dialogueId">DialogueSO.dialogueId 값</param>
        public static string GetDialogueKey(string dialogueId) => $"dialogue/{dialogueId}";

        /// <summary>SkillSO Addressable 키를 반환한다. → "skill/{skillId}"</summary>
        /// <param name="skillId">SkillSO.skillId 값</param>
        public static string GetSkillKey(string skillId) => $"skill/{skillId}";

        /// <summary>스킬 이펙트 Prefab Addressable 키를 반환한다. → "fx/{effectId}"</summary>
        /// <param name="effectId">이펙트 고유 ID</param>
        public static string GetFxKey(string effectId) => $"fx/{effectId}";
    }
}
