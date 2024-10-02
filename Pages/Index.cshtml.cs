using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppRabbitMQ.Services;

namespace WebAppRabbitMQ.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly RabbitMQService _rabbitMQService;

        public IndexModel(ILogger<IndexModel> logger, RabbitMQService rabbitMQService)
        {
            _logger = logger;
            _rabbitMQService = rabbitMQService;
        }

        //public void OnGet()
        //{

        //}

        [BindProperty]
        public string QueueName { get; set; }

        [BindProperty]
        public string Message { get; set; }

        public string ConsumedMessage { get; set; }

        public List<string> Queues { get; set; }

        public string Status => _rabbitMQService.Status();

        public IActionResult OnPostCreateQueue()
        {
            _rabbitMQService.CreateQueue(QueueName);
            return Page();
        }

        public IActionResult OnPostPublishMessage()
        {
            _rabbitMQService.PublishMessage(QueueName, Message);
            return Page();
        }

        public IActionResult OnPostConsumeMessage()
        {
            ConsumedMessage = _rabbitMQService.ConsumeMessage(QueueName);
            return Page();
        }

        public IActionResult OnPostDeleteQueue()
        {
            _rabbitMQService.DeleteQueue(QueueName);
            return Page();
        }

        public async Task<IActionResult> OnPostListQueues()
        {
            Queues = await _rabbitMQService.ListQueuesAsync();
            return Page();
        }
    }
}
