   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   J    Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigureCernoWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ Height=�6T   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   ConfigureCerno � Title# Z 5	      $	��350��66   $	��390��6C    � Icon$* $Assets/Icons/tabutronic_logo_def.png��6O   .��5
      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $��75 �6'   �� 5      $	��120 �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      Current#��Z 6D   $��DemiBold"�6j   $
��Left=�5      $��Center=�6.   �� 5      -   5        $��textBlockCurrentPort��5      $��1��68   $��0��6H   $��5q�6U   $
��COM4��6`   $��Center=�5      $��Center=�60   �� 5      $��0��6   $��1��6,   $��5q�69   	   ComPorts#��Z	 6D   $��DemiBold"�6k   $
��Left=�5      $��Center=�6.   �� 5      -   5        $��comboBoxComPorts��5   ,   $��5,5,5,5q�6   $��1��6D   $��1��6T   $��Center=�5      $	��100��6/   $��Center=�5       $��Center=�66   $��Center=�5!      $��32��64   �� 5"      -   5        $��buttonCheck��5"      $��2��6,   $��1��6<   $��10,5,5,5q�6I   $	��100��6[   $��Right=�5#   ,   $��Center=�6H   
   CheckSelectedCOMPortTip � ToolTip# Z
 6c   $y�Visible=�5$      .��5%      ��  �� Orientation$ 
Horizontal=�6!   .+�5&      �� -   5        $��textBlockCheck��5&   $   $��Center=�6:      Check#��Z 6U   �� 5'      $��Assets/Icons/connect.png��6    $��32��6B   5(      5)      �� 5*      -   5        $��textBlockInformation��5*      $��5,5,5,5q�68   $��0��6I   $��2��6Y   $y�	Collapsed=�5+      $
��Wrap=�6*   
���\\��   NoCOMPortDetected#��Z 5,   *   $��3��5-      $	��Top=�6'   $��DemiBold"�6?   5/      50      �� 51   
   $��5q�6   
�������$��1q�6/   ._�52      �  53      �� 54      $��85 �6'   �� 55      $	��120 �6'   �� 56      $��* �6'    58      �� 59      $
��Auto �6$   .+�5>      �� $��0��6   $��0��6,   $��5q�69      Calibration#��Z 6D   $��DemiBold"�6n   $
��Left=�5?      $��Center=�6.   �� 5@      -   5        $��textBlockCalibrate��5@      $��1��66   $��0��6F   $��5q�6S      IsNotCalibrated#��Z 6^   $��Center=�5A      $��Center=�5B      �� 5C      -   5        $��buttonCalibrate��5D      $��2��5C      $��0��6)   $��10,5,5,5q�66   $��Right=�6H   $	��100��6d   .��5E      �� $ 
Horizontal=�6!   .+�5F      �� $��Center=�6$      	Calibrate#��Z 6?   �� 5G      $��2,0,0,0q�6    $&�� Assets/Icons/chessboard_base.png��61   $��32��6[   5H      5I      5K      5L      �� 5M   
   -   5        $��borderMoveLine��5M      $y�Visible=�6'   $��5q�6<   
�������$��1q�6Z   ._�5N      �� .+�5O      �� -	   5        $��checkBoxMoveLine��5O   &   $��5q�6      ActivateAllLEDsAlongMove#��Z 6>   6z   �� 5P      -
   5        $��checkBoxOwnMoves��5P   &   $��5q�6      SelectionIsExclusive# Z 6>      ShowOwnMoves#��Z 6�   6�   �� 5Q      -   5        $��checkBoxPossibleMoves��5Q   &   $��5q�6   '    SelectionIsExclusiveShowOwnMoves# Z 6C   !   ShowAllMovesSelectedFigure#��Z 6�   6  �� 5R      -   5        $��checkBoxBestMove��5R   &   $��5q�6   # Z 6>   !   ShowBestMoveSelectedFigure#��Z 6�   6  5S      5T      �� 5U   
   -   5        $��borderChesstimation��5U      $y�Visible=�6,   $��5q�6A   
�������$��1q�6_   {�.  ._�5V      �� $��5q�6      UsingChesstimation#��Z 6#   6V   5W      � 5X   
    5Y      �� 5Z      $��70 �6#   �� 5[      $	��120 �6#   �� 5\      $��* �6#    5_      �� 5`      $
��Auto �6    .+�5d      �� -   5        $��buttonOk��5d   X   $��0��6   $��0��6%   $��5q�62   $
��Left=�6=   $��32��6h   $��32��5e      
������ 
������ .��5g      �� $$��Assets/Icons/accept_button.png��6   5h      �� 5i      -   5        $��buttonCancel��5i   Y   $��2��6   $��0��6%   $��5q�62   $��Right=�6=   $��32��6m   $��32��5j      
������ 
������ .��5l      �� $��Assets/Icons/cancel.png��6   5m      5n      5p      5q      