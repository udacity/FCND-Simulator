using DroneInterface;
using DroneControllers;

namespace Drones
{
    class QuadDrone : IDrone
    {
        QuadController quadCtrl;
        SimpleQuadController simpleQuadCtrl;
    }
}