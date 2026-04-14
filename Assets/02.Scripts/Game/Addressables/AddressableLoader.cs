using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MMORPG.Data;

namespace MMORPG.Game
{
    /// <summary>
    /// Addressables 비동기 로드를 래핑하는 정적 유틸리티 클래스.
    /// 모든 로드는 UniTask 기반 비동기로 처리되며, 로드 실패 시 Debug.LogError를 출력한다.
    /// 반환된 에셋은 호출측에서 사용이 끝난 뒤 Addressables.Release로 해제해야 한다.
    /// </summary>
    public static class AddressableLoader
    {
        /// <summary>
        /// 지정한 키로 에셋을 비동기 로드한다.
        /// 로드 실패 시 LogError를 출력하고 default를 반환한다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <param name="key">Addressable 키 문자열</param>
        public static async UniTask<T> LoadAsync<T>(string key)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            try
            {
                return await handle.ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressableLoader] 로드 실패 — key: \"{key}\" / {e.Message}");
                return default;
            }
            finally
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
        }

        /// <summary>questId에 해당하는 QuestSO를 비동기 로드한다.</summary>
        /// <param name="questId">QuestSO.questId 값</param>
        public static UniTask<QuestSO> LoadQuestSOAsync(string questId)
            => LoadAsync<QuestSO>(AddressableKeys.GetQuestKey(questId));

        /// <summary>npcId에 해당하는 NPCSO를 비동기 로드한다.</summary>
        /// <param name="npcId">NPCSO.npcId 값</param>
        public static UniTask<NPCSO> LoadNPCSOAsync(string npcId)
            => LoadAsync<NPCSO>(AddressableKeys.GetNpcKey(npcId));

        /// <summary>dialogueId에 해당하는 DialogueSO를 비동기 로드한다.</summary>
        /// <param name="dialogueId">DialogueSO.dialogueId 값</param>
        public static UniTask<DialogueSO> LoadDialogueSOAsync(string dialogueId)
            => LoadAsync<DialogueSO>(AddressableKeys.GetDialogueKey(dialogueId));

        /// <summary>
        /// 게임 시작 시 PlayerSO를 사전 로드한다.
        /// 키는 AddressableKeys.PRELOAD_PLAYER 상수를 사용한다.
        /// </summary>
        public static UniTask<PlayerSO> PreloadPlayerSOAsync()
            => LoadAsync<PlayerSO>(AddressableKeys.PRELOAD_PLAYER);
    }
}
