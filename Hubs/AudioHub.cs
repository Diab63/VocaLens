using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace VocaLens.Hubs
{
    public class AudioHub : Hub
    {
        public async Task SendTranscription(string textEnglish, string textArabic)
        {
            await Clients.All.SendAsync("ReceiveTranscription", textEnglish, textArabic);
        }
    }
}
