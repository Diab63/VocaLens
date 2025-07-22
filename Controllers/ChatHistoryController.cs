using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaLens.Models;
using VocaLens.Service;
using VocaLens.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VocaLens.Controllers
{
    [Route("api/chat-history")]
    [ApiController]
    [Authorize] 
    public class ChatHistoryController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatHistoryController(ChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// POST: api/chat-history/filter
        /// Sets the user's filter choice and returns filtered chat history.
        /// </summary>
        [HttpPost("filter")]
        public async Task<ActionResult<List<ChatHistoryDto>>> GetFilteredChatHistory([FromBody] LanguageFilterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.FilterType))
                return BadRequest("FilterType is required");

            var filter = request.FilterType.ToLower();
            if (filter != "arabic" && filter != "english" && filter != "translation")
                return BadRequest("Invalid choice. Allowed values: arabic, english, translation");

            _chatService.SetCurrentChoice(filter);
            var chatHistory = await _chatService.GetChatHistoryAsync(filter);
            return Ok(chatHistory);
        }

        /// <summary>
        /// GET: api/chat-history/choice
        /// Returns the current user's selected choice.
        /// </summary>
        [HttpGet("choice")]
        public ActionResult GetCurrentChoice()
        {
            var current = _chatService.GetCurrentChoice();
            return Ok(new { choice = current });
        }
    }
}