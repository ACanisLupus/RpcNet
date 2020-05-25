namespace RpcNet
{
    public interface IXdrWritable
    {
        void WriteTo(IXdrWriter writer);
    }
}
