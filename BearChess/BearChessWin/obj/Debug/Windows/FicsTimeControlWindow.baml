   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   ;    5www.SoLaNoSoft.com.BearChessWin.FicsTimeControlWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   -   5          � WindowStyle$  
ToolWindow=�5   L    � SizeToContent$ Height=�5       � 
ResizeMode$ NoResize=�61    � Title$ Define time control��5   	   $	��220��6%   $	��600��62   .��5	      �  � RowDefinitions 5
   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��2q�6   $��0��6   
�������$��1q�6<   ._�5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5       5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6(   $��5q�65   $��Center=�6@   $	��190��6[   .��6g   Time per game with increment:6�   �� 5      $��1��6   $��0��6-   $��5q�6:    �� Orientation$ 
Horizontal=�6E   .+�5      >   8www.SoLaNoSoft.com.BearChessWin.NumericUpDownUserControl  -   5          Name$- 'numericUpDownUserControlTimePerGameWith��5   <   $��40��5   <      MinValue$ 5��5   <   	   MaxValue$		 999��6I   
   Value$
 1��6X   �� 5       $��Center=�6   .��67   min.6=   �� 5!      $��Center=�6   .��67   plus6=     5"      -   5        $2 ,numericUpDownUserControlTimePerGameIncrement��5"   <   $��40��5#   <   $ 0��5$   <   $	 60��6I   $
 5��6W   �� 5%      $��Center=�6   .��67   sec. per move6F   5&      �� 5'      $��0��6   $��1��6(   $��5q�65   .��6@   Your color:6M   �� 5(      $��1��6   $��1��6-   $��5q�6:   $ 
Horizontal=�6E   .+�5)      	� -   5        $��radioButtonNoColor��5)   ?    	� 	GroupName$ color��6"   $��5q�64    d� 	IsChecked$
 TrueY�6Y   .��6j   	no matter6u   	� 5*      -   5        $��radioButtonWhite��5*   ?   $ color��6"   $��5q�64   .��6W   white6^   	� 5+      -   5        $��radioButtonBlack��5+   ?   $ color��6"   $��5q�64   .��6W   black6^   5,      �� 5-      $��0��6   $��2��6(   $��5q�65   .��6@   Type of match:6P   �� 5.      $��1��6   $��2��6-   $��5q�6:   $ 
Horizontal=�6E   .+�5/      	� -   5        $��radioButtonRated��5/   ?   $ rated��6"   $��5q�64   $
 TrueY�6W   .��6h   rated6o   	� 50      -   5        $��radioButtonUnrated��50   F   $ rated��6"   $��30,5,5,5q�64   .��6`   	unrated6i   51      52      53      ]� 54   
   $��0��6   $��1��6$   ��.  $��5q�6G   .+�55      �� -	   5        $��buttonOk��55      $��15,5,5,5q�6>   $
��Left=�6P   
������ 
������ .��57      �� $'��!../Assets/Icons/accept_button.png��6   $��32��6C   $��32��6N   58      �� 59      -
   5        $��buttonCancel��59      $��5,5,20,5q�6F   $��Right=�6X   
������ 
������ .��5;      �� $ ��../Assets/Icons/cancel.png��6   $��32��6<   $��32��6G   5<      5=      5>      5?      