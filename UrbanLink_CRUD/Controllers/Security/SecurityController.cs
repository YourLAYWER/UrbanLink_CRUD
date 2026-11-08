using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UrbanLink_CRUD.Controllers.Security
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SecurityController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/Security/public-key
        [HttpGet("public-key")]
        public IActionResult GetPublicKey()
        {
            var publicKey = _config["Cryptography:EcdsaPublicKey"];
            if (string.IsNullOrEmpty(publicKey))
            {
                return StatusCode(500, new { message = "Public key not configured on server." });
            }

            // We wrap it in an object so Android's Retrofit can easily parse the JSON
            return Ok(new { publicKey = publicKey });
        }
    }
}
