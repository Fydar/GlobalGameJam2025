public interface IInterpolator<TValue>
{
    public TValue Target { get; set; }

    public TValue Value { get; set; }
    public void Update(float deltaTime);
}
