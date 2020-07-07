namespace RpcNet
{
    public interface ILogger
    {
        void Trace(string entry);
        void Info(string entry);
        void Error(string entry);
    }
}
