namespace Vabulu.Models.Auth {
    public class ResetPassword {
        public string Email { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
    }
}