   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   H    Bwww.SoLaNoSoft.com.BearChessWin.SelectInstalledEngineForGameWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   -   5          � Icon$  ../Assets/Icons/robot.png��5   P   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   LoadUCIEngine � Title# Z 5   	   $	��390��66   $	��500��6C   $	��500��6P   .��5	      �  � RowDefinitions 5
   
   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $��dataGridEngine��5      $��0��6B   $��5q�6O      AutoGenerateColumns .     SelectionMode$ Single=�5         
IsReadOnly . ��5   )   ��    RowStyle 5      �� 5       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6   .�5      -   5        5      	 � 	Resources	 5      
 �� Color��%�   (��       5   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� (     5   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5      $
 CornflowerBlue��6e   �� 5      $
 White��6i   5         Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6>      Name   Header# Z 6K   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *Name+6K     5      $
 Auto 6@      Author# Z 6M    6%   �� *Author+6M   5      ]� 5   
   $��5q�6   $��1��6   ��.  .+�5       �� -   5        $��buttonOk��5    7   $��5q�6   $
��Left=�6!   
������ 
������    LoadSelectedEngineTip � ToolTip# Z 6�   .��5!      �� $'��!../Assets/Icons/accept_button.png��6   $��32��6C   $��32��6N   6\   5"      �� 5#      $��Center=�6   $��5q�63      Filter#��Z 6>   �� 5$      -   5        $��textBoxFilter��5$   R   $��Center=�6   $��5q�61   $
��Left=�6<   $��25��6�   $	��130��6�   6�   �� 5%      -   5        $��buttonCancel��5%   ?   $��25,5,5,5q�6   $��Right=�6(   
������ 
������    Cancel# Z 6�   .��5&      �� $ ��../Assets/Icons/cancel.png��6   $��32��6<   $��32��6G   6U   5'      50      51      52      