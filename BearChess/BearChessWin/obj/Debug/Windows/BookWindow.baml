   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   0    *www.SoLaNoSoft.com.BearChessWin.BookWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          � Icon$#  ../Assets/Icons/book_open.png��5   P   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   Book � Title# Z 5   	   $	��290��6-   $	��290��6=   $	��410��6J   .��5	      �  � RowDefinitions 5
   
   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $��dataGridMoves��5   ,   $	��190��5      $	��Top=�6#   $��0q�6;   $��14(�6O   
������ "   VerticalScrollBarVisibility$
 Auto=�5      $��0��66   $��5q�6C      AutoGenerateColumns .     
IsReadOnly . ��5   A   ��    RowStyle 5      �� 5       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6   	 �� Triggers	 5      �� 5      
 �� Binding
 6&   �� *Recommendations+ �� Value 06J   .a�5      ��  �� Property � �� �� Value �#ffffff5      5      �� 5      
 6&   �� *Recommendations+ 1006J   .a�5      ��  � �� �#ffffff5      5      �� 5      
 6&   �� *Recommendations+ 646J   .a�5      ��  � �� �#ff85665      5       �� 5!      
 6&   �� *Recommendations+ 1286J   .a�5"      ��  � �� �#c2f0c25#      5$      5&         Columns 5(      0  *System.Windows.Controls.DataGridTextColumn  5)      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6B   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *
MoveText+   Header 5*      �� 5+         Move#��Z 6$   6I      ElementStyle 5-      �� 5.      " ��6    .�5/      ��  � ��$ Right=�6D   �� 50       � ��$ Consolas)�6;   �� 51       � ��$
 Bold"�6;   52      54        5A      $
 Auto 6@    6%   �� *Weight+ 5B      �� 5C         Weight#��Z 6$   6K   5E        5F      $
 Auto 6C    6%   �� *	NoOfGames+ 5G      �� 5H         Games#��Z 6$   6J   5J        5K      $
 Auto 6B    6%   �� *
NoOfWins+ 5L      �� 5M         Wins#��Z 6$   6I   5O        5P      $
 Auto 6B    6%   �� *
NoOfLoss+ 5Q      �� 5R         Losts#��Z 6$   6J   5T        5U      $
 Auto 6C    6%   �� *	NoOfDraws+ 5V      �� 5W         Draws#��Z 6$   6J   5Y      5^      �� 5_   
   -   5        $��buttonCancel��5_   )   $��5q�6   $��1��6   $��Right=�6=   
������ 
������ .��5a      �� $"��../Assets/Icons/door_out.png��6   $��32��6:   $��32��6E   6S   5b      5c      5d      