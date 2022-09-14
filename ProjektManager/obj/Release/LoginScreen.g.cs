﻿#pragma checksum "..\..\LoginScreen.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "A3C3614ED6C3F8FFDEB88C22EE4D1F16C01883D441119B3ED502C28FF2935BF7"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using ProjektManager;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ProjektManager {
    
    
    /// <summary>
    /// LoginScreen
    /// </summary>
    public partial class LoginScreen : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 70 "..\..\LoginScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button close_bt;
        
        #line default
        #line hidden
        
        
        #line 84 "..\..\LoginScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button min_bt;
        
        #line default
        #line hidden
        
        
        #line 133 "..\..\LoginScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtUsername;
        
        #line default
        #line hidden
        
        
        #line 150 "..\..\LoginScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox txtPassword;
        
        #line default
        #line hidden
        
        
        #line 157 "..\..\LoginScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button login_bt;
        
        #line default
        #line hidden
        
        
        #line 171 "..\..\LoginScreen.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button help;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ProjektManager;component/loginscreen.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\LoginScreen.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.close_bt = ((System.Windows.Controls.Button)(target));
            
            #line 72 "..\..\LoginScreen.xaml"
            this.close_bt.Click += new System.Windows.RoutedEventHandler(this.ButtonClose);
            
            #line default
            #line hidden
            return;
            case 2:
            this.min_bt = ((System.Windows.Controls.Button)(target));
            
            #line 86 "..\..\LoginScreen.xaml"
            this.min_bt.Click += new System.Windows.RoutedEventHandler(this.ButtonMinimize);
            
            #line default
            #line hidden
            return;
            case 3:
            this.txtUsername = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.txtPassword = ((System.Windows.Controls.PasswordBox)(target));
            
            #line 156 "..\..\LoginScreen.xaml"
            this.txtPassword.KeyDown += new System.Windows.Input.KeyEventHandler(this.OnKeyDownHandler);
            
            #line default
            #line hidden
            return;
            case 5:
            this.login_bt = ((System.Windows.Controls.Button)(target));
            
            #line 158 "..\..\LoginScreen.xaml"
            this.login_bt.Click += new System.Windows.RoutedEventHandler(this.Login);
            
            #line default
            #line hidden
            return;
            case 6:
            this.help = ((System.Windows.Controls.Button)(target));
            
            #line 172 "..\..\LoginScreen.xaml"
            this.help.Click += new System.Windows.RoutedEventHandler(this.Help_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

