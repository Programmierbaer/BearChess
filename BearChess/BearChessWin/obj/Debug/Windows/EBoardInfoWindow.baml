   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   Q-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   6    0www.SoLaNoSoft.com.BearChessWin.EBoardInfoWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   -   5          � 
ResizeMode$  NoResize=�5   L    � WindowStartupLocation$ CenterOwner=�5   ?    � Title$" Electronic Board Information��5	   	   $	��550��6.   $	��800��6;   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,    � ColumnDefinitions 5   
   �� 5      $��* �6   6+   �� 5      $
��Auto �6   6.   .+�5   
   C   =www.SoLaNoSoft.com.BearChessWin.GraphicsChessBoardUserControl  -   5          Name$ chessBoardUcGraphics��5   5   $��5,5,5,0q�5   5   $��0��6F   $��1��6V   $
��Left=�6d   $y�Visible=�5   S   �� 5   
   -   5        $��textBlockBoard��5       $��10q�6   $��1��66   $��1��6F   $
��Left=�6T   $	��300��6o   $
��Wrap=�6{   $	��Top=�6�   6�   �� 5   
   -   5        $��buttonOk��5      $��5q�6   $��0��6,   $��2��6<   $
��Left=�6I   
������ 
������ .��5      �� $'��!../Assets/Icons/accept_button.png��6   $��32��6?   5      �� 5   
   -   5        $��buttonReset��5      $��5q�6   $��1��6/   $��2��6?   $
��Left=�6L   
������ 
������  � ToolTip$ Reset board��6�   .��5      �� $'��!../Assets/Icons/Board64 black.ico��6   $��32��6?   5      5      5      