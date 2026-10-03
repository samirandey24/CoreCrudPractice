namespace CoreCrudWithJwt.Services
{
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(
            int userId,
            string useremail,
            string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = jwtSettings["Key"]
                ?? throw new InvalidOperationException("JWT Key is missing.");

            var issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is missing.");

            var audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException("JWT Audience is missing.");

            var expiryMinutes =
                int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new Claim(
                JwtRegisteredClaimNames.UniqueName,
                useremail),

            new Claim(
                ClaimTypes.Name,
                useremail),

            new Claim(
                ClaimTypes.Role,
                role),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }

}
