   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   L    Fwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigureCertaboWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources    ConfigureCertabo � Title# Z  5
   	   $	��380��69   $	��390��6F    � Icon$# Assets/Icons/Certabo_icon.ico��6R   .��5      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $��75 �6'   �� 5      $	��120 �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      Current#��Z 6D   $��DemiBold"�6j   $
��Left=�5      $��Center=�62   �� 5      -   5        $��textBlockCurrentPort��5      $��1��68   $��0��6H   $��5q�6U   $
��COM4��6`   $��Center=�5      $��Center=�64   �� 5      $��0��6   $��1��6,   $��5q�69      ComPorts#��Z 6D   $��DemiBold"�6k   $
��Left=�5      $��Center=�62   �� 5       -   5        $��comboBoxComPorts��5    ,   $��5,5,5,5q�6   $��1��6D   $��1��6T   $��Center=�5!      $	��100��63   $��Center=�5"      $��Center=�6:   $��Center=�5#      $��32��68   �� 5$      -   5        $��buttonCheck��5$      $��2��6,   $��1��6<   $��10,5,5,5q�6I   $	��100��6[   $��Right=�5%   0   $��Center=�6L      CheckSelectedCOMPortTip � ToolTip# Z 6g   $y�Visible=�5&      .��5'      �� 	 �� Orientation$	 
Horizontal=�6!   .+�5(      �� -   5        $��textBlockCheck��5(   $   $��Center=�6:   
   Check#��Z
 6U   �� 5)      $��Assets/Icons/connect.png��6    $��32��6B   5*      5+      �� 5,      -   5        $��textBlockInformation��5,      $��5,5,5,5q�68   $��0��6I   $��2��6Y   $y�	Collapsed=�5-      $
��Wrap=�6.   
���\\��   NoCOMPortDetected#��Z 5.   .   $��3��5/      $	��Top=�6+   $��DemiBold"�6C   51      52      �� 53   
   $��5q�6   
�������$��1q�6/   ._�54      �  55      �� 56      $��85 �6'   �� 57      $	��120 �6'   �� 58      $��* �6'    5:      �� 5;      $
��Auto �6$   .+�5@      �� $��0��6   $��0��6,   $��5q�69      Calibration#��Z 6D   $��DemiBold"�6n   $
��Left=�5A      $��Center=�62   �� 5B      -   5        $��textBlockCalibrate��5B      $��1��66   $��0��6F   $��5q�6S      IsNotCalibrated#��Z 6^   $��Center=�5C      $��Center=�5D      �� 5E      -   5        $��buttonCalibrate��5F      $��2��5E      $��0��6)   $��10,5,5,5q�66   $��Right=�6H   $	��100��6d   .��5G      �� $	 
Horizontal=�6!   .+�5H      �� $��Center=�6$      	Calibrate#��Z 6?   �� 5I      $��2,0,0,0q�6    $&�� Assets/Icons/chessboard_base.png��61   $��32��6[   5J      5K      5M      5N      �� 5O   
   -   5        $��borderMoveLine��5O      $y�Visible=�6'   $��5q�6<   
�������$��1q�6Z   ._�5P      �� .+�5Q      �� -	   5        $��checkBoxMoveLine��5Q   &   $��5q�6      ActivateAllLEDsAlongMove#��Z 6>   6z   �� 5R      -
   5        $��checkBoxOwnMoves��5R   &   $��5q�6      SelectionIsExclusive# Z 6>      ShowOwnMoves#��Z 6�   6�   �� 5S      -   5        $��checkBoxPossibleMoves��5S   &   $��5q�6   '    SelectionIsExclusiveShowOwnMoves# Z 6C   !   ShowAllMovesSelectedFigure#��Z 6�   6  �� 5T      -   5        $��checkBoxBestMove��5T   &   $��5q�6   # Z 6>   !   ShowBestMoveSelectedFigure#��Z 6�   6  5U      5V      �� 5W   
   -   5        $��borderChesstimation��5W      $y�Visible=�6,   $��5q�6A   
�������$��1q�6_   {�.  ._�5X      �� $��5q�6      UsingChesstimation#��Z 6#   6V   5Y      � 5Z   
    5[      �� 5\      $��70 �6#   �� 5]      $	��120 �6#   �� 5^      $��* �6#    5a      �� 5b      $
��Auto �6    .+�5f      �� -   5        $��buttonOk��5f   X   $��0��6   $��0��6%   $��5q�62   $
��Left=�6=   $��32��6h   $��32��5g      
������ 
������ .��5i      �� $$��Assets/Icons/accept_button.png��6   5j      �� 5k      -   5        $��buttonCancel��5k   Y   $��2��6   $��0��6%   $��5q�62   $��Right=�6=   $��32��6m   $��32��5l      
������ 
������ .��5n      �� $��Assets/Icons/cancel.png��6   5o      5p      5r      5s      