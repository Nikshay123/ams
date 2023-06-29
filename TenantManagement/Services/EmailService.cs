using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Models;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Services
{
    public class EmailService : NotifyBase, IEmailService
    {
        private static string EMAIL_API_ENDPOINT = null;
        private readonly string AZURE_EMAIL_ENDPOINT_CONNECTION_STRING = "EmailApiEndpoint";
        private readonly IConfiguration _config;
        private readonly IRequestContext _reqContext;
        private readonly IRazorEmailRenderer _razorRenderer;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;
        private readonly ILogger<EmailService> _logger;
        private readonly List<EmailModel> _emailSpecs = new();

        public EmailService(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            IRazorEmailRenderer razorrenderer,
            IRequestContext reqcontext,
            IBackgroundJobService backgroundJobService,
            IMapper mapper,
            ILogger<EmailService> logger) : base(clientFactory)
        {
            _config = configuration;
            _reqContext = reqcontext;
            _razorRenderer = razorrenderer;
            _backgroundJobService = backgroundJobService;
            _mapper = mapper;
            _logger = logger;

            if (EMAIL_API_ENDPOINT == null)
            {
                Interlocked.Exchange(ref EMAIL_API_ENDPOINT, configuration.GetConnectionString(AZURE_EMAIL_ENDPOINT_CONNECTION_STRING));
            }
        }

        public List<EmailModel> Emails
        { get { return _emailSpecs; } }

        public EmailModel QueueEmailSpec(string template = null, User from = null, User to = null, Account account = null)
        {
            var emailModel = new EmailModel
            {
                Template = template,
                ModelData = new()
                {
                    { nameof(EmailDataKeys.Domain),  (string)_config.GetValue(typeof(string), AppGlobals.BASE_DOMAIN_CONFIG_NAME)}
                }
            };

            if (from != null)
            {
                emailModel.AddFrom(from);
            }

            if (to != null)
            {
                emailModel.AddTo(to);
            }

            if (account != null)
            {
                emailModel.AddAccount(account);
            }

            _emailSpecs.Add(emailModel);
            return emailModel;
        }

        public async Task DispatchEmail(Guid? tenantId = null)
        {
            if (System.Transactions.Transaction.Current == null)
            {
                await SendBatch(Emails);
                Emails.Clear();
            }
            else
            {
                _backgroundJobService.RegisterJob<EmailService>(x => SendBatch(Emails), null, $"{nameof(EmailService)}:{nameof(DispatchEmail)}");
            }
        }

        public async Task SendBatch(List<EmailModel> emailSpecs)
        {
            if (System.Transactions.Transaction.Current == null)
            {
                foreach (var email in emailSpecs)
                {
                    using var jc = new BackgroundJobContext((Guid)(_reqContext.TenantId));
                    Hangfire.BackgroundJob.Enqueue<IEmailService>(es => es.SendMail(email));
                }
            }
        }

        public async Task<string> SendMail(EmailModel emailSpec)
        {
            emailSpec.MsgBody = await _razorRenderer.Render(emailSpec.ModelData, emailSpec.Template);

            var regex = new Regex(@"<title>(.+)</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var mc = regex.Match(emailSpec.MsgBody);
            if (mc.Success)
            {
                emailSpec.Subject = mc.Groups[1].Value;
            }

            var content = new StringContent(JsonSerializer.Serialize(emailSpec));
            var response = await PostApiCall(content, EMAIL_API_ENDPOINT);

            if (response.IsSuccessStatusCode)
            {
                var resStream = await response.Content.ReadAsStringAsync();
                return resStream;
            }
            else
            {
                return null;
            }
        }
    }
}