using System.Threading.Tasks;
using HC.TFPlugin;
using HC.TFPlugin.Attributes;

namespace Terrascape.AcmeProvider
{
    /// <summary>
    /// Can be used to create and manage accounts on an ACME server.
    /// </summary>
    /// <remarks>
    /// Once registered, the same private key that has been used for
    /// registration can be used to request authorizations for certificates.
    /// <p class="note">
    /// This resource used to be called <c>acmelo_registration</c>.
    /// </p>
    /// </remarks>
    [TFResource("acmelo_account")]
    public class AccountResource
    {
        /// <summary>
        /// The private key used to identity the account.
        /// </summary>
        [TFAttribute("account_key_pem",
            Required = true,
            ForceNew = true,
            Sensitive = true)]
        public string AccountKeyPem { get; set; }

        /// <summary>
        /// The contact email address for the account.
        /// </summary>
        [TFAttribute("email_address",
            Required = true,
            ForceNew = true)]
        public string EmailAddress { get; set ;}

        /// <summary>
        /// The original full URL of the account.
        /// </summary>
        [TFAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// The current full URL of the account.
        /// </summary>
        [TFAttribute("registration_url", Computed = true)]
        public string RegistrationUrl { get; set; }

        public Task CreateAsync() => Task.CompletedTask;

        public Task ReadAsync() => Task.CompletedTask;

        public Task UpdateAsync() => Task.CompletedTask;
    }
}