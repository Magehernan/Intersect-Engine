﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Intersect.Building.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class KeyGenerationTaskStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal KeyGenerationTaskStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Intersect.Building.Properties.KeyGenerationTaskStrings", typeof(KeyGenerationTaskStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        internal static string ErrorCreatingOutputDirectory {
            get {
                return ResourceManager.GetString("ErrorCreatingOutputDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to read existing private key, regenerating..
        /// </summary>
        internal static string ErrorReadingPrivateKey {
            get {
                return ResourceManager.GetString("ErrorReadingPrivateKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to write new private key..
        /// </summary>
        internal static string ErrorWritingPrivateKey {
            get {
                return ResourceManager.GetString("ErrorWritingPrivateKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to write new public key..
        /// </summary>
        internal static string ErrorWritingPublicKey {
            get {
                return ResourceManager.GetString("ErrorWritingPublicKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to write new public key in Unity folder..
        /// </summary>
        internal static string ErrorWritingPublicKeyUnity {
            get {
                return ResourceManager.GetString("ErrorWritingPublicKeyUnity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GenerateEachBuild is false and keys already exist in {0}, skipping..
        /// </summary>
        internal static string KeysAlreadyExist {
            get {
                return ResourceManager.GetString("KeysAlreadyExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to New keys successfully generated..
        /// </summary>
        internal static string KeysGenerated {
            get {
                return ResourceManager.GetString("KeysGenerated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to KeySize must be a power of 2 above 1024 but received {0}..
        /// </summary>
        internal static string KeySizeInvalid {
            get {
                return ResourceManager.GetString("KeySizeInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid or no OutputDirectory was specified..
        /// </summary>
        internal static string OutputDirectoryInvalid {
            get {
                return ResourceManager.GetString("OutputDirectoryInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The existing private key is invalid, regenerating..
        /// </summary>
        internal static string PrivateKeyInvalid {
            get {
                return ResourceManager.GetString("PrivateKeyInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Public key was missing but the private key already exists, regenerating only the public key..
        /// </summary>
        internal static string PublicKeyMissing {
            get {
                return ResourceManager.GetString("PublicKeyMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully regenerated the public key..
        /// </summary>
        internal static string PublicKeyRegenerated {
            get {
                return ResourceManager.GetString("PublicKeyRegenerated", resourceCulture);
            }
        }
    }
}
