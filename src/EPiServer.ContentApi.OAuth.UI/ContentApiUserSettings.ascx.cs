using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer.ContentApi.OAuth.Internal;
using EPiServer.Personalization;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.OAuth.UI
{
    [GuiPlugIn(LanguagePath = "/admin/edituser/contentapisettings", Area = PlugInArea.SidSettingsArea, Url = "~/modules/_protected/EPiServer.ContentDeliveryApi.OAuth/ContentApiUserSettings.ascx")]
    public partial class ContentApiUserSettings : UserControl, IUserSettings, ICustomPlugInLoader
    {
        private Injected<IRefreshTokenRepository> _refreshTokenRepository;
        private string _username;

        public void LoadSettings(string userName, EPiServerProfile data)
        {
            this._username = this.Page.User.Identity.Name;
            LoadRefreshTokensForUser(userName);
        }

        private void LoadRefreshTokensForUser(string userName)
        {
            var tokens = _refreshTokenRepository.Service.FindByUsername(userName);
            rptRefreshTokens.DataSource = tokens;
            rptRefreshTokens.DataBind();
        }

        public void SaveSettings(string userName, EPiServerProfile data)
        {

        }

        public bool SaveRequiresUIReload { get; set; }

        public PlugInDescriptor[] List()
        {
            return new[] { PlugInDescriptor.Load(typeof(ContentApiUserSettings)) };
        }

        protected void rptRefreshTokens_OnItemCommand(object source, RepeaterCommandEventArgs e)
        {
			var refreshTokenId = System.Guid.Empty;
			if (!System.Guid.TryParse(e.CommandArgument.ToString(), out refreshTokenId))
			{
				return;
			}

			var refreshToken =  _refreshTokenRepository.Service.FindById(refreshTokenId);
            if (refreshToken != null)
            {
				_refreshTokenRepository.Service.Remove(refreshToken);
			}

            LoadRefreshTokensForUser(this._username);
        }
    }
}