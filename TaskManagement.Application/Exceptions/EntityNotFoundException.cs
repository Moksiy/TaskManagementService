using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when an entity is not found
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// Creates a new EntityNotFoundException
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        /// <param name="id">Entity identifier</param>
        public EntityNotFoundException(string entityType, object id)
            : base($"{entityType} with id {id} was not found.")
        {
            EntityType = entityType;
            Id = id;
        }

        /// <summary>
        /// Type of entity that was not found
        /// </summary>
        public string EntityType { get; }

        /// <summary>
        /// Identifier of the entity that was not found
        /// </summary>
        public object Id { get; }
    }
}
