   M S B A M L   `   `   `  ����   BearChessWin?    9www.SoLaNoSoft.com.BearChessWin.SelectInstalledBookWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � Icon$#  ../Assets/Icons/book_open.png�� � SizeToContent$ WidthAndHeight=� � WindowStartupLocation$ CenterOwner=� � Title$! Load & Manage Opening Books��$	��250��$	��600��.���  � RowDefinitions �� $
��Auto ��� $
��Auto �.+�&   System.Windows.Controls.DataGrid  -   $��dataGridBook��$��0��$��5q�   AutoGenerateColumns .     SelectionMode$ Single=�   
IsReadOnly . ����  � 	Resources 	 �� Color��%�   (��       *1/SystemColors.InactiveSelectionHighlightBrushKey+)(��      *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� $	 CornflowerBlue���� $	 White��
   Columns
 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Name+   Header$
 Name��  $
 Auto  �� *
FileName+$
 File��  $
 Auto  �� *PositionsCount+$ 	Positions��  $
 Auto  �� *
MovesCount+$ Moves��  $
 Auto  �� *Size+$ Size (bytes)��]� $��5q�$��1����.  .+��� -   $��buttonOk��$��5q�$
��Left=�
������ 
������  � ToolTip$ Load selected book��.���� $#��../Assets/Icons/book_open.png��$��32��$��32���� -   $��buttonCancel��$��25,5,5,5q�$��Right=�
������ 
������ $ Cancel��.���� $"��../Assets/Icons/door_out.png��$��32��$��32���� -   $��buttonDelete��$��5,5,20,5q�$��Right=�
������ 
������ $ Uninstall selected book��.���� $��../Assets/Icons/bin.png��$��32��$��32���� -   $��buttonInstall��$��5,5,20,5q�$��Right=�
������ 
������ $ Install a new book��.���� $"��../Assets/Icons/book_add.png��$��32��$��32��