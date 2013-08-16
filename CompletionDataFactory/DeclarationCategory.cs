using System;
using ICSharpCode.NRefactory.TypeSystem;

namespace QuantConnect.Server.Autocomplete.CompletionDataFactory
{
    /// <summary>
    /// Indicates how the completion option is declared
    /// </summary>
    public enum DeclarationCategory
    {
        NotSet = 0,

        Anonymous,
        // not sure how this figures in...
        Array,
        Dynamic,
        Literal,
        Void,
        Namespace,
        Enum,
        EnumerationMember,
        Struct,
        Interface,
        Class,
        StaticClass,
        AbstractClass,
        MethodParameter,
        Delegate,
        TypeParameter,
        Variable,

        ClassMemberConstant,
        ClassMemberField,
        ClassMemberMethod,
        ClassMemberProperty,
        ClassMemberEvent,
        ClassMemberIndexer,
        ClassMemberOperator,
        ClassMemberConstructor,
        ClassMemberDestructor,
        ClassMemberStaticConstructor,
        ClassMemberType
    }

    public static class _DeclarationCategoryExtensionMethods
    {
        public static DeclarationCategory ResolveDeclarationCategoryFromEntityType(this EntityType type)
        {
            switch (type)
            {
                //case EntityType.Accessor:
                case EntityType.Constructor: return DeclarationCategory.ClassMemberConstructor;
                case EntityType.Destructor: return DeclarationCategory.ClassMemberDestructor;
                case EntityType.Event: return DeclarationCategory.ClassMemberEvent;
                case EntityType.Field: return DeclarationCategory.ClassMemberField;
                case EntityType.Indexer: return DeclarationCategory.ClassMemberIndexer;
                case EntityType.Method: return DeclarationCategory.ClassMemberMethod;
                case EntityType.Operator: return DeclarationCategory.ClassMemberOperator;
                case EntityType.Property: return DeclarationCategory.ClassMemberProperty;
                case EntityType.TypeDefinition: return DeclarationCategory.ClassMemberType;
                case EntityType.None: throw new Exception("EntityType.None is not supported");
            }
            return DeclarationCategory.NotSet;
        }

        public static DeclarationCategory ResolveDeclarationCategoryFromTypeKind(this TypeKind typeKind)
        {
            switch (typeKind)
            {
                case TypeKind.Anonymous: return DeclarationCategory.Anonymous;
                case TypeKind.Class: return DeclarationCategory.Class;
                case TypeKind.Delegate: return DeclarationCategory.Delegate;
                case TypeKind.Dynamic: return DeclarationCategory.Dynamic;
                case TypeKind.Enum: return DeclarationCategory.Enum;
                case TypeKind.Interface: return DeclarationCategory.Interface;
                case TypeKind.Struct: return DeclarationCategory.Struct;
                case TypeKind.TypeParameter: return DeclarationCategory.TypeParameter;
                case TypeKind.Void: return DeclarationCategory.Void;
                //case TypeKind.Array: return DeclarationCategory.Array;
                default:
                    throw new Exception("EntityType.None is not supported");
            }
            return DeclarationCategory.NotSet;
        }
    }
}