using Nez;

namespace Spirefall
{
    /// <summary>
    /// A target dummy. Arrows detect it via GetComponent{Target} on whatever
    /// their Mover hit; the arena decides what a hit means (score + respawn).
    /// </summary>
    public class Target : Component
    {
    }
}
