using System.Net.NetworkInformation;
using Cysharp.Threading.Tasks;

namespace _Project
{
    public class InternetConnection
    {
        private async UniTask<bool> Check()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await UniTask.Run(() => ping.Send("8.8.8.8"));
                    return (reply.Status == IPStatus.Success);
                }
            }
            catch (PingException)
            {
                return false;
            }
        }
    }
}