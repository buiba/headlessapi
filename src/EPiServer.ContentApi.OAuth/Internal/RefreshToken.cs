using EPiServer.Data.Dynamic;
using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Data;

namespace EPiServer.ContentApi.OAuth.Internal
{
	/// <summary>
	/// Default implementation of <see cref="IRefreshToken"/> used to store in DDS
	/// </summary>
	internal class RefreshToken : IRefreshToken, IDynamicData
	{
		/// <inheritdoc />
		[EPiServerIgnoreDataMember]
		public Guid Guid
		{
			get { return Id.ExternalId; }
			set { Id = Identity.NewIdentity(value); }
		}

		/// <summary>
		/// ID stored in DDS
		/// </summary>
		public Identity Id { get; set; }

		/// <inheritdoc />
		[Key]
        public string RefreshTokenValue { get; set; }

		/// <inheritdoc />
		[Required]
        [MaxLength(50)]
        public string Subject { get; set; }

		/// <inheritdoc />
		[Required]
        [MaxLength(50)]
        public string ClientId { get; set; }

		/// <inheritdoc />
		public DateTime IssuedUtc { get; set; }

		/// <inheritdoc />
		public DateTime ExpiresUtc { get; set; }

		/// <inheritdoc />
		[Required]
        public string ProtectedTicket { get; set; }
	}
}
