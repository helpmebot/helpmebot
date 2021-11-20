using Microsoft.AspNetCore.Mvc;

namespace Helpmebot.WebUI.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;

    public class ResponsesController : ControllerBase
    {
        public ResponsesController(IApiService apiService) : base(apiService)
        {
        }
        
        [HttpGet("/responses")]
        public IActionResult Index()
        {
            // var responses = new List<Response>();
            //
            // var messageKeys = this.ApiService.GetMessageKeys();
            // foreach (var key in messageKeys)
            // {
            //     responses.Add(this.ApiService.GetMessageDefaultResponses(new ResponseKey { MessageKey = key }));
            // }

            var responses = this.ApiService.GetMessageResponses().OrderBy(x => x.Key.MessageKey).ToList();
            
            return View(responses);
        }
    }
}