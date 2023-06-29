using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Constants;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;
using TenantManagement.Models.Response;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly int TransientShortAuthCodeLength = 9;
        private readonly int TransientLongAuthCodeLength = 12;
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly IRequestContext _reqContext;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, IUserRepository userrepo, IEmailService emailService, IRequestContext reqcontext, ILogger<AuthService> logger)
        {
            _config = configuration;
            _userRepo = userrepo;
            _emailService = emailService;
            _reqContext = reqcontext;
            _logger = logger;
        }

        public async Task<LoginResponse> Authenticate(string username, string userpass)
        {
            var user = await ValiateCredentials(username, userpass);
            if (user == null)
            {
                return null;
            }

            return await IssueAuthToken(user);
        }

        public async Task<LoginResponse> GetAccountAuthToken(int? accountId = null)
        {
            User user = await _userRepo.GetByIdAsync(_reqContext.UserId.Value, Constants.IncludeAll);
            if (user == null)
            {
                return null;
            }

            if (accountId == null)
            {
                if (user.Roles.FirstOrDefault(x => !x.Name.ToString().StartsWith(nameof(Account))) == null)
                {
                    throw new BaseException(HttpStatusCode.Forbidden, "App Role Not Found", null, _logger);
                }
            }
            else if (user.Accounts.FirstOrDefault(x => x.AccountId == accountId) == null)
            {
                throw new BaseException(HttpStatusCode.NotFound, "Invalid AccountId", null, _logger);
            }

            return await IssueAuthToken(user, accountId);
        }

        public async Task IssueTransientToken(EmailTemplates template, string username = null, int userid = 0)
        {
            User user = null;
            if (!string.IsNullOrEmpty(username))
            {
                user = await _userRepo.GetByNameAsync(username);
            }
            else if (userid > 0)
            {
                user = await _userRepo.GetByIdAsync(userid);
            }

            if (user == null)
            {
                throw new BaseException(HttpStatusCode.NotFound, "Invalid User", null, _logger);
            }

            await IssueNotification(user, template);
        }

        public async Task<LoginResponse> RefreshAuthToken(string username, string refreshToken)
        {
            User user = null;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(refreshToken))
            {
                user = await _userRepo.GetByNameAsync(username, Constants.IncludeAll);
            }

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return null;
            }

            return await IssueAuthToken(user);
        }

        public async Task<LoginResponse> TransientAuthToken(string username, string resetToken)
        {
            var hash = CryptoUtils.GenerateHashV2($"{username.ToLower()}:{resetToken}");
            User user = null;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(resetToken))
            {
                user = await _userRepo.GetByNameAsync(username);
            }

            if (user == null || user.TransientAuthToken != hash || !user.TransientAuthExpiry.HasValue || user.TransientAuthExpiry < DateTime.UtcNow)
            {
                if (user != null && !string.IsNullOrEmpty(user.TransientAuthToken) && user.TransientAuthExpiry.HasValue && user.TransientAuthExpiry > DateTime.UtcNow)
                {
                    //reduce expiry by half for any access
                    var expiryHalf = (user.TransientAuthExpiry.Value - DateTime.UtcNow).TotalSeconds / 2;
                    user.TransientAuthExpiry = user.TransientAuthExpiry.Value.AddSeconds(-expiryHalf);
                    await _userRepo.Update(user);
                }
                return null;
            }
            else
            {
                user.Verified = true;
            }

            var scopes = new List<string>() { TransientScopes.TransientAuthentication.ToString() };
            if (!string.IsNullOrEmpty(user.TransientContext))
            {
                scopes.Add(user.TransientContext);
            }
            return await IssueAuthToken(user, null, scopes);
        }

        public async Task<LoginResponse> GetAuthToken(int? userid = null, string username = null)
        {
            User user = null;
            if (userid.HasValue)
            {
                user = await _userRepo.GetByIdAsync(userid.Value, Constants.IncludeAll);
            }

            if (user == null && !string.IsNullOrEmpty(username))
            {
                user = await _userRepo.GetByNameAsync(username, Constants.IncludeAll);
            }

            if (user == null)
            {
                return null;
            }

            return await IssueAuthToken(user);
        }

        protected async Task<LoginResponse> IssueAuthToken(User user, int? account = null, List<string> transientScopes = null)
        {
            if (user.Accounts != null && user.Accounts.Count > 0)
            {
                if (account != null)
                {
                    //ensure user has access to account
                    if (user.Accounts.FirstOrDefault(x => x.AccountId == account.Value) == null)
                    {
                        account = null;
                    }
                }

                if (account == null && user.Roles?.FirstOrDefault(x => !x.Name.ToString().StartsWith(nameof(Account))) == null)
                {
                    account = user.Accounts.FirstOrDefault(x => x.UserPrimary == true)?.AccountId;
                    if (account == null)
                    {
                        account = user.Accounts.First().AccountId;
                    }
                }
            }
            else
            {
                account = null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["AppSecret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenClaimsIdentity(user, account, transientScopes),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature),
                Issuer = _config["JwtConfig:Issuer"],
                Audience = _config["JwtConfig:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenStr = tokenHandler.WriteToken(token);

            LoginResponse response = new LoginResponse
            {
                Token = tokenStr,
                TenantId = user.TenantId.ToString(),
                Roles = user.Roles.Select(r => r.Name.ToString()).ToList(),
                RefreshToken = CryptoUtils.GenerateHashV2($"{tokenDescriptor.Expires.ToString()}:{token}"),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(AppGlobals.RefreshTokenExpiryDays)
            };

            if ((transientScopes == null || !transientScopes.Contains(TransientScopes.TransientAuthentication.ToString())) && user.Verified.HasValue && user.Verified == true)
            {
                user.TransientAuthToken = null;
                user.TransientAuthExpiry = null;
                user.TransientContext = null;
            }

            user.RefreshToken = response.RefreshToken;
            user.RefreshTokenExpiry = response.RefreshTokenExpiry;
            user.LatestLogin = DateTime.UtcNow;
            await _userRepo.Update(user);
            return response;
        }

        public async Task IssueNotification(User user, EmailTemplates template)
        {
            string resetToken = null;
            if (template == EmailTemplates.PasswordReset || template == EmailTemplates.ChangeEmailPasswordReset)
            {
                resetToken = CryptoUtils.GetRandomString(TransientShortAuthCodeLength);
                user.TransientAuthExpiry = DateTime.UtcNow.AddMinutes(AppGlobals.TransientAuthExpiryMinutes);
            }
            else
            {
                resetToken = CryptoUtils.GetRandomString(TransientLongAuthCodeLength);
                user.TransientAuthExpiry = DateTime.UtcNow.AddDays(AppGlobals.TransientAuthExpiryDays);
            }

            user.TransientAuthToken = CryptoUtils.GenerateHashV2($"{user.Username.ToLower()}:{resetToken}");
            await _userRepo.Update(user);

            User from = null;
            if (_reqContext.UserId.HasValue)
            {
                from = _reqContext.User ?? await _userRepo.GetByIdAsync(_reqContext.UserId.Value);
            }

            var emailspec = _emailService.QueueEmailSpec(template.ToString(), from: from, to: user);
            emailspec.ModelData.Add(nameof(EmailDataKeys.Id), user.Username);
            emailspec.ModelData.Add(nameof(EmailDataKeys.Code), resetToken);
            await _emailService.DispatchEmail();
        }

        protected static ClaimsIdentity GenClaimsIdentity(User user, int? account = null, List<string> transientScopes = null)
        {
            var claims = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, user.Username),
                new Claim(AppGlobals.ClaimTypeSubjectId, user.UserId.ToString()),
                new Claim(AppGlobals.ClaimTypeTenantId, user.TenantId.ToString()),
            });

            if (user.Accounts != null && user.Accounts.Count > 0 && account != null)
            {
                claims.AddClaim(new Claim(AppGlobals.ClaimTypeOrgId, account.Value.ToString()));
                var act = user.Accounts.FirstOrDefault(x => x.AccountId == account.Value);
                if (act != null)
                {
                    foreach (var role in act.Roles)
                    {
                        claims.AddClaim(new Claim(ClaimTypes.Role, role.Name.ToString()));
                    }
                }
            }
            else
            {
                if (user.Roles != null && user.Roles.Count > 0)
                {
                    foreach (var role in user.Roles)
                    {
                        claims.AddClaim(new Claim(ClaimTypes.Role, role.Name.ToString()));
                    }
                }
            }

            if (user.Scopes != null && user.Scopes.Count > 0)
            {
                foreach (var scope in user.Scopes)
                {
                    claims.AddClaim(new Claim(AppGlobals.ClaimTypeScopeId, scope.Name));
                }
            }

            if (transientScopes != null && transientScopes.Count > 0)
            {
                foreach (var scope in transientScopes)
                {
                    claims.AddClaim(new Claim(AppGlobals.ClaimTypeScopeId, scope));
                }
            }

            return claims;
        }

        protected async Task<User> ValiateCredentials(string username, string userpass)
        {
            User user = await _userRepo.GetByNameAsync(username, Constants.IncludeAll);

            if (user == null)
            {
                return null;
            }

            if (user.Userhash == CryptoUtils.HashPassword(username.ToLower(), userpass, user.Salt) ||
                user.Userhash == CryptoUtils.HashPassword(username.ToLower(), userpass, user.Salt, true) ||
                user.Userhash == CryptoUtils.HashPassword(username, userpass, user.Salt) ||
                user.Userhash == CryptoUtils.HashPassword(username, userpass, user.Salt, true))
            {
                return user;
            }

            return null;
        }
    }
}