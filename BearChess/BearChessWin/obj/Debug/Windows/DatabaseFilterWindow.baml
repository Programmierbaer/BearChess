   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   :    4www.SoLaNoSoft.com.BearChessWin.DatabaseFilterWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          � SizeToContent$  WidthAndHeight=�5   P    � Icon$&  ../Assets/Icons/table_filter.png��5       � 
ResizeMode$ NoResize=�6B   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   DatabaseFilter � Title# Z 5   	   $	��430��67   $	��450��6D   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� -   5        $��checkBoxEnableFilter��5      $��5q�6/   $��0��6:      EnableFilter#��Z 5   <   5      � 5   
   -   5        $��
gridFilter��5      $��1��6!   $��5q�6.   
������� 5      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    .+�5      �� $��5q�6   $��0��6$   $��0��61   .+�5      �� -   5        $��checkBoxTournament��5   &   $��5q�6      NoTournamentGames#��Z 6@   �� 5      -   5        $��checkBoxDuel��5   &   $��5q�6      NoDuelGames#��Z 6:   �� 5      -   5        $��checkBoxDuplicates��5   &   $��5q�6   	   OnlyDuplicates#��Z	 6@   5      � 5      $��5q�6   $��1��6   $��0��6+   
 � ColumnDefinitions
 5       �� 5!      $
��Auto �6'   �� 5"      $��* �6'    5$      �� 5%      $
��Auto �6$   �� 5&      $
��Auto �6$   �� 5'      $
��Auto �6$   �� 5(      $
��Auto �6$   �� 5)      $
��Auto �6$   .+�5+      �� $��5q�6   $��0��6'   $��0��67      Player#��Z 6D   6k   �� 5,      -   5        $��textBoxWhite��5,      $��5q�6.   $��1��69   $��0��6I   �� 5-      $��1��6   $��1��6*    �� Orientation$ 
Horizontal=�6:   .+�5.      	� -   5        $��radioButtonWhite��5.   "    	� 	GroupName$ Player1��6:   $��5q�6N    d� 	IsChecked$
 TrueY�6Y      White#��Z 6j   50      	� 51      -	   5        $��radioButtonWhiteWhatever��51   #   $ Player1��6E   $��5q�6Y      Whatever#��Z 6d   6�   52      �� 53      $��5q�6   $��0��6'   $��2��67   #��Z 6D   6k   �� 54      -
   5        $��textBoxBlack��54      $��5q�6.   $��1��69   $��2��6I   �� 55      $��3��6   $��1��6*   $ 
Horizontal=�6:   .+�56      	� -   5        $��radioButtonBlack��56   #   $ Player2��6<   $��5q�6P   $
 TrueY�6[      Black#��Z 6l   57      	� 58      -   5        $��radioButtonBlackWhatever��58   #   $ Player2��6D   $��5q�6X   #��Z 6c   6�   59      �� 5:      $��5q�6   $��0��6'   $��4��67      Event#��Z 6D   6j   �� 5;      -   5        $��textBoxEvent��5;      $��5q�6.   $��1��69   $��4��6I   5<      �� 5=      $��5q�6   $��0��6$   $��2��64   $ 
Horizontal=�6A   .+�5?      �� $��5q�6   $��Center=�6'      From#��Z 6B   6g   (  "System.Windows.Controls.DatePicker  5@      -   5        $��datePickerFromDate��5@      $��Center=�67   $��10,5,5,5q�6R   �� 5B      $��5q�6   $��Center=�6'   	   To#��Z 6B   6e     5C      -   5        $��datePickerToDate��5C      $��Center=�65   $��5q�6P   5D      �� 5E      $��5q�6   $��0��6$   $��3��64   $ 
Horizontal=�6A   .+�5F      �� -   5        $��buttonApplyFilter��5F   $   $��5q�6   
������ 
������    ApplyFilter � ToolTip# Z 5H   2   .��5I      �� $(��"../Assets/Icons/filter_reapply.png��6   $��32��6H   $��32��6S   5J      �� 5K      -   5        $��buttonClearFilter��5K   +   $��15,5,5,5q�6   
������ 
������    ClearFilter# Z 5M   2   .��5N      �� $&�� ../Assets/Icons/filter_clear.png��6   $��32��6F   $��32��6Q   5O      5P      5Q      ]� 5R   
   $��2��6   ��.  .+�5S      �� -   5        $��
buttonSave��5S   7   $��10q�6   $
��Left=�6!   
������ 
������    SaveFilterAsDefault# Z 5T   c   .��5U      �� $"��../Assets/Icons/diskette.png��6   $��32��6>   $��32��6I   5V      �� 5W      -   5        $��buttonCancel��5W   9   $��10q�6   $��Right=�6!   
������ 
������ .��5Y      �� $"��../Assets/Icons/door_out.png��6   $��32��6:   $��32��6E   5Z      5[      5\      5]      