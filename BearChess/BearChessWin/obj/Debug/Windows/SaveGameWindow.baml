   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   a5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   <    6www.SoLaNoSoft.com.BearChessWin.Windows.SaveGameWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   SaveGame � Title# Z 5	   	   $	��350��61   $	��360��6A   $	��300��6N   $	��300��6Z   .��5
      ]� ��.  .+�5   
   � $��5q�6   $	��Top=�6    � ColumnDefinitions 5      �� 5      $
��Auto �6#   62   �� 5      $��* �6#   6/    � RowDefinitions 5      �� 5      $
��Auto �6    60   �� 5      $
��Auto �6    60   �� 5      $
��Auto �6    60   �� 5      $
��Auto �6    60   �� 5      $
��Auto �6    60   �� 5      $	��110 �6    6/   .+�5      �� $��5q�6   $��Center=�6#   $
��Left=�6>   $��0��6Z   $��0��6g      White#��Z 6w   �� 5      -   5        $��textBoxWhite��5   q   $	��210��6   $��5q�6"   $��Center=�6-   $
��Left=�6H   $��0��6d   $��1��6�   6�   �� 5      $��5q�6   $��Center=�6$   $
��Left=�6?   $��1��6[   $��0��6h      Black#��Z 6x   6�   �� 5      -   5        $��textBoxBlack��5   r   $	��210��6   $��5q�6#   $��Center=�6.   $
��Left=�6I   $��1��6e   $��1��6�   6�   �� 5      $��5q�6   $��Center=�6#   $
��Left=�6>   $��2��6Z   $��0��6g      Result#��Z 6w   6�   �� 5      $��5q�6   $��Center=�6%   $
��Left=�6@   $��2��6\   $��1��6i   	 �� Orientation$	 
Horizontal=�6y   .+�5      	� -   5        $��radioButton10��5      $��0,0,5,0q�63   $��Center=�6D   
 	� 	GroupName$
 Result��6f   .��5       �� $	��1-0��6    6-   5!      	� 5"      -   5        $��radioButton01��5"      $��15,0,5,0q�65   $��Center=�6H   $
 Result��6k   .��5#      �� $	��0-1��6    6-   5$      	� 5%      -   5        $��radioButton11��5%      $��15,0,5,0q�63   $��Center=�6F   $
 Result��6i   .��5&      �� $��½-½��6    6-   5'      	� 5(      -   5        $��radioButtonStar��5(      $��15,0,5,0q�65   $��Center=�6H   $
 Result��6k    d� 	IsChecked$
 TrueY�6~   .��5)      �� $��*��6    6+   5*      5+      �� 5,      $��5q�6   $��Center=�6#   $
��Left=�6>   $��3��6Z   $��0��6g      Event#��Z 6w   6�   �� 5-      -   5        $��textBoxEvent��5-   q   $	��210��6   $��5q�6"   $��Center=�6-   $
��Left=�6H   $��3��6d   $��1��6�   6�   �� 5.      $��5q�6   $��Center=�6#   $
��Left=�6>   $��4��6Z   $��0��6g      Date#��Z 6w   6�   �� 5/      -   5        $��textBoxDate��5/   q   $	��210��6   $��5q�6"   $��Center=�6-   $
��Left=�6H   $��4��6d   $��1��6�   6�   �� 50      $��0��6   $��2��6+   $��5��6@   .��51      �� -	   5        $��textBlockMoves��51      $
��Wrap=�62   $��5q�6G   6U   52      54      ]� 55   
   $��5q�6   $��Bottom=�6   ��.  .+�56      �� -
   5        $��buttonOk��56      $��5,5,5,5q�6%   $
��Left=�66   
������ 
������  �� 	IsDefault . .��57      �� $'��!../Assets/Icons/accept_button.png��6   $��32��6C   $��32��6N   6\   58      �� 59      -   5        $��buttonReplace��59      $��5,5,5,5q�6*   $
��Left=�6;   
������ 
������    ReplaceGame � ToolTip# Z 6�   .��5:      �� $&�� ../Assets/Icons/file_save_as.png��6   $��32��6B   $��32��6M   6[   5;      �� 5<      -   5        $��buttonCancel��5<      $��5,5,5,5q�6*   $��Right=�6;   
������ 
������  �� IsCancel . .��5=      �� $ ��../Assets/Icons/cancel.png��6   $��32��6<   $��32��6G   6U   5>      5?      5@      5A      