   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5      k:clr-namespace:www.SoLaNoSoft.com.BearChessWin.UserControls,www.SoLaNoSoft.com.BearChessWin.UserControls  5      5    /www.SoLaNoSoft.com.BearChessWin.BookUserControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5      Llocal:clr-namespace:www.SoLaNoSoft.com.BearChessWin.UserControls     5      
�������.��5
      �   � RowDefinitions  5   
   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   .+�5   
   �� -   5        $��textBlockTitle��5   B   $��DemiBold"�6   $��1q�6*   $��0��65   Book6^   &   System.Windows.Controls.DataGrid  5   
   -   5        $��dataGridMoves��5   +   $	��190��5      $	��Top=�6#   $��0q�6;   $��14(�6O   
������ "   VerticalScrollBarVisibility$
 Auto=�5      $��1��65   $��1q�6B      AutoGenerateColumns .     
IsReadOnly . ��5   @   ��    RowStyle 5      �� 5       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6    �� Triggers 5      �� 5       �� Binding 6&   �� *Recommendations+ �� Value 06J   .a�5      �� 	 �� Property	 � ��
 �� Value
 �#ffffff5      5      �� 5       6&   �� *Recommendations+ 1006J   .a�5      �� 	 � ��
 �#ffffff5      5      �� 5        6&   �� *Recommendations+ 646J   .a�5!      �� 	 � ��
 �#ff85665"      5#      �� 5$       6&   �� *Recommendations+ 1286J   .a�5%      �� 	 � ��
 �#c2f0c25&      5'      5)         Columns 5+      0  *System.Windows.Controls.DataGridTextColumn  5,      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6B   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *
MoveText+   Header 5-      �� 5.      :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   Move#��Z 6$   6I      ElementStyle 50      �� 51      " ��6    .�52      �� 	 � ��$
 Right=�6D   �� 53      	 � ��$
 Consolas)�6;   �� 54      	 � ��$

 Bold"�6;   55      57        5D      $
 Auto 6@    6%   �� *Weight+ 5E      �� 5F         Weight#��Z 6$   6K   5H        5I      $
 Auto 6C    6%   �� *	NoOfGames+ 5J      �� 5K         Games#��Z 6$   6J   5M        5N      $
 Auto 6B    6%   �� *
NoOfWins+ 5O      �� 5P         Wins#��Z 6$   6I   5R        5S      $
 Auto 6B    6%   �� *
NoOfLoss+ 5T      �� 5U         Losts#��Z 6$   6J   5W        5X      $
 Auto 6C    6%   �� *	NoOfDraws+ 5Y      �� 5Z         Draws#��Z 6$   6J   5\      5a      �� 5b   
   -   5        $��buttonCancel��5b   )   $��5q�6   $��2��6   $��Right=�6=   
������ 
������ .��5d      �� $"��../Assets/Icons/door_out.png��6   $��20��6:   $��20��6E   6S   5e      5f      5g      