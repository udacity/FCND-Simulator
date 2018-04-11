using DroneInterface;
namespace DroneInterface
{
    /// <summary>
    /// DroneSystem interface, includes methods for:
    ///  vehicle
    ///  controller
    ///  sensors
    /// </summary>
	public interface IDroneSystem : IDroneVehicle, IDroneController, IDroneSensors {}
}