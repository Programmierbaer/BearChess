   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   2    ,www.SoLaNoSoft.com.BearChessWin.SplashWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   
������  � WindowStyle$  SingleBorderWindow=�5       � 
ResizeMode$ NoResize=�6;    � Title$ Information��5   	   $	��110��6   $	��390��6+   .��5	      � $��5,0,5,0q�6   .+�5
   
   �� $��Bottom=�6   .+�5      �� -   5        $��ScrollViewer��5   ,   $��0,5,0,0q�6   $	��200��6@   $
��Auto=�6P   .��5      �� -   5        $��StackPanelProgressControl��5      5      � 5      $��5,0,5,5q�6    � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'   �� 5      $
��Auto �6'   .+�5      �� -   5         ��Name$ SubCancelLabel��5   X   $��Center=�6   $��1��67   $��5,5,4,0q�6G   $��Cancel requested...��5      
���  ��$��Italic$�5   -   $
��Bold"�6@   $y�Visible=�6R   $��CharacterEllipsis=�5      �� 5      -   5         ��Name$ CancelButton��5   ;   $��5,10,4,0q�6   $��2��6+   
������ 
������ $��24��6�   $��24��6�   $y�Visible=�5      $��Right=�6.   .��5      �� $ ��../Assets/Icons/cancel.png��6   6B   5      5       5!      5"      5#      