using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaLens.DTOs;

namespace VocaLens.Controllers
{
    [Authorize]
    [Route("api/font")]
    [ApiController]
    
    public class FontController : ControllerBase
    {
        // Static in-memory storage
        private static int _fontSize = 1; // Default font size

        // GET: api/font
        [HttpGet]
        public ActionResult<int> GetFontSize()
        {
            return Ok("FontSize" + _fontSize);
        }

        // POST: api/font
        [HttpPost]
        public IActionResult UpdateFontSize([FromBody] FontSizeDTO fontSizeDto)
        {
            if (fontSizeDto.FontSize < 1 || fontSizeDto.FontSize > 9)
            {
                return BadRequest("Font size must be between 1 and 9.");
            }

            _fontSize = fontSizeDto.FontSize;
            return Ok(new { Message = "Font size updated successfully", FontSize = _fontSize });
        }
    }
}
