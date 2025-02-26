﻿using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAML_Test_Application_AzureAD_B2C.Models;

namespace SAML_Test_Application_AzureAD_B2C.Controllers
{
    public class SamlController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        private readonly Saml2Configuration config;
        private readonly SamlSettings _samlSettings;


        public SamlController(Saml2Configuration config, IOptions<SamlSettings> samlSettings)
        {
            this.config = config;
            _samlSettings = samlSettings.Value;
        }


        [Route("saml/login")]
        public IActionResult Login()
        {

            //var saml2AuthnRequest = new Saml2AuthnRequest(config)
            //{
            //    AssertionConsumerServiceUrl = new Uri(builder.Configuration["Saml:AssertionConsumerServiceUrl"]),
            //    ForceAuthn = true,
            //    Destination = new Uri(builder.Configuration["AzureAdB2C:MetadataUri"].Replace("/metadata", "/sso/login"))
            //};

            var binding = new Saml2RedirectBinding();
            var saml2AuthnRequest = new Saml2AuthnRequest(config)
            {
                //AssertionConsumerServiceUrl = new Uri("https://localhost:5001/saml/acs"),
                // https://localhost:44306/Auth/Login?ReturnUrl=%2F
                // https://samltestapplicationazureadb2-production.up.railway.app/Auth/Login?ReturnUrl=%2F
                AssertionConsumerServiceUrl = new Uri("https://samltestapplicationazureadb2-production.up.railway.app/Auth/Login?ReturnUrl=%2F"),
                ForceAuthn = true,
               // Destination = new Uri(builder.Configuration["AzureAdB2C:MetadataUri"].Replace("/metadata", "/sso/login"))
            };
            return binding.Bind(saml2AuthnRequest).ToActionResult();

        }


        [Route("saml/acs")]
        public async Task<IActionResult> Acs()
        {
            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse(config);

            binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                throw new Exception("SAML login failed.");
            }
            binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            var relayState = binding.RelayState;
            await saml2AuthnResponse.CreateSession(HttpContext);

            return Redirect(relayState ?? "/");
        }



        [Authorize]
        [Route("saml/logout")]
        public IActionResult Logout()
        {
            return SignOut("Cookies", "Saml2");
        }



    }
}
