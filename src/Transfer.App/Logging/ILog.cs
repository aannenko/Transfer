namespace Transfer.App.Logging;

public interface ILog
{
    void Info(string message);

    void Warn(string message);

    void Error(string message);
}
