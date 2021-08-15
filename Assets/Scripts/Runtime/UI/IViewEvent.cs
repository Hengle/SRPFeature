namespace Game.Runtime
{
    public interface IViewEvent
    {
        public void OnBeforeLoad();
        public void OnInstantiate();
        public void OnBeforeShow();
        public void OnAfterShow();
        public void OnBeforeClose();
        public void OnAfterClose();
        public void OnUpdate1();
        public void OnUpdate2();
        public void OnUpdate3();
        public void OnDestory();
    }
}
