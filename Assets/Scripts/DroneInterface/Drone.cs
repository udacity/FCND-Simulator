using DroneInterface;
namespace DroneInterface
{
    /// <summary>
    /// Drone interface, includes methods for:
    ///  commands (alter state)
    ///  state
    /// </summary>
	//public interface IDrone : IDroneCommands, IDroneState {}
    public interface IDrone : IDroneVehicle, IDroneController, IDroneSensors { }
}