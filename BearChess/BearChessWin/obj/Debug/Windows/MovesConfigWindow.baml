   M S B A M L   `   `   `  ����  BearChessWin7    1www.SoLaNoSoft.com.BearChessWin.MovesConfigWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � 
ResizeMode$  NoResize=�5       � SizeToContent$ WidthAndHeight=�60    � WindowStartupLocation$ CenterOwner=�5   	    � Title$ Notation��5   	   $	��230��6   $	��200��6'   .��5	      � $��5,10,5,5q�6    � ColumnDefinitions 5
   
   �� 5      $	��100 �6   6-   �� 5      $��80 �6   6,    � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   	� -   5        $��radioButtonFigurine��5   U    	� 	GroupName$ 
figureType��6   $��5q�6-   $��0��68   $��0��6E    d� 	IsChecked$
 TrueY�6p   .��6�   
Figurine6�   	� 5   
   -   5        $��radioButtonLetter��5   U   $ 
figureType��6   $��5q�6-   $��1��68   $��0��6E   .��6�   KQRNB6�   	� 5   
   -   5        $��radioButtonLong��5   S   $ moveType��6   $��5q�6+   $��0��66   $��1��6C   $
 TrueY�6j   .��6�   
1. e2-e46�   	� 5   
   -   5        $��radioButtonShort��5   S   $ moveType��6   $��5q�6+   $��1��66   $��1��6C   .��6�   1. e46�   �� 5   
   $��3��6   $��0��6#   .+�5   
   	� -   5        $��radioButtonGB��5   G   $��3��6   $��0��6#   $ country��63   $
 TrueY�6\   $
��10,5q�6m   .��5      �� $��16��6   $��16��6#   $,��&../Assets/Icons/flag_great_britain.png��6/   6a   5      	� 5   
   -   5        $��radioButtonDE��5   U   $��4��6   $��0��6#   $ country��63   $
��10,5q�6G   .��5      �� $��16��6   $��16��6#   $&�� ../Assets/Icons/flag_germany.png��6/   6[   5       	� 5!      -   5        $��radioButtonSP��5!   Y   $��4��6   $��0��6'   $ country��67   $
��10,5q�6K   .��5"      �� $��16��6   $��16��6#   $$��../Assets/Icons/flag_spain.png��6/   6Y   5#      	� 5$      -   5        $��radioButtonPo��5$   Y   $��4��6   $��0��6'   $ country��67   $
��10,5q�6K   .��5%      �� $��16��6   $��16��6#   $%��../Assets/Icons/flag_poland.png��6/   6Z   5&      5'      �� 5(   
   $��3��6   $��1��6"   .+�5)      	� -	   5        $��radioButtonFR��5)   Y   $��4��6   $��0��6'   $ country��67   $
��10,5q�6K   .��5*      �� $��16��6   $��16��6#   $%��../Assets/Icons/flag_france.png��6/   6Z   5+      	� 5,      -
   5        $��radioButtonIT��5,   Y   $��4��6   $��0��6'   $ country��67   $
��10,5q�6K   .��5-      �� $��16��6   $��16��6#   $$��../Assets/Icons/flag_italy.png��6/   6Y   5.      	� 5/      -   5        $��radioButtonDA��5/   Y   $��4��6   $��0��6'   $ country��67   $
��10,5q�6K   .��50      �� $��16��6   $��16��6#   $&�� ../Assets/Icons/flag_denmark.png��6/   6[   51      	� 52      -   5        $��radioButtonIceland��52   Y   $��4��6   $��0��6'   $ country��67   $
��10,5q�6K   .��53      �� $��16��6   $��16��6#   $&�� ../Assets/Icons/flag_iceland.png��6/   6[   54      55      �� 57   
   -   5        $��buttonOk��57      $��5q�6!   $
��Left=�6,   $��Bottom=�6H   $��0��6d   
������ 
������ $��5��58      $��32��68   $��32��6C   .��59      �� $'��!../Assets/Icons/accept_button.png��6   6A   5:      �� 5;   
   -   5        $��buttonCancel��5;      $��5q�6%   $��Right=�60   $��Bottom=�6L   $��1��6h   
������ 
������ $��5��5<      $��32��6;   $��32��6F   .��5=      �� $ ��../Assets/Icons/cancel.png��6   6;   5>      5?      5@      