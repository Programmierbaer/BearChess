﻿#pragma checksum "..\..\..\Windows\WinConfigureOSA.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "E5B3C8AAD27FF5DFC6F137B40CC6C497DA740F4796FF1F39D81F9C79447C1C96"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

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
using www.SoLaNoSoft.com.BearChessWin.Properties;
using www.SoLaNoSoft.com.BearChessWin.Windows;


namespace www.SoLaNoSoft.com.BearChessWin {
    
    
    /// <summary>
    /// WinConfigureOSA
    /// </summary>
    public partial class WinConfigureOSA : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 28 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock textBlockCurrentPort;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock textBlockCurrentBaud;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox comboBoxComPorts;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox comboBoxBaud;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonCheck;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock textBlockCheck;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock textBlockInformation;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonOk;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\..\Windows\WinConfigureOSA.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonCancel;
        
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
            System.Uri resourceLocater = new System.Uri("/BearChessWin;component/windows/winconfigureosa.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\WinConfigureOSA.xaml"
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
            this.textBlockCurrentPort = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.textBlockCurrentBaud = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.comboBoxComPorts = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.comboBoxBaud = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.buttonCheck = ((System.Windows.Controls.Button)(target));
            
            #line 45 "..\..\..\Windows\WinConfigureOSA.xaml"
            this.buttonCheck.Click += new System.Windows.RoutedEventHandler(this.ButtonCheck_OnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.textBlockCheck = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.textBlockInformation = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.buttonOk = ((System.Windows.Controls.Button)(target));
            
            #line 73 "..\..\..\Windows\WinConfigureOSA.xaml"
            this.buttonOk.Click += new System.Windows.RoutedEventHandler(this.ButtonOk_OnClick);
            
            #line default
            #line hidden
            return;
            case 9:
            this.buttonCancel = ((System.Windows.Controls.Button)(target));
            
            #line 78 "..\..\..\Windows\WinConfigureOSA.xaml"
            this.buttonCancel.Click += new System.Windows.RoutedEventHandler(this.ButtonCancel_OnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

