   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   N    Hwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigureSquareOffWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	     � SizeToContent$  Height=�5       � 
ResizeMode$ NoResize=�62    � WindowStartupLocation$ CenterOwner=�6H   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   ConfigureSquareOffPro � Title# Z 5	   	   $	��440��6>   $	��490��6K   .��5
      �� .+�5   
   �� -   5        $��borderScans��5      $��5q�6$   
�������$��1q�6B   ._�5      �� $��0,5,0,5q�6   .+�5      ]� ��.  .+�5      �� $
��Left=�6    $��5q�66      Scans#��Z 6A   $��DemiBold"�6e   $��70��6{   $
��Left=�5      $��Center=�68      ScanIntervalInMS � ToolTip# Z 5      �� 5      -   5        $��buttonDelScan��5   _   $
��Left=�6   $��0,0,0,0q�63   $
��Left=�6D   $��32��5      $��32��5      
������ 
������    DecrementScanTime# Z 5      .��5      �� $"��Assets/Icons/time_delete.png��6    5      �� 5      -   5        $��sliderScanTime��5   3   $
��Left=�6   $��15,5,5,5q�6I   	 � Minimum$		 150Q�6[   
 � Maximum$	
 500Q�6i    �� TickPlacement$
 Both=�6w   $	��200��5       � Value$	 500Q�6&   5      �� 5      -   5        $��buttonPlusScan��5   _   $
��Left=�6   $��5,0,0,0q�63   $
��Left=�6D   $��32��5      $��32��5      
������ 
������    IncrementScanTime# Z 5!      .��5"      �� $��Assets/Icons/time_add.png��6    5#      �� 5$      -   5        $��textBlockScansPerSec��5$       $
��Left=�6<   $
��Left=�6R   $��5,0,0,0q�5%      $��Center=�5&      $��every 500 ms��68   5'      �� 5(       �� Orientation$ 
Horizontal=�6   .+�5)      �� -   5        $��buttonScanDefault��5)   0   $��	10,5,10,5q�6   
������ $ Reset to default��5*      
������ $��32��5+   4   $��32��6?   .��5,      �� $��Assets/Icons/time.png��6    5-      �� 5.      $��Center=�6       ResetDefault250ms#��Z 6;   6m   5/      50      51      �� 52   
   -   5        $��borderDelay��52      $��5q�6$   
�������$��1q�6B   ._�53   
   �� $��0,5,0,5q�6   .+�54      �� $
��Left=�6   $��5q�6.      DelaySendLED#��Z 69   $��DemiBold"�6d   $
��Left=�55      $��Center=�67   ]� 56      ��.  .+�57      �� -   5        $��buttonDelay��57   [   $
��Left=�6   $��0,0,0,0q�6/   $
��Left=�6@   $��32��58      $��32��59      
������ 
������    DecrementDelay# Z 5;      .��5<      �� $"��Assets/Icons/time_delete.png��6   5=      �� 5?      -	   5        $��sliderDelayTime��5?   /   $
��Left=�6   $��15,5,5,5q�6F   $	 20Q�6X   $	
 250Q�6e   $
 Both=�6s   $	��200��5@      $	 100Q�6"   5A      �� 5C      -
   5        $��buttonPlusDelay��5C   [   $
��Left=�6   $��5,0,0,0q�6/   $
��Left=�6@   $��32��5D      $��32��5E      
������ 
������    IncrementDelay# Z 5G      .��5H      �� $��Assets/Icons/time_add.png��6   5I      �� 5J      -   5        $��textBlockDelayPerSec��5J      $
��Left=�68   $
��Left=�6N   $��5,0,0,0q�5K      $��Center=�5L      $��every 3500 ms��64   5M      �� 5N      $ 
Horizontal=�6   .+�5O      �� -   5        $��buttonDelayDefault��5O   ,   $��	10,5,10,5q�6   
������ $ Reset to default��5P      
������ $��32��5Q   0   $��32��6;   .��5R      �� $��Assets/Icons/time.png��6   5S      �� 5T      $��Center=�6       ResetDefault100ms#��Z 6;   6m   5U      5V      5W      �� 5X   
   $y�Visible=�6   $��5q�6'   
�������$��1q�6E   ._�5Y      �� .+�5Z      �� -   5        $��checkBoxMoveLine��5Z   &   $��5q�6      ActivateAllLEDsAlongMove#��Z 6>   6z   	� 5[      -   5        $��checkBoxDefault��5[   9    	� 	GroupName$	 Hos��6   $��5q�6.      NoAdditionalInfo#��Z 6R   6�   	� 5\      -   5        $��checkBoxPossibleMoves��5\   9   $	 Hos��6   $��5q�6.   !   ShowAllMovesSelectedFigure#��Z 6X   6�   	� 5]      -   5        $��checkBoxBestMove��5]   9   $	 Hos��6   $��5q�6.   !   ShowBestMoveSelectedFigure#��Z 6R   6�   5^      5_      ]� 5`   
   $��0,0,0,5q�6   .+�5a      �� -   5        $��buttonOk��5a   X   $
��Left=�6   $��10,5,5,5q�6+   $
��Left=�6=   $��32��6h   $��32��5b      
������ 
������ $ Ok��5c   Z   .��5d      �� $$��Assets/Icons/accept_button.png��6   5e      �� 5g      -   5        $��buttonCancel��5g   Z   $��Right=�6   $��5,5,10,5q�6,   $��Right=�6>   $��32��6n   $��32��5h      
������ 
������ $ Cancel��5j      .��5k      �� $��Assets/Icons/cancel.png��6   5l      5m      5n      5o      