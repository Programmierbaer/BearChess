   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   7    1www.SoLaNoSoft.com.BearChessWin.FicsCommandWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   -   5          � SizeToContent$  Height=�5       � WindowStyle$ 
ToolWindow=�61    � Title$ Define FICS command��5   	   $	��150��6%   $	��400��62   .��5	      �  � ColumnDefinitions 5
   
   �� 5   
   $
��Auto �6   6*   �� 5   
   $��* �6   6'    � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� $��5q�6   $��0��6   $��0��6/   $��SemiBold"�6<   Description:6`   �� 5   
   -   5        $��textBlockDescription��5      $��5q�6   $��1��69   $��0��6I   6X   �� 5   
   $��5q�6   $��0��6   $��1��6/   $��SemiBold"�6<   
Command:6\   �� 5   
   -   5        $��textBlockCommand��5      $��5q�6   $��1��65   $��1��6E   6T   ]� 5   
   $��0��6   $��2��6$   $��2��68   $��5q�6E   $��Bottom=�6P   ��.  .+�5      �� -   5        $��buttonOk��5      $��5,5,5,5q�6%   $
��Left=�66   $��28��6M   $��28��6X   
������ 
������  �� 	IsDefault . .��5      �� $'��!../Assets/Icons/accept_button.png��6   6F   5      �� 5      -   5        $��buttonClear��5      $��5,5,5,5q�6(   $
��Left=�69   $��28��6P   $��28��6[   
������ 
������  . .��5      �� $*��$../Assets/Icons/clear_formatting.png��6   6I   5      �� 5      -   5        $��buttonCancel��5      $��5,5,5,5q�6*   $��Right=�6;   $��28��6S   $��28��6^   
������ 
������  �� IsCancel . .��5      �� $ ��../Assets/Icons/cancel.png��6   6?   5       5!      5"      5#      