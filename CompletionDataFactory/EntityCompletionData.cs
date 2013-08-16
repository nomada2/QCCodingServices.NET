using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;
using QuantConnect.Server.Autocomplete.CompletionDataFactory;

namespace QuantConnect.Server.Autocomplete.CompletionDataFactory
{
    public partial class CodeCompletionDataFactory
    {
        public class EntityCompletionData : CompletionData, IEntityCompletionData
        {
            #region IEntityCompletionData implementation

            public IEntity Entity { get; private set; }

            #endregion

            public EntityCompletionData(IEntity entity)
                : this(entity, entity.Name)
            {
                // Don't put any custom code in here.  The constructor overload is the place for that.
            }

            public EntityCompletionData(IEntity entity, string entityName)
                : base(entityName)
            {
                Entity = entity;
                DeclarationCategory = entity.EntityType.ResolveDeclarationCategoryFromEntityType();
                Description = entity.Documentation;
            }

        }

        /// <summary>
        /// These appear to represent members of classes
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IEntity entity)
        {
            var cd = new EntityCompletionData(entity);
            return cd;
        }

        /// <summary>
        /// No usages???
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IEntity entity, string text)
        {
            var cd = new CompletionData(text);
            return cd;
        }

        /// <summary>
        /// No usages???
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IUnresolvedEntity entity)
        {
            var cd = new CompletionData(entity.Name);
            return cd;
        }



    }
}
