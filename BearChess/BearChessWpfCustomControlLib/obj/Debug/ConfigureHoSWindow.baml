   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   H    Bwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigureHoSWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ Height=�6T   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   ConfigureHoS � Title# Z 5	   	   $	��380��65   $	��480��6B    � Icon$ Assets/Icons/hos.png��6N   .��5
      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $	��120 �6'   �� 5      $	��120 �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      CurrentCOMPort#��Z 6D   $��DemiBold"�6q   $
��Left=�5      $��Center=�61   �� 5      -   5        $��comboBoxComPorts��5   ,   $��5,5,5,5q�6   $��1��6D   $��0��6T   $��Center=�5      $	��100��69   $��Center=�5      $��Center=�6@   $��Center=�5      $��32��6>   �� 5      -   5        $��textBlockInformation��5      $��5,5,5,5q�68   $��0��6I   $��2��6Y   $y�	Collapsed=�5      $
��Wrap=�6-   
���\\��	   NoCOMPortDetected#��Z	 5    -   $��3��5!      $	��Top=�6*   $��DemiBold"�6B   5"      5#      �� 5$   
   -   5        $��borderDelay��5$      $��5q�6$   
�������$��1q�6B   ._�5%      �� $��0,5,0,5q�6   .+�5&      �� $
��Left=�6   $��5q�62   
   DelaySendLEDbtle#��Z
 6=   $��DemiBold"�6l   $
��Left=�5'      $��Center=�67   ]� 5(      ��.  .+�5)      �� -   5        $��buttonDelScan��5)   _   $
��Left=�6   $��0,0,0,0q�63   $
��Left=�6D   $��32��5*      $��32��5+      
������ 
������    DecrementDelay � ToolTip# Z 5-      .��5.      �� $"��Assets/Icons/time_delete.png��6    5/      �� 51      -   5        $��sliderScanTime��51   3   $
��Left=�6   $��15,5,5,5q�6I    � Minimum$
 1000Q�6[    � Maximum$
 3500Q�6j    �� TickPlacement$
 Both=�6y   $	��200��52       � Value$
 2500Q�6"   53      �� 55      -   5        $��buttonPlusScan��55   _   $
��Left=�6   $��5,0,0,0q�63   $
��Left=�6D   $��32��56      $��32��57      
������ 
������    IncrementDelay# Z 59      .��5:      �� $��Assets/Icons/time_add.png��6    5;      �� 5<      -   5        $��textBlockScansPerSec��5<       $
��Left=�6<   $
��Left=�6R   $��5,0,0,0q�5=      $��Center=�5>      $��every 3500 ms��64   5?      �� 5@       �� Orientation$ 
Horizontal=�6   .+�5A      �� -   5        $��buttonScanDefault��5A   0   $��	10,5,10,5q�6   
������ $ Reset to default��5B      
������ $��32��5C   0   $��32��6;   .��5D      �� $��Assets/Icons/time.png��6    5E      �� 5F      $��Center=�6       ResetDefault2500#��Z 6;   6l   5G      5H      5I      �� 5J   
   $y�Visible=�6   $��5q�6'   
�������$��1q�6E   ._�5K      �� .+�5L      	� -	   5        $��checkBoxDefault��5L   9    	� 	GroupName$	 Hos��6   $��5q�6.      NoAdditionalInfo#��Z 6R   6�   	� 5M      -
   5        $��checkBoxOwnMoves��5M   9   $	 Hos��6   $��5q�6.      ShowOwnMoves#��Z 6S   6�   	� 5N      -   5        $��checkBoxPossibleMoves��5N   9   $	 Hos��6   $��5q�6.   !   ShowAllMovesSelectedFigure#��Z 6X   6�   	� 5O      -   5        $��checkBoxBestMove��5O   9   $	 Hos��6   $��5q�6.   !   ShowBestMoveSelectedFigure#��Z 6R   6�   5P      5Q      � 5R   
    5S      �� 5T      $��70 �6#   �� 5U      $	��120 �6#   �� 5V      $��* �6#    5Y      �� 5Z      $
��Auto �6    .+�5^      �� -   5        $��buttonOk��5^   X   $��0��6   $��0��6%   $��5q�62   $
��Left=�6=   $��32��6h   $��32��5_      
������ 
������ .��5a      �� $$��Assets/Icons/accept_button.png��6   5b      �� 5c      -   5        $��buttonCancel��5c   Y   $��2��6   $��0��6%   $��5q�62   $��Right=�6=   $��32��6m   $��32��5d      
������ 
������ .��5f      �� $��Assets/Icons/cancel.png��6   5g      5h      5j      5k      