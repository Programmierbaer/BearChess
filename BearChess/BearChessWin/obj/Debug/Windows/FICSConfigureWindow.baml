   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   9    3www.SoLaNoSoft.com.BearChessWin.FICSConfigureWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ Height=�6T    � Title$  Free Internet Chess Server��5	   	   $	��200��6,   $	��300��69   .��5
      �  � ColumnDefinitions 5   
   �� 5      $
��Auto �6   6.   �� 5      $��* �6   6+    � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� $��5q�6   $��0��6   $��0��6/   $��SemiBold"�6<   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   Username#��Z 6R   �� 5   
   -   5        $��textBlockUserName��5      $��5q�6   $��1��66   $��0��6F   6U   �� 5   
   $��5q�6   $��0��6   $��1��6/   $��SemiBold"�6<      Password#��Z 6R   G� 5   
   -   5        $��textBlockPassword��5   2    G� PasswordChar$ *��6   $��5q�6'   $��1��6L   $��1��6\   6k   �� 5   
   $��5q�6   $��0��6   $��2��6/   $��SemiBold"�6<   	   Server#��Z	 6R   �� 5   
   -   5        $��textBlockServer��5      $��5q�6   $��1��64   $��2��6D   .��6Q   www.freechess.org6d   �� 5   
   $��5q�6   $��0��6   $��3��6/   $��SemiBold"�6<   
   Port#��Z
 6R   �� 5   
   -   5        $��textBlockPort��5      $��5q�6   $��1��62   $��3��6B   .��6O   50006U   �� 5   
   $��5q�6   $��4��6   $��0��6,   $��SemiBold"�6<      AsGuest#��Z 6R   6z   �� 5    
   -   5        $��checkBoxAsGuest��5       $��5q�6   $��4��65   $��1��6B   6U   ]� 5!   
   $��0��6   $��2��6$   $��5��68   $��5q�6E   $��Bottom=�6P   ��.  .+�5"      �� -   5        $��buttonOk��5"      $��5,5,5,5q�6%   $
��Left=�66   $��28��6M   $��28��6X   
������ 
������  �� 	IsDefault . .��5#      �� $'��!../Assets/Icons/accept_button.png��6   6F   5$      �� 5%      -   5        $��buttonCancel��5%      $��5,5,5,5q�6*   $��Right=�6;   $��28��6S   $��28��6^   
������ 
������  �� IsCancel . .��5&      �� $ ��../Assets/Icons/cancel.png��6   6?   5'      5(      5)      5*      