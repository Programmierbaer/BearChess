   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   Q-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   9    3www.SoLaNoSoft.com.BearChessWin.DuelIncrementWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5	       � 
ResizeMode$ NoResize=�6>   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   IncrementCycles � Title# Z 5
   	   $	��150��68   $	��300��6E   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� $��0��6   $��0��6&    �� Orientation$ 
Horizontal=�63   $
��Left=�6L   $��5q�6g   .+�5      �� $	��110��6   $��Center=�6       NumberOfGames#��Z 6;   6l   >   8www.SoLaNoSoft.com.BearChessWin.NumericUpDownUserControl  5      -   5          Name$+ %numericUpDownUserControlNumberOfGames��5   4   $��40��6c      MinValue$ 1��5   -   	   MaxValue$		 999��6:   
   Value$
 1��6I   5      ]� 5   
   $��0��6   $��1��6$   ��.  $��5q�6G   .+�5      �� -   5        $��buttonOk��5      $��15,5,5,5q�6>   $
��Left=�6P   
������ 
������ .��5      �� $'��!../Assets/Icons/accept_button.png��6   $��32��6C   $��32��6N   5      �� 5      -   5        $��buttonCancel��5      $��5,5,20,5q�6F   $��Right=�6X   
������ 
������ .��5      �� $ ��../Assets/Icons/cancel.png��6   $��32��6<   $��32��6G   5      5      5       5!      