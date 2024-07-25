namespace Common.Infra;

public interface ILogging<T>
{
    void Clear() { }

    void Append(T item) { }

}


