   M S B A M L   `   `   `  ����  BearChessWin>    8www.SoLaNoSoft.com.BearChessWin.ParameterSelectionWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � WindowStartupLocation$  CenterOwner=�5       � Title$ 
Parameter ��5   	   $	��360��6   $	��450��6)   .��5      �  � RowDefinitions 5	   
   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   � $��0��6   $��5q�6    � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $��* �6#   �� 5      $
��Auto �6#    5      �� 5      $
��Auto �6    �� 5      $��* �6    �� 5      $
��Auto �6    .+�5      �� -   5        $��textBlockText��5   %   $��30��5      $	��Top=�6$    �� TextAlignment$ Center=�6<   $��5,0,5,5q�6S   $��0��6d   $��0��5      Engine6B   �� 5      -   5        $��textBoxText��5   c   $��30��5      $	��Top=�6"    �� TextAlignment$
 Left=�6:   $
��Left=�6O   $	��Top=�5      $��5,0,5,5q�65   $��1��6F   $��0��6V   .��6v   Engine6~   �� 5      -   5        $��buttonParameterFile��5   2   $��0��6   $��2��6"   
������ 
������ .��5      �� $ ��../Assets/Icons/folder.png��6   $��16��6<   $��16��6G   5      �� 5       -   5        $��listBoxEngines��5!   K   $	��Top=�5       $��5q�6.   $��1��69   $��1��6I    �� SelectionMode$ Single=�6V   5!   a   �� 5"      -   5        $��textBlockFilter��5"   <   $��30��6   $	��Top=�6$   $ Center=�6S   $��5,0,5,5q�6j   $��0��6{   $��2��5#      $y�Visible=�6%   	Filter:6C   �� 5$      -   5        $��textBoxFilter��5%   c   $��30��5$      $	��Top=�6"   $
 Left=�6:   $
��Left=�6O   $	��Top=�5%      $��5,0,5,5q�65   $��1��6F   $��2��6V   $y�Visible=�6x   5&      ]� 5'   
   $��1��6   $��10q�6!   ��.  .+�5(      �� -   5        $��buttonOk��5(   +   $
��Left=�6   
������ 
������  � ToolTip$
 Save��5)   .   .��5*      �� $'��!../Assets/Icons/accept_button.png��6   $��32��6C   $��32��6N   5+      �� 5,      -   5        $��checkBoxSkipWarning��5,   @   $��Center=�6   $
��10,5q�62   $
��Left=�6\   .��6r   /-Skip error if the engine already is installed6�   �� 5-      -	   5        $��buttonCancel��5-   ,   $��Right=�6   
������ 
������ $ Cancel��5.   .   .��5/      �� $ ��../Assets/Icons/cancel.png��6   $��32��6<   $��32��6G   50      52      53      54      