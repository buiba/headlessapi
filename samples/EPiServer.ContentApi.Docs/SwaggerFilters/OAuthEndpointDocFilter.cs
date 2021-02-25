using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace EPiServer.ContentApi.Docs
{
    /// <summary>
    ///     Swagger could not detect OAuth API so we have to do it programmatically.
    /// </summary>
    public class OAuthEndpointDocFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.paths.Add("/api/episerver/auth/token", GetAccessTokenByUsernameAndPassPath());
            swaggerDoc.paths.Add("/api/episerver/auth/token/", GetAccessTokenByRefreshTokenPath());
        }

        private PathItem GetAccessTokenByRefreshTokenPath()
        {
            var pathDesc = new PathItem();

            pathDesc.post = new Operation()
            {
                tags = new[] { "Authorization" },
                operationId = "Authorization_ByRefreshToken",
                consumes = new[] { "application/x-www-form-urlencoded" },
                produces = new[] { "application/json", "text/json" },
                description = "Get new access token by using refresh token",
                summary = "Get token by using refresh token."
            };

            pathDesc.parameters = new List<Parameter>();
            pathDesc.parameters.Add(new Parameter
            {
                name = "Accept",
                @in = "header",
                description = "Accept Header Value should be application/json",
                required = false,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "Content-Type",
                @in = "header",
                description = "Value should be application/x-www-form-urlencoded",
                required = false,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "grant_type",
                @in = "body",
                description = "Value should be refresh_token",
                required = true,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "refresh_token",
                @in = "body",
                description = "Your refresh token provided by Oauth",
                required = true,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "client_id",
                @in = "body",
                description = "Value should be Default",
                required = true,
                type = "string"
            });

            var exampleResponse = new
            {
                access_token = "KSNPBKtdWZkaCCybyzTrt6w3vKVW_psSU7IwB6AipdsH3Ed4Gj-tEP_cLMcg3-TNVSajzSZYNknV7ceC5E9ax3MEF-8G7tWm9gjKVUGM7imCnEzmuTjSGeo1l4t5ZXtS3A-PJRXHw6NxsigKhPnWxSDgkPWo6iFAjV2xs_6aKD5n6zT55Dhu_6R84VfY2WwNy9wyPW8wFroi2yZk5lcmNUNsLr_WPv9eYdLnBj4TxAeUGLJX6RGSbZGYjP_dnDA97ha-4QYskodJyXSTq9IwXyQtGZLo8N9jLDVgALocdSybAB1docAMXYjTanmrImLAIUKjcoA_PYX7F9P84FnuN9bH046jXUEf9J2fPo6NlGvtvrMpnhcCaCT00cRBvnFLhn99MSbEKVK1NNaM8QdTUC2xC5NRPAmR77kLVm5Qmsj15drmjth8Kqam8377ht9o4j3A7rY4L-5dRY6avX7MTDqI_ufGuZ-ikjZtdO6DGnispaSNKCYgQBNoXafLZKeOyZPiCD4IFMfD-01YdnqJgQ",
                token_type = "bearer",
                expires_in = 1199,
                refresh_token = "fa7dc977c2674d6b99ca4f742ce956c8",
                client_id = "Default",
                username = "Your username",
                issued = "Fri, 06 Jul 2018 10:30:50 GMT",
                expires = "Fri, 06 Jul 2018 10:50:50 GMT"
            };

            pathDesc.post.responses = new Dictionary<string, Response>();
            pathDesc.post.responses.Add("200", new Response() { description = "OK", schema = new Schema() { type = "object" }, examples = exampleResponse });
            return pathDesc;
        }

        private PathItem GetAccessTokenByUsernameAndPassPath()
        {
            var pathDesc = new PathItem();
            pathDesc.post = new Operation()
            {
                tags = new[] { "Authorization" },
                operationId = "Authorization_GetTokenByUsernameAndPass",
                consumes = new[] { "application/x-www-form-urlencoded" },
                produces = new[] { "application/json", "text/json" },
                description = "Authorization",
                summary = "Get token by username and password."
            };

            pathDesc.parameters = new List<Parameter>();
            pathDesc.parameters.Add(new Parameter
            {
                name = "Accept",
                @in = "header",
                description = "Accept Header Value should be application/json",
                required = false,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "Content-Type",
                @in = "header",
                description = "Value should be application/x-www-form-urlencoded",
                required = false,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "grant_type",
                @in = "body",
                description = "Value should be password",
                required = true,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "username",
                @in = "body",
                description = "Value should be the username in the system used for authentication and authorisation",
                required = true,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "password",
                @in = "body",
                description = "Value should be username's password",
                required = true,
                type = "string"
            });
            pathDesc.parameters.Add(new Parameter
            {
                name = "client_id",
                @in = "body",
                description = "Value should be Default",
                required = true,
                type = "string"
            });

            var exampleResponse = new
            {
                access_token = "KSNPBKtdWZkaCCybyzTrt6w3vKVW_psSU7IwB6AipdsH3Ed4Gj-tEP_cLMcg3-TNVSajzSZYNknV7ceC5E9ax3MEF-8G7tWm9gjKVUGM7imCnEzmuTjSGeo1l4t5ZXtS3A-PJRXHw6NxsigKhPnWxSDgkPWo6iFAjV2xs_6aKD5n6zT55Dhu_6R84VfY2WwNy9wyPW8wFroi2yZk5lcmNUNsLr_WPv9eYdLnBj4TxAeUGLJX6RGSbZGYjP_dnDA97ha-4QYskodJyXSTq9IwXyQtGZLo8N9jLDVgALocdSybAB1docAMXYjTanmrImLAIUKjcoA_PYX7F9P84FnuN9bH046jXUEf9J2fPo6NlGvtvrMpnhcCaCT00cRBvnFLhn99MSbEKVK1NNaM8QdTUC2xC5NRPAmR77kLVm5Qmsj15drmjth8Kqam8377ht9o4j3A7rY4L-5dRY6avX7MTDqI_ufGuZ-ikjZtdO6DGnispaSNKCYgQBNoXafLZKeOyZPiCD4IFMfD-01YdnqJgQ",
                token_type = "bearer",
                expires_in = 1199,
                refresh_token = "fa7dc977c2674d6b99ca4f742ce956c8",
                client_id = "Default",
                username = "Your username",
                issued = "Fri, 06 Jul 2018 10:30:50 GMT",
                expires = "Fri, 06 Jul 2018 10:50:50 GMT"
            };


            pathDesc.post.responses = new Dictionary<string, Response>();
            pathDesc.post.responses.Add("200", new Response() { description = "OK", schema = new Schema() { type = "object" }, examples = exampleResponse });
            return pathDesc;
        }
    }
}
