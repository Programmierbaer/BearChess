   M S B A M L   `   `   `  ����M  IBearChessServerWin, Version=0.1.0.0, Culture=neutral, PublicKeyToken=nullm;clr-namespace:www.SoLaNoSoft.com.BearChessServerWin.Windows-www.SoLaNoSoft.com.BearChessServerWin.Windows  5   	   W SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�cclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib;assembly=BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib 5   	   s>clr-namespace:www.SoLaNoSoft.com.BearChessServerWin.Properties0www.SoLaNoSoft.com.BearChessServerWin.Properties  5   	   I    Cwww.SoLaNoSoft.com.BearChessServerWin.Windows.QueryTournamentWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Mlocal;clr-namespace:www.SoLaNoSoft.com.BearChessServerWin.Windows     5   	   wcontrolcclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib;assembly=BearChessWpfCustomControlLib     5   	   Lp>clr-namespace:www.SoLaNoSoft.com.BearChessServerWin.Properties     5   	   -   5          � SizeToContent$  WidthAndHeight=�5	       � WindowStartupLocation$ CenterOwner=�69   @   :www.SoLaNoSoft.com.BearChessServerWin.Properties.Resources   
Tournament � Title# Z 5
   	   $	��200��63   $	��350��6@   .��5      � $��5q�6    +� FocusedElement 6   ��  �� ElementName$ textBoxName�� � ColumnDefinitions 5   
   �� 5      $
��Auto �6   6.   �� 5      $
��Auto �6   6.    � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� $��5q�6   $��0��6    $��0��60      TournamentName#��Z 6=   $��Center=�6j   �� 5   
   -   5        $��textBoxName��5      $��0��6   $
��Left=�63   $��5q�6O   $	��200��6[   $��1��6h   $��0��6x   6�   �� 5   
   $��5q�6   $��0��6    $��1��60   	   NumberOfBoards#��Z	 6=   $��Center=�6j   N  Hwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.NumericUpDownUserControl  5   
   -   5        
  Name$#
 numericUpDownUserControBoards��5   B   $��1��5   +   $��5q�68   $��1��6C   $��1��6S   $
��Left=�6`      MinValue$ 1��5   B      Value$ 1��6O      MaxValue$ 15��5   B      TextWidthProperty$ 30��6P   ]� 5   
   $��0��6   $��2��6$   $��2��68   $��5,10,5,5q�6E   $��Bottom=�6W   ��.  .+�5      �� -   5        $��buttonOk��5      $��5q�6&   $
��Left=�61   $��28��6H   $��28��6S   
������ 
������  �� 	IsDefault . .��5      �� $'��!../Assets/Icons/accept_button.png��6   6F   5      �� 5       -   5        $��buttonCancel��5       $��5q�6*   $��Right=�65   $��28��6M   $��28��6X   
������ 
������  �� IsCancel . .��5!      �� $ ��../Assets/Icons/cancel.png��6   6@   5"      5#      5$      5%      