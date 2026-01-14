using Serilog.Core;
using Serilog.Events;
using System.Net.Http;
using System.Threading.Tasks;

namespace TODO_List.Alerts
{
    public class TelegramSink : ILogEventSink
    {
        private readonly string _workerUrl;
        private readonly string _apiKey;
        private readonly HttpClient _client = new HttpClient();
        public TelegramSink(string workerUrl, string apiKey)
        {
            _workerUrl = workerUrl;
            _apiKey = apiKey;
        }
        public void Emit(LogEvent logEvent)
        {
            if(logEvent.Level >= LogEventLevel.Error)
            {
                var message = logEvent.RenderMessage();
                SendTelegramMessageAsync(message).GetAwaiter().GetResult();
            }
        }
        private async Task SendTelegramMessageAsync(string message)
        {
            try
            {
                var url = $"{_workerUrl}?text={Uri.EscapeDataString(message)}&key={_apiKey}";
                var response = await _client.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Telegram Alert Failed: " + result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending Telegram alert: " + ex.Message);
            }
        }
    }
}
