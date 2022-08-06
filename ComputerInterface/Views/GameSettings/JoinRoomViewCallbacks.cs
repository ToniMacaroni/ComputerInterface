using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    class JoinRoomViewCallbacks : MonoBehaviour
    {
        public JoinRoomView view;

        // Yeah I'm pretty lazy
        void Update()
        {
            view.Redraw();
        }
    }
}
