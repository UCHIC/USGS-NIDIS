﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CustomInstallAction {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class CustomAction : global::System.Configuration.ApplicationSettingsBase {
        
        private static CustomAction defaultInstance = ((CustomAction)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new CustomAction())));
        
        public static CustomAction Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GenericOD webservice")]
        public string ServerComment {
            get {
                return ((string)(this["ServerComment"]));
            }
            set {
                this["ServerComment"] = value;
            }
        }
    }
}
