namespace Ptl.Core.Interfaces
{
    public interface IPtlSender
    {
        Task SendAsync(object command);
    }
}
