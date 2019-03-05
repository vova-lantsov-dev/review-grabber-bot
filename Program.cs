using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ReviewGrabberBot
{
    public class Program
    {
        internal static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureServices(services =>
                {
                })
                .RunConsoleAsync();
        }
    }
}