namespace Game.Runtime
{
    public abstract class AViewBase : IViewEvent
    {
        void IViewEvent.OnBeforeLoad()
        {
            OnBeforeLoad();
        }

        void IViewEvent.OnInstantiate()
        {
            OnInstantiate();
        }

        void IViewEvent.OnBeforeShow()
        {
            OnBeforeShow();
        }

        void IViewEvent.OnAfterShow()
        {
            OnAfterShow();
        }

        void IViewEvent.OnBeforeClose()
        {
            OnBeforeClose();
        }
        
        void IViewEvent.OnAfterClose()
        {
            OnAfterClose();
        }

        void IViewEvent.OnUpdate1()
        {
            OnUpdate1();
        }

        void IViewEvent.OnUpdate2()
        {
            OnUpdate2();
        }

        void IViewEvent.OnUpdate3()
        {
            OnUpdate3();
        }

        void IViewEvent.OnDestory()
        {
            OnDestory();
        }

        protected virtual void OnBeforeLoad()
        {
        }

        protected virtual void OnInstantiate()
        {
        }

        protected virtual void OnBeforeShow()
        {
        }

        protected virtual void OnAfterShow()
        {
        }
        
        protected virtual void OnBeforeClose()
        {
        }

        protected virtual void OnAfterClose()
        {
        }

        protected virtual void OnUpdate1()
        {
        }

        protected virtual void OnUpdate2()
        {
        }

        protected virtual void OnUpdate3()
        {
        }

        protected virtual void OnDestory()
        {
        }
    }
}