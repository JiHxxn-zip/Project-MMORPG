namespace MMORPG.Game
{
    public class DamageTakenEvent
    {
        public Character Defender    { get; set; }
        public float     FinalDamage { get; set; }
        public bool      IsCritical  { get; set; }
    }
}
