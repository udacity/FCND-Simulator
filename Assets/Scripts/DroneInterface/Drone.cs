using DroneInterface;
namespace DroneInterface
{
    // Minimal drone interface (commands and state).
	public interface IDrone : IDroneCommands, IDroneState
    {
    }
}