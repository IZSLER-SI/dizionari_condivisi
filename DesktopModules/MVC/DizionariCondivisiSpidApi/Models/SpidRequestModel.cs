using Newtonsoft.Json;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi.Models
{
    public class SpidRequestModel
    {
        [JsonProperty(PropertyName = "referer")]
        public string Referer { get; set; }

        [JsonProperty(PropertyName = "useragent")]
        public string UserAgent { get; set; }

        [JsonProperty(PropertyName = "shibcookiename")]
        public string ShibCookieName { get; set; }

        [JsonProperty(PropertyName = "shibsessionid")]
        public string ShibSessionId { get; set; }

        [JsonProperty(PropertyName = "shibsessionindex")]
        public string ShibSessionIndex { get; set; }

        [JsonProperty(PropertyName = "shibsessionexpires")]
        public string ShibSessionExpires { get; set; }

        [JsonProperty(PropertyName = "shibsessioninactivity")]
        public string ShibSessionInactivity { get; set; }

        [JsonProperty(PropertyName = "shibidentityprovider")]
        public string ShibIdentityProvider { get; set; }

        [JsonProperty(PropertyName = "shibauthenticationmethod")]
        public string ShibAuthenticationMethod { get; set; }

        [JsonProperty(PropertyName = "shibauthenticationinstant")]
        public string ShibAuthenticationInstant { get; set; }

        [JsonProperty(PropertyName = "shibauthncontextclass")]
        public string ShibAuthncontextClass { get; set; }

        [JsonProperty(PropertyName = "shibauthncontextdecl")]
        public string ShibAuthncontextDecl { get; set; }

        [JsonProperty(PropertyName = "shibassertioncount")]
        public string ShibAssertionCount { get; set; }

        [JsonProperty(PropertyName = "shibhandler")]
        public string ShibHandler { get; set; }

        [JsonProperty(PropertyName = "uniqueid")]
        public string Uniqueid { get; set; }

        [JsonProperty(PropertyName = "xforwardedfor")]
        public string XForwardedFor { get; set; }

        [JsonProperty(PropertyName = "xclientip")]
        public string XClientIp { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "companyname")]
        public string Companyname { get; set; }

        [JsonProperty(PropertyName = "countyofbirth")]
        public string Countyofbirth { get; set; }

        [JsonProperty(PropertyName = "dateofbirth")]
        public string Dateofbirth { get; set; }

        [JsonProperty(PropertyName = "digitaladdress")]
        public string Digitaladdress { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "expirationdate")]
        public string Expirationdate { get; set; }

        [JsonProperty(PropertyName = "familyname")]
        public string Familyname { get; set; }

        [JsonProperty(PropertyName = "fiscalnumber")]
        public string Fiscalnumber { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "idcard")]
        public string Idcard { get; set; }

        [JsonProperty(PropertyName = "ivacode")]
        public string Ivacode { get; set; }

        [JsonProperty(PropertyName = "mobilephone")]
        public string Mobilephone { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "placeofbirth")]
        public string Placeofbirth { get; set; }

        [JsonProperty(PropertyName = "registeredoffice")]
        public string Registeredoffice { get; set; }

        [JsonProperty(PropertyName = "spidcode")]
        public string Spidcode { get; set; }

        [JsonProperty(PropertyName = "created")]
        public string Created { get; set; }
    }
}