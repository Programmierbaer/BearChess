   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   ;    5www.SoLaNoSoft.com.BearChessWin.ChessBoardSetupWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	     � 
ResizeMode$  NoResize=�5   P    � WindowStartupLocation$ CenterOwner=�5       � SizeToContent$ WidthAndHeight=�6>   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   SetupBoardAndPieces � Title# Z 5   	   $	��250��6<   $	��500��6I   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� -   5        $��groupBoxBoards��5      $��5q�6)   $��0��64   $��0��6D      Board#��Z 6Q   .��5      �  5      �� 5      $
��Auto �6'   �� 5      $
��Auto �6'    5      �� 5      �� 5      �� 5      �� 5      �� 5      .+�5      �� -   5        $��comboBoxBoards��5      $	��200��6f   $��25��5       $��Center=�6'   $	��Top=�6I   $��2��6a   $��5q�5!      $��0��6&   $��0��66   �� 5$      -   5        $��buttonOpenBoard��5$      $��5q�6P   $��0��6[   $��1��6k   $
��Left=�5%      
������ 
������ .��5&      �� $��5q�6   $&�� ../Assets/Icons/file_manager.png��6'   $��32��6Q   $��32��6\   5'      �� 5(      -   5        $��buttonDeleteBoard��5(      $��Right=�62   $��5q�6N   $��1��6Y   $��1��6i   
������ 
������ .��5*      �� $��5q�6   $��../Assets/Icons/bin.png��6'   $��32��6H   $��32��6S   5+      5-      5.      �� 5/   
   -   5        $��groupBoxPieces��5/      $��5q�6)   $��1��64   $��0��6D      Pieces#��Z 6Q   .��50      �  51      �� 52      $
��Auto �6'   �� 53       55      �� 56      �� 57      �� 58      .+�5:      �� -   5        $��comboBoxPieces��5:      $	��200��61   $��25��6=   $	��Top=�6I   $��5q�6a   $��0��5;      $��2��6+   $��0��6?   6L   �� 5=      -   5        $��buttonOpenPieces��5=      $��5q�6R   $��0��6]   	 J� 	AllowDrop	 . $��1��5>      $
��Left=�6&   
������ 
������ .��5?      �� $��5q�6   $&�� ../Assets/Icons/file_manager.png��6'   $��32��6Q   $��32��6\   5@      �� 5A      -   5        $��buttonDeletePieces��5A      $��Right=�63   $��5q�6O   $��1��6Z   $��1��6j   
������ 
������ .��5C      �� $��5q�6   $��../Assets/Icons/bin.png��6'   $��32��6H   $��32��6S   5D      5F      5G      �� 5H   
   -	   5        $��5q�5H      $
��Left=�6   $��0��67   
������ 
������ 	
   Ok � ToolTip# Z
 5I   +   $��1��5J      .��5K      �� $'��!../Assets/Icons/accept_button.png��6   $��32��6?   $��32��6J   5L      �� 5M   
   -
   5        $��buttonCancel��5M      $��5q�6%   $��Right=�60   $��1��6L   
������ 
������    Cancel# Z 5N   +   $��1��5O      .��5P      �� $ ��../Assets/Icons/cancel.png��6   $��32��68   $��32��6C   5Q      5R      5S      