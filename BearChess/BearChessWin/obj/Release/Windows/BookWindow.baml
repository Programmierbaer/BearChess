   M S B A M L   `   `   `  ����   BearChessWin0    *www.SoLaNoSoft.com.BearChessWin.BookWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     � Icon$#  ../Assets/Icons/book_open.png�� � 
ResizeMode$ NoResize=� � Title$
 Book��$	��290��$	��290��$	��300��.��]� ��.  .+�&   System.Windows.Controls.DataGrid  -   $��dataGridMoves��$	��190��$	��Top=�$	��Top=�$��0q�
������ "   VerticalScrollBarVisibility$
 Auto=�$��0��$��5q�   AutoGenerateColumns .     
IsReadOnly . ����    Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *	FromField+	   Header$
	 From��  $
 Auto  �� *	ToField+$	 To��  $
 Auto  �� *Weight+$	 Weight��  $
 Auto  �� *	NoOfGames+$	 Games��  $
 Auto  �� *
NoOfWins+$
	 Wins��  $
 Auto  �� *
NoOfLoss+$
	 Lost���� -   $��buttonCancel��$��5q�$��Bottom=�$��Right=�
������ 
������ .���� $"��../Assets/Icons/door_out.png��$��32��$��32��