   M S B A M L   `   `   `  ����M  IBearChessServerWin, Version=0.1.0.0, Culture=neutral, PublicKeyToken=nullm;clr-namespace:www.SoLaNoSoft.com.BearChessServerWin.Windows-www.SoLaNoSoft.com.BearChessServerWin.Windows  5   	   s>clr-namespace:www.SoLaNoSoft.com.BearChessServerWin.Properties0www.SoLaNoSoft.com.BearChessServerWin.Properties  5   	   I    Cwww.SoLaNoSoft.com.BearChessServerWin.Windows.ConfigureServerWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Mlocal;clr-namespace:www.SoLaNoSoft.com.BearChessServerWin.Windows     5   	   Lp>clr-namespace:www.SoLaNoSoft.com.BearChessServerWin.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ Height=�6T    � Title$ BearChess Server��5	   	   $	��200��6"   $	��300��6/   .��5
      �  � ColumnDefinitions 5   
   �� 5      $
��Auto �6   6.   �� 5      $��* �6   6+    � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� $��5q�6   $��0��6   $��0��6/   $��SemiBold"�6<   @   :www.SoLaNoSoft.com.BearChessServerWin.Properties.Resources   
ServerName#��Z 6R   �� 5   
   -   5        $��textBlockServer��5      $��5q�6   $��1��64   $��0��6D   .��6Q   	localhost6\   �� 5   
   $��5q�6   $��0��6   $��1��6/   $��SemiBold"�6<      Port#��Z 6R   �� 5   
   -   5        $��textBlockPort��5      $��5q�6   $��1��62   $��1��6B   .��6O   88886U   ]� 5   
   $��0��6   $��2��6$   $��2��68   $��5q�6E   $��Bottom=�6P   ��.  .+�5      �� -   5        $��buttonOk��5      $��5,5,5,5q�6%   $
��Left=�66   $��28��6M   $��28��6X   
������ 
������  �� 	IsDefault . .��5      �� $'��!../Assets/Icons/accept_button.png��6   6F   5      �� 5      -   5        $��buttonCancel��5      $��5,5,5,5q�6*   $��Right=�6;   $��28��6S   $��28��6^   
������ 
������ 	 �� IsCancel	 . .��5       �� $ ��../Assets/Icons/cancel.png��6   6?   5!      5"      5#      5$      