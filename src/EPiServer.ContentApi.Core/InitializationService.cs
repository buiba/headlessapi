using EPiServer.ContentApi.Core.Configuration;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace EPiServer.ContentApi.Core
{
    /// <summary>
    /// Setup initial configuration for Content Delivery. E.g virtual roles, HttpConfiguration
    /// </summary>
    [ServiceConfiguration(typeof(InitializationService))]
    public class InitializationService
    {
        protected static readonly ILogger _logger = LogManager.GetLogger();
        private static object _lock = new object();
        private static bool _isInitialized = false;
        protected readonly IVirtualRoleRepository _virtualRoleRepository;
        protected readonly ContentApiConfiguration _apiConfig;

        [Obsolete("This will be removed in the next major release")]
        protected readonly UIRoleProvider _uiRoleProvider;

        public InitializationService() : this(
            ServiceLocator.Current.GetInstance<IVirtualRoleRepository>(),
            ServiceLocator.Current.GetInstance<ContentApiConfiguration>())
        {
        }

        public InitializationService(IVirtualRoleRepository virtualRoleRepository,
        ContentApiConfiguration appConfig)
        {
            _virtualRoleRepository = virtualRoleRepository;
            _apiConfig = appConfig;
        }

        [Obsolete("Use constructor without UIRoleProvider")]
        public InitializationService(
            IVirtualRoleRepository virtualRoleRepository,
            ContentApiConfiguration appConfig,
            UIRoleProvider uIRoleProvider) : this(virtualRoleRepository, appConfig)
        {
        }

        /// <summary>
        /// Initialize virtual roles
        /// </summary>
        public virtual void InitializeVirtualRoles()
        {
            if (_isInitialized)
            {
                return;
            }

            lock (_lock)
            {
                _logger.Information("Start initializing virtual role for Headless APi");

                var contentApiOption = _apiConfig.GetOptions();
                var requiredRole = contentApiOption.RequiredRole;
                if (string.IsNullOrWhiteSpace(requiredRole))
                {
                    return;
                }

                var allVirtualRoles = _virtualRoleRepository.GetAllRoles();
                if (allVirtualRoles != null && allVirtualRoles.Any(vr => string.Equals(vr, requiredRole, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                };

                var allRoles = Enumerable.Empty<string>();
                try
                {
                    allRoles = GetAllRoles();
                }
                catch (Exception ex)
                {
                    // CMS UI error due to not checking null before .ToList() when getting roles
                    // we just log the error here and let the application continue initializing
                    _logger.Error("The site may have just started for the first time, so virtual roles required for Headless cannot be instantiated. Please, restart the site.", ex);
                }

                if (allRoles == null || !allRoles.Any())
                {
                    return;
                }

                RegisterVirtualRole(allRoles);

                _isInitialized = true;

                _logger.Information("Process ends.");
            }
        }

        /// <summary>
        /// Determines whether specified role is content api read or not.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>true if the role is content api read, otherwise false.</returns>
        protected virtual bool IsContentApiRole(string roleName)
        {
            return Regex.IsMatch(roleName, @"(^Administrators$)|(\\Administrators$)|(^WebAdmins$)|(\\WebAdmins$)|(^WebEditors$)|(\\WebEditors$)|(^CommerceAdmins$)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        }

        /// <summary>
        /// Get all roles from system
        /// </summary>
        protected virtual IEnumerable<string> GetAllRoles()
        {
            return new string[] { "Administrators", "WebAdmins", "WebEditors", "CommerceAdmins" };
        }

        /// <summary>
        /// Register virtual role with mapped roles
        /// </summary>
        protected virtual void RegisterVirtualRole(IEnumerable<string> allRoles)
        {
            var requiredRole = _apiConfig.GetOptions().RequiredRole;
            var mappedRoles = allRoles.Where(IsContentApiRole);

            if (!_virtualRoleRepository.GetAllRoles().Contains(requiredRole))
            {
                var mr = new MappedRole(_virtualRoleRepository);
                mr.Initialize(requiredRole, new NameValueCollection { { "mode", "Any" }, { "roles", string.Join(",", mappedRoles.ToArray()) } });
                _virtualRoleRepository.Register(requiredRole, mr);
            }
        }
    }
}
