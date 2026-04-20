namespace MMORPG.Core
{
    public enum DamageType
    {
        Physical,   // 물리: 방어력 적용
        Magic,      // 마법: 마법 저항 + 방어력 적용
        True        // 고정: 방어 무시
    }
}
