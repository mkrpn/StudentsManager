using System.ComponentModel.DataAnnotations;

namespace StudentsManager.Models
{
    public abstract class Oauth2Config
    {
        public abstract string ConfigName { get; }

        [Required]
        public string CallbackUrl { get; set; }

        [Required]
        public string AuthUrl { get; set; }

        [Required]
        public string AccessTokenUrl { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientSecret { get; set; }

        public string Scope { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string Token { get; set; }
    }
}
