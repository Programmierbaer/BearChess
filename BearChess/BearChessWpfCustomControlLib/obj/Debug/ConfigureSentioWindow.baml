   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   K    Ewww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigureSentioWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6A    � SizeToContent$ Height=�6W   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   ConfigureSentio � Title# Z 5	      $	��280��63   $	��390��6@    � Icon$* $Assets/Icons/tabutronic_logo_def.png��6L   .��5
      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $��75 �6'   �� 5      $	��120 �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      Current#��Z 6D   $��DemiBold"�6j   $
��Left=�5      $��Center=�64   �� 5      -   5        $��textBlockCurrentPort��5      $��1��68   $��0��6H   $��5q�6U   $
��COM4��6`   $��Center=�5      $��Center=�66   �� 5      $��0��6   $��1��6,   $��5q�69   	   ComPorts#��Z	 6D   $��DemiBold"�6k   $
��Left=�5      $��Center=�64   �� 5      -   5        $��comboBoxComPorts��5   ,   $��5,5,5,5q�6   $��1��6D   $��1��6T   $��Center=�5      $	��100��65   $��Center=�5       $��Center=�6<   $��Center=�5!      $��32��6:   �� 5"      -   5        $��buttonCheck��5"      $��2��6,   $��1��6<   $��10,5,5,5q�6I   $	��100��6[   $��Right=�5#   2   $��Center=�6N   $y�Visible=�5$      .��5%      �� 
 �� Orientation$
 
Horizontal=�6!   .+�5&      �� -   5        $��textBlockCheck��5&   $   $��Center=�6:      Check#��Z 6U   6{   �� 5'      $��Assets/Icons/connect.png��6    $��32��6B   5(      5)      �� 5*      -   5        $��textBlockInformation��5*      $��5,5,5,5q�68   $��0��6I   $��2��6Y   $y�	Collapsed=�5+      $
��Wrap=�60   
���\\��   NoCOMPortDetected#��Z 5,   0   $��3��5-      $	��Top=�6-   $��DemiBold"�6E   5.      5/      �� 51   
   -   5        $��borderMoveLine��51      $y�Visible=�6'   $��5q�6<   
�������$��1q�6Z   ._�52      �� .+�53      �� -   5        $��checkBoxMoveLine��53   &   $��5q�6      ActivateAllLEDsAlongMove#��Z 6>   6z   �� 54      -   5        $��checkBoxOwnMoves��54   &   $��5q�6      SelectionIsExclusive � ToolTip# Z 6>      ShowOwnMoves#��Z 6�   6�   �� 55      -	   5        $��checkBoxPossibleMoves��55   &   $��5q�6   '    SelectionIsExclusiveShowOwnMoves# Z 6C   !   ShowAllMovesSelectedFigure#��Z 6�   6  �� 56      -
   5        $��checkBoxBestMove��56   &   $��5q�6   # Z 6>   !   ShowBestMoveSelectedFigure#��Z 6�   6  57      58      � 59   
    5:      �� 5;      $��70 �6#   �� 5<      $	��120 �6#   �� 5=      $��* �6#    5@      �� 5A      $
��Auto �6    .+�5E      �� -   5        $��buttonOk��5E   X   $��0��6   $��0��6%   $��5q�62   $
��Left=�6=   $��32��6h   $��32��5F      
������ 
������ .��5H      �� $$��Assets/Icons/accept_button.png��6   5I      �� 5J      -   5        $��buttonCancel��5J   Y   $��2��6   $��0��6%   $��5q�62   $��Right=�6=   $��32��6m   $��32��5K      
������ 
������ .��5M      �� $��Assets/Icons/cancel.png��6   5N      5O      5Q      5R      