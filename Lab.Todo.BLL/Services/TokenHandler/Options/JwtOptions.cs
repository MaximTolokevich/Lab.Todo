using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.BLL.Services.TokenHandler.Options
{
    public class JwtOptions
    {
        [Required, Range(typeof(TimeSpan), "00:00:00", "10675199.02:48:05.4775807", ErrorMessage = "Token expiration time have to be greater than zero.")]
        public TimeSpan? TokenExpirationTime { get; set; }

        [Required, MinLength(16)]
        public string SecretValue { get; set; }

        [Required, MinLength(1)]
        public IEnumerable<string> EncryptionAlgorithms { get; set; }
    }
}