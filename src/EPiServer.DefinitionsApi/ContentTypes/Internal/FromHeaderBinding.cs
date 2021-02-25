using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using EPiServer.ContentApi.Error.Internal;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    // This class will be completely different in .NET Core - so don't bother creating reusable component.
    internal class FromHeaderBinding : HttpParameterBinding
    {
        private readonly string _name;

        public FromHeaderBinding(HttpParameterDescriptor parameter, string headerName)
            : base(parameter)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentNullException(nameof(headerName));
            }

            _name = headerName;
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext.Request.Headers.TryGetValues(_name, out var values))
            {
                var converter = TypeDescriptor.GetConverter(Descriptor.ParameterType);
                try
                {
                    actionContext.ActionArguments[Descriptor.ParameterName] = converter.ConvertFromString(values.FirstOrDefault());
                }
                catch (Exception)
                {
                    throw new ErrorException(HttpStatusCode.BadRequest, $"The {_name} header contains an invalid value");
                }
            }
            else if (Descriptor.IsOptional)
            {
                actionContext.ActionArguments[Descriptor.ParameterName] = DefaultParameterValue();
            }
            else
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"The {_name} header must be provided.");
            }

            return Task.CompletedTask;
        }

        private object DefaultParameterValue() => Descriptor.DefaultValue ?? (Descriptor.ParameterType.IsValueType ? Activator.CreateInstance(Descriptor.ParameterType) : null);
    }
}
