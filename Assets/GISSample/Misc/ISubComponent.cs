namespace GISSample.Misc
{
    /// <summary>
    /// MonoBehaviourではないけれど、UpdateやOnEnable等を実行したいクラスに使います。
    /// <see cref="GISSampleSubComponents"/>から呼び出すことを想定しています。
    /// </summary>
    public interface ISubComponent
    {
        public void Update(float deltaTime);
        public void OnEnable();
        public void OnDisable();

        public void Start();

    }
}