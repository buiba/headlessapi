using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EPiServer.ContentApi.Commerce.Internal.Models.Cart;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPiServer.ContentApi.IntegrationTests.Commerce
{
    public static class CommerceServiceFixtureExtensions
    {
        private static readonly Encoding Encoding = Encoding.UTF8;
        private const string MediaType = "application/json";

        public static async Task<(HttpResponseMessage,CartApiModel)> PostCartAsync(this CommerceServiceFixture fixture, CartApiModel model)
        {
            return await fixture.PostAsync<CartApiModel>(Constants.CartsApiBaseUrl, model);
        }

        public static async Task<(HttpResponseMessage, CartApiModel)> PutCartAsync(this CommerceServiceFixture fixture, Guid id, CartApiModel model)
        {
            var requestPayload = new StringContent(JsonConvert.SerializeObject(model), Encoding, MediaType);
            var response = await fixture.Client.PutAsync(Constants.CartsApiBaseUrl + $"{id}", requestPayload);
            var responseBody = await response.Content.ReadAsStringAsync();

            return (response, JsonConvert.DeserializeObject<CartApiModel>(responseBody));
        }

        public static async Task<(HttpResponseMessage, CartApiModel)> GetCartAsync(this CommerceServiceFixture fixture, object id)
        {
            return await fixture.GetAsync<CartApiModel>(Constants.CartsApiBaseUrl + id);
        }

        public static async Task<(HttpResponseMessage, T)> GetAsync<T>(this CommerceServiceFixture fixture, string requestUri)
        {
            var response = await fixture.Client.GetAsync(requestUri);
            var responseBody = await response.Content.ReadAsStringAsync();

            return (response, JsonConvert.DeserializeObject<T>(responseBody));
        }

        public static async Task<(HttpResponseMessage, object)> ConvertToOrderAsync(this CommerceServiceFixture fixture, object id)
        {
            var response = await fixture.Client.PostAsync(Constants.CartsApiBaseUrl + $"{id}/ConvertToOrder", null);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var jsonErrorMessage = JObject.Parse(responseBody);
                var errorResponseBody = JsonConvert.DeserializeObject<Error>(jsonErrorMessage["error"].ToString());
                return (response, errorResponseBody);
            }
            return (response, JsonConvert.DeserializeObject<OrderApiModel>(responseBody));
        }

        public static async Task<(HttpResponseMessage, OrderApiModel)> GetOrderAsync(this CommerceServiceFixture fixture, string orderNumber)
        {
            return await fixture.GetAsync<OrderApiModel>(Constants.OrdersApiBaseUrl + orderNumber);
        }

        public static async Task<(HttpResponseMessage, T)> PostOrderAsync<T>(this CommerceServiceFixture fixture, OrderInputModel model)
        {
            return await fixture.PostAsync<T>(Constants.OrdersApiBaseUrl, model);
        }

        public static async Task<(HttpResponseMessage, CheckoutApiModel)> PrepareCheckoutAsync(this CommerceServiceFixture fixture, object id)
        {
            return await fixture.PostAsync<CheckoutApiModel>(Constants.CartsApiBaseUrl + $"{id}/preparecheckout", "");
        }

        public static async Task<(HttpResponseMessage, T)> PostAsync<T>(this CommerceServiceFixture fixture, string requestUri, object payload)
        {
            var requestPayload = new StringContent(JsonConvert.SerializeObject(payload), Encoding, MediaType);
            var response = await fixture.Client.PostAsync(requestUri, requestPayload);
            var responseBody = await response.Content.ReadAsStringAsync();

            return (response, JsonConvert.DeserializeObject<T>(responseBody));
        }
    }
}
