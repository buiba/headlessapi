using System;
using System.Collections.Generic;
using EPiServer.ContentManagementApi.Models.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    internal class ContentApiPatchModelJsonConverter : JsonConverter
    {
        private static readonly ISet<string> PatchMetadata = new HashSet<string>()
        {
            nameof(ContentApiPatchModel.RouteSegment),
            nameof(ContentApiPatchModel.StartPublish),
            nameof(ContentApiPatchModel.StopPublish),
            nameof(ContentApiPatchModel.Status)
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(ContentApiPatchModel);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jsonObject = JObject.Load(reader);
            var model = new ContentApiPatchModel();

            serializer.Populate(jsonObject.CreateReader(), model);

            foreach (var metadataProp in PatchMetadata)
            {
                if (jsonObject.TryGetValue(metadataProp, StringComparison.OrdinalIgnoreCase, out _))
                {
                    model.UpdatedMetadata.Add(metadataProp);
                }
            }

            return model;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
