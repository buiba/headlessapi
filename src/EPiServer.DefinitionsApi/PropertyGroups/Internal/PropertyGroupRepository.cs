using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.DataAbstraction;

namespace EPiServer.DefinitionsApi.PropertyGroups.Internal
{
    internal class PropertyGroupRepository
    {
        private readonly ITabDefinitionRepository _tabDefinitionRepository;

        /// <summary>
        /// For mocking.
        /// </summary>
        internal PropertyGroupRepository()
        { }

        public PropertyGroupRepository(ITabDefinitionRepository tabDefinitionRepository)
        {
            _tabDefinitionRepository = tabDefinitionRepository;
        }

        public PropertyGroupModel Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var tabDefinition = _tabDefinitionRepository.Load(name);

            if (tabDefinition is null)
            {
                return null;
            }

            return FromTabDefinition(tabDefinition);
        }

        public IEnumerable<PropertyGroupModel> List()
        {
            return _tabDefinitionRepository.List()
                .Select(FromTabDefinition);
        }

        /// <summary>
        /// Stores or updates a specified property group in the repository.
        /// </summary>
        /// <param name="propertyGroup">The property group that should be persisted.</param>
        /// <returns>The kind of action that the method did:
        /// SaveResult.Created: The propertyGroup is created
        /// SaveResult.Updated: The propertyGroup is updated
        /// </returns>
        public virtual SaveResult Save(PropertyGroupModel propertyGroup)
        {
            if (propertyGroup is null)
            {
                throw new ArgumentNullException(nameof(propertyGroup));
            }

            if (string.IsNullOrEmpty(propertyGroup.Name))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"The Name of property group is required", ProblemCode.SystemGroup);
            }

            var existing = _tabDefinitionRepository.Load(propertyGroup.Name);

            return existing is null ? Create(propertyGroup) : Update(existing, propertyGroup);
        }

        internal SaveResult Update(TabDefinition existingTabDefinition, PropertyGroupModel propertyGroup)
        {
            if (existingTabDefinition is null)
            {
                throw new ArgumentNullException(nameof(existingTabDefinition));
            }

            if (propertyGroup is null)
            {
                throw new ArgumentNullException(nameof(propertyGroup));
            }

            if (propertyGroup.SystemGroup.HasValue && propertyGroup.SystemGroup != existingTabDefinition.IsSystemTab)
            {
                throw new ErrorException(HttpStatusCode.Conflict, $"The system group property is read-only and cannot be modified.", ProblemCode.SystemGroup);
            }

            var writableObject = existingTabDefinition.CreateWritableClone();

            writableObject.Name = propertyGroup.Name;
            writableObject.DisplayName = propertyGroup.DisplayName;
            writableObject.SortIndex = propertyGroup.SortIndex;

            _tabDefinitionRepository.Save(writableObject);

            return SaveResult.Updated;
        }

        internal SaveResult Create(PropertyGroupModel propertyGroup)
        {
            if (propertyGroup is null)
            {
                throw new ArgumentNullException(nameof(propertyGroup));
            }

            if (propertyGroup.SystemGroup.HasValue && propertyGroup.SystemGroup.Value)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot create the system group", ProblemCode.SystemGroup);
            }

            var writableObject = new TabDefinition()
            {
                IsSystemTab = propertyGroup.SystemGroup ?? false,
                Name = propertyGroup.Name,
                DisplayName = propertyGroup.DisplayName,
                SortIndex = propertyGroup.SortIndex,
            };

            _tabDefinitionRepository.Save(writableObject);

            return SaveResult.Created;
        }

        public bool TryDelete(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var propertyGroup = _tabDefinitionRepository.Load(name);

            if (propertyGroup is null)
            {
                return false;
            }

            if (propertyGroup.IsSystemTab)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"The name '{propertyGroup.Name}' refers to a system group. System group are read-only and cannot be deleted", ProblemCode.SystemGroup);
            }

            _tabDefinitionRepository.Delete(propertyGroup);

            return true;
        }

        internal PropertyGroupModel FromTabDefinition(TabDefinition tabDefinition)
        {
            if (tabDefinition is null)
            {
                throw new ArgumentNullException(nameof(tabDefinition));
            }

            return new PropertyGroupModel
            {
                Name = tabDefinition.Name,
                DisplayName = tabDefinition.DisplayName,
                SortIndex = tabDefinition.SortIndex,
                SystemGroup = tabDefinition.IsSystemTab
            };
        }
    }
}
