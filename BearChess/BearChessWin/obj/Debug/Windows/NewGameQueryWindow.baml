   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   @    :www.SoLaNoSoft.com.BearChessWin.Windows.NewGameQueryWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5      :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   WhatDoYouLikeToDo � Title# Z 5	   	   $	��180��6;   $	��500��6H    � SizeToContent$ WidthAndHeight=�6T   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $	��100 �6   �� 5      $	��100 �6   �� 5      $	��100 �6   .+�5   
   �� $��0��6   $��0��6%   $��2��62    �� Orientation$ 
Horizontal=�6F   .+�5      �� $*��$../Assets/Icons/emotion_question.png��6   $
��Left=�6B   $��5q�6]   $��32��6h   $��32��6t   �� 5      $
��Bold"�6   $��5q�6*   $��Center=�65   $��Center=�6P      Action#��Z 6m   5      �� 5   
   -   5        $��buttonStart��5      $��1��6   $��
5,10,10,10q�61   $��0��6E   
������ 
������ .��5      �� .+�5      �� $��0,5,0,0q�6   $#��../Assets/Icons/bearchess.png��6)   $��32��6P   $��32��6[   6i   �� 5      $��5q�6   $
��Wrap=�6'      StartNewGame#��Z 6;   5      5      �� 5   
   -   5        $��buttonContinue��5      $��1��6   $��
5,10,10,10q�64   $��1��6H   
������ 
������ .��5      �� .+�5       �� $��0,5,0,0q�6   $+��%../Assets/Icons/control_play_blue.png��6+   $��32��6Z   $��32��6e   6s   �� 5!      $��5q�6   	   Continue#��Z	 6'   5"      5#      �� 5$   
   -   5        $��buttonCancel��5$      $��1��6   $��
5,10,10,10q�62   $��2��6F   
������ 
������ .��5%      �� .+�5&      �� $��0,5,0,0q�6   $ ��../Assets/Icons/cancel.png��6+   $��32��6O   $��32��6Z   6h   �� 5'      $��5q�6   
   Stop#��Z
 6'   5(      5)      5*      5+      