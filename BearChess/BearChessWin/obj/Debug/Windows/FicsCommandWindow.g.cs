﻿#pragma checksum "..\..\..\Windows\FicsCommandWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "93AAE12688BBFFE9D3F3AE4A4DA5FA1E1BC7A219B41AAEE8B966CB233F6BBB4B"
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
using www.SoLaNoSoft.com.BearChessWin.Windows;


namespace www.SoLaNoSoft.com.BearChessWin {
    
    
    /// <summary>
    /// FicsCommandWindow
    /// </summary>
    public partial class FicsCommandWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\..\Windows\FicsCommandWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox textBlockDescription;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\Windows\FicsCommandWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox textBlockCommand;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Windows\FicsCommandWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonOk;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Windows\FicsCommandWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonClear;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\Windows\FicsCommandWindow.xaml"
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
            System.Uri resourceLocater = new System.Uri("/BearChessWin;component/windows/ficscommandwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\FicsCommandWindow.xaml"
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
            
            #line 7 "..\..\..\Windows\FicsCommandWindow.xaml"
            ((www.SoLaNoSoft.com.BearChessWin.FicsCommandWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.FicsCommandWindow_OnClosing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.textBlockDescription = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.textBlockCommand = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.buttonOk = ((System.Windows.Controls.Button)(target));
            
            #line 24 "..\..\..\Windows\FicsCommandWindow.xaml"
            this.buttonOk.Click += new System.Windows.RoutedEventHandler(this.ButtonOk_OnClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.buttonClear = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\..\Windows\FicsCommandWindow.xaml"
            this.buttonClear.Click += new System.Windows.RoutedEventHandler(this.ButtonClear_OnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.buttonCancel = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

