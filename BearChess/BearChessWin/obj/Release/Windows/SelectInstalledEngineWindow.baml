   M S B A M L   `   `   `  ����   BearChessWinA    ;www.SoLaNoSoft.com.BearChessWin.SelectInstalledEngineWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � Icon$  ../Assets/Icons/robot.png�� � 
ResizeMode$ NoResize=� � SizeToContent$ WidthAndHeight=� � WindowStartupLocation$ CenterOwner=� � Title$! Load and Manage UCI Engines��$	��390��$	��450��$	��450��.���  � RowDefinitions �� $	��290 ��� $
��Auto �.+�&   System.Windows.Controls.DataGrid  -   $��dataGridEngine��$��0��$��5q�   AutoGenerateColumns .     SelectionMode$ Single=�   
IsReadOnly . 	 J� 	AllowDrop	 . ���� 
 � 	Resources
  �� Color��%�   (��       *1/SystemColors.InactiveSelectionHighlightBrushKey+)(��      *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� $ CornflowerBlue���� $ White��   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Name+   Header$
 Name��  $
 Auto  �� *Author+$ Author��]� $��5q�$��1����.  .+��� -   $��buttonOk��$��5q�$
��Left=�
������ 
������  � ToolTip$ Load selected engine��.���� $��../Assets/Icons/robot.png��$��32��$��32���� -   $��buttonCancel��$��25,5,5,5q�$��Right=�
������ 
������ $ Close��.���� $"��../Assets/Icons/door_out.png��$��32��$��32���� -   $��buttonDelete��$��5,5,5,5q�$��Right=�
������ 
������ $ Uninstall selected engine��.���� $��../Assets/Icons/bin.png��$��32��$��32���� -   $��buttonInstall��$��5q�$��Right=�
������ 
������ $ Install a new engine��	 . .���� $,��&../Assets/Icons/file_extension_exe.png��$��32��$��32���� -   $��buttonConfigure��$��5,5,5,5q�$��Right=�
������ 
������ $ Configure selected engine��.���� $��../Assets/Icons/cog.png��$��32��$��32��