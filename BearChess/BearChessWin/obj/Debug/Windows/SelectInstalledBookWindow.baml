   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   Q-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   ?    9www.SoLaNoSoft.com.BearChessWin.SelectInstalledBookWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   -   5          � Icon$#  ../Assets/Icons/book_open.png��5   	    � WindowStartupLocation$ CenterOwner=�5	      :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   LoadAndManageBooks � Title# Z 5
   	   $	��250��6;   $	��600��6H    � 	Resources 5      <   6www.SoLaNoSoft.com.BearChessWin.BoolToContentConverter   BoolToContentConverter��%   &	          5   
   6.   5      .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $��dataGridBook��5      $��0��6?   $��5q�6L      AutoGenerateColumns .  "   VerticalScrollBarVisibility$
 Auto=�5   F      SelectionMode$ Single=�5      	   
IsReadOnly	 . ��5   '   ��  5      
 �� Color��%�   (��       5   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� ,     5   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5      $
 CornflowerBlue��5   "   �� 5      $
 White��6i   5       � ContextMenu 5      �� 5      .@�5      m� -   5           SetBookAsDefault#��Z 5       m� Icon 5       �� 5!      $"��../Assets/Icons/bookmark.png��6$   5#      5%         Columns 5'      0  *System.Windows.Controls.DataGridTextColumn  5(      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6>   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *Name+   Header 5)      �� 5*         Name#��Z 6$   5,        5-      $	 100 6B    6%   �� *
FileName+ 5.      �� 5/         FileName#��Z 6$   51        52      $
 Auto 6H    6%   �� *PositionsCount+ 53      �� 54         	Positions#��Z 6$   56        57      $
 Auto 6D    6%   �� *
MovesCount+ 58      �� 59         Moves#��Z 6$   5;        5<      $
 Auto 6D    6%   �� *
GamesCount+ 5=      �� 5>         Games#��Z 6$   5@        5A      $
 Auto 6>    6%   �� *Size+ 5B      �� 5C         	SizeBytes#��Z 6$   5E        5F      $
 Auto 5G   j    6   �� *IsDefaultBook+ �� 	Converter# [   5H      �� 5I         DefaultBook#��Z 6$   5K      5M      ]� 5N   
   $��5q�6   $��1��6   ��.  .+�5O      �� -   5        $��buttonOk��5O   6   $��5q�6   $
��Left=�6    
������ 
������    LoadBook � ToolTip# Z 5P   /   .��5Q      �� $#��../Assets/Icons/book_open.png��6   $��32��6?   $��32��6J   5R      �� 5T      -   5        $��buttonCancel��5T   >   $��25,5,5,5q�6   $��Right=�6'   
������ 
������    Exit# Z 5U   H   .��5V      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   5W      �� 5X      -   5        $��buttonDelete��5X   >   $��5,5,20,5q�6   $��Right=�6'   
������ 
������    UninstallBook# Z 5Y   H   .��5Z      �� $��../Assets/Icons/bin.png��6   $��32��69   $��32��6D   5[      �� 5\      -   5        $��buttonInstall��5\   >   $��5,5,20,5q�6   $��Right=�6'   
������ 
������    InstallBook# Z 5]   H   .��5^      �� $"��../Assets/Icons/book_add.png��6   $��32��6>   $��32��6I   5_      �� 5`      -   5        $��buttonDefault��5`   >   $��5,5,20,5q�6   $��Right=�6'   
������ 
������ # Z 5b      .��5c      �� $"��../Assets/Icons/bookmark.png��6   $��32��6>   $��32��6I   5d      5e      5f      5h      