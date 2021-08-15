using UnityEngine;

namespace Game.Runtime
{
    public class UIRoot : MonoBehaviour
    {
        [SerializeField]
        private Camera _uiCamera;
        public Camera UICamera => _uiCamera;

        [SerializeField]
        private Canvas _resident;
        public Canvas Resident => _resident;

        [SerializeField]
        private Canvas _normal;
        public Canvas Normal => _normal;

        [SerializeField]  
        private Canvas _mission;
        public Canvas Mission => _mission;

        [SerializeField]
        private Canvas _overlay;
        public Canvas Overlay => _overlay;

        private void Update()
        {
            UINavigation.Update1();
            if ((Time.frameCount & 1) == 0)
            {
                UINavigation.Update2();
            }

            if (Time.frameCount % 3 == 0)
            {
                UINavigation.Update3();
            }
        }
    }
}