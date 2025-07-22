using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VocaLens.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TextColor : ControllerBase
    {
        // Static storage for color
        private static string _textColorHex = "#FFFFFF"; // Default: white

        
        [HttpPost("color")]
        public IActionResult SetTextColor([FromBody] TextColorDto colorDto)
        {
            if (string.IsNullOrWhiteSpace(colorDto.HexColor))
                return BadRequest("Invalid color");

            _textColorHex = colorDto.HexColor;
            return Ok("Text color updated.");
        }

        
        [HttpGet("color")]
        public IActionResult GetTextColor()
        {
            return Ok(_textColorHex);
        }
    }

    public class TextColorDto
    {
        public string HexColor { get; set; }
    }
}
