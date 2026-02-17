using System;
using SingularityGroup.HotReload.DTO;
using SingularityGroup.HotReload.Localization;

namespace SingularityGroup.HotReload {
    internal class SymbolResolvingFailedException : Exception {
        public SymbolResolvingFailedException(SMethod m, Exception inner) 
            : base(string.Format(Localization.Translations.Errors.UnableToResolveMethodInAssembly, m.displayName, m.assemblyName), inner) { }
        
        public SymbolResolvingFailedException(SType t, Exception inner) 
            : base(string.Format(Localization.Translations.Errors.UnableToResolveTypeInAssembly, t.typeName, t.assemblyName), inner) { }
        
        public SymbolResolvingFailedException(SField t, Exception inner) 
            : base(string.Format(Localization.Translations.Errors.UnableToResolveFieldInAssembly, t.fieldName, t.assemblyName), inner) { }
    }
}
