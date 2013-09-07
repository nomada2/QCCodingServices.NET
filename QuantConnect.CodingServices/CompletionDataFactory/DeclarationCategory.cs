using System;
using ICSharpCode.NRefactory.TypeSystem;

namespace QuantConnect.CodingServices.CompletionDataFactory
{
    /// <summary>
    /// Indicates how the completion option is declared
    /// See http://slps.github.com/zoo/index.html#cs for browsable C# grammars.
    /// </summary>
    public enum DeclarationCategory
    {
        NotSet = 0,

        // TODO: Some of these guys need to be factored out into a separate CompletionCategory enumeartion 
        //  (independent from the NRefactory CompletionCategory enum).

        XmlDocumentation,

        Literal,

        Namespace,

        Enumeration_Member,
        Local_Variable,
        //StaticClass,
        //AbstractClass,
        //MethodParameter,


        #region Type-oriented Categories

        Anonymous,
        //Array,  // not sure how this one figures in...
        Class,
        Delegate,
        Dynamic,
        Enum,
        Interface,
        Struct,
        Type_Parameter,
        Void,

        #endregion

        #region Class member categories (most of which are permissible in interfaces and structs)

        Constant,
        Field,
        Method,
        Property,
        Event,
        Indexer,
        Operator,
        Constructor,
        Destructor,
        Static_Constructor,

        /// <summary>
        /// An embedded symbolKind definition
        /// </summary>
        Type_Definition,
        
        #endregion

    }

    // TODO: We need an extra access modifier enumeration for code completion members that support access modifiers


    public static class _DeclarationCategoryExtensionMethods
    {
        public static DeclarationCategory ResolveDeclarationCategoryFromSymbolKind(this SymbolKind symbolKind)
        {
            switch (symbolKind)
            {
                //case SymbolKind.Accessor:
                case SymbolKind.Constructor: return DeclarationCategory.Constructor;
                case SymbolKind.Destructor: return DeclarationCategory.Destructor;
                case SymbolKind.Event: return DeclarationCategory.Event;
                case SymbolKind.Field: return DeclarationCategory.Field;
                case SymbolKind.Indexer: return DeclarationCategory.Indexer;
                case SymbolKind.Method: return DeclarationCategory.Method;
                case SymbolKind.Operator: return DeclarationCategory.Operator;
                case SymbolKind.Property: return DeclarationCategory.Property;
                case SymbolKind.TypeDefinition: return DeclarationCategory.Type_Definition;
                case SymbolKind.None: throw new Exception("SymbolKind.None is not supported");
                default:
                    throw new Exception("Unsupported SymbolKind: "+symbolKind);
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
                case TypeKind.TypeParameter: return DeclarationCategory.Type_Parameter;
                case TypeKind.Void: return DeclarationCategory.Void;
                //case TypeKind.Array: return DeclarationCategory.Array;
                default:
                    throw new Exception("Unsupported TypeKind: "+typeKind);
            }
            return DeclarationCategory.NotSet;
        }
    }
}