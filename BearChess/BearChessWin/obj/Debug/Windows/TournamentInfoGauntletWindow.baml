   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   a5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   B    <www.SoLaNoSoft.com.BearChessWin.TournamentInfoGauntletWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   -   5          � SizeToContent$  Height=�5       � Icon$" ../Assets/Icons/cup_gold.png��61   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   EngineTournamentInfo � Title# Z 5	   	   $	��170��6=   $	��350��6J   $	��170��6V   $	��300��6f   .��5
      �� .+�5   
   ]� ��.  .+�5      �� -   5        $��textBlockStatus��5      $
��Left=�6/   $��5q�6E   Current game 1 of 26f   �� 5      -   5        $��checkBoxSwitchColor��5      $��Center=�62   $��Right=�6N   {�.  $��0,0,50,0q�6w   6�   �� 5      $��80��6   $��Center=�6   $��Right=�6:   .��6R   Switch color:6a   5      �� 5   
   6   �� 5   
   $��Visible=�6   $
��Auto=�6?   .��5      � -   5        $��gridDuel��5      $��0,0,0,5q�6#    � ColumnDefinitions 5      �� 5      $��30 �6'   �� 5      $
��Auto �6'   �� 5      $��40 �6'   �� 5      $
��Auto �6'   �� 5      $
��Auto �6'    � RowDefinitions 5      �� 5      �� 5      �� 5      .+�5!      �� $��5q�6   $��0��6'   $
��Wrap=�64   $��30��6H    �� TextAlignment$ Center=�6S   $��2��6j   $��SemiBold"�6z   $	��∑��6�   6�   �� 5"      -   5        $��textBlockSumDeliquent��5"      $��5q�69   $��0��6D   $
��Wrap=�6Q   $��80��6e   $ Center=�6p   $��4��6�   $��SemiBold"�6�   6�   5$      5%      �� 5&   
   6   ]� 5'   
   $��5q�6   .+�5(      �� -   5        $��	buttonWin��5(      $��25,5,5,5q�6&   $��Right=�68   $��32��6T   $��32��6_   
������    SaveGameAsWinForWhite � ToolTip# Z 5)   .   
������ $
��Left=�5*   /   $	��1-0��5+   /   ��5,      �� 5-      	 �� 
TargetType"	 ��6&   .R�5.      � .+�5/      A� 
������$��1��50   &   ��51   "   
� 52   &   .��53   *   � 
 � Offset$
 0Q�67    � Color$ 	lightBlue��54   7   � 55   *   $
 1Q�67   $ 	lightBlue��56   7   � 57   *   $
 1Q�67   $ 	LightBlue��58   7    �� 	Transform 59   *   V� 5:   .   .~�5;   2   ��  �� ScaleY$
 0.65Q�6A   5<   /   5>   '   5@      �� 5A      $��Center=�6/   $��Center=�5B   /   5C      5D      5F      �� 5H      -   5        $��
buttonLose��5H      $��5q�6'   $��Right=�62   $��32��6N   $��32��6Y   
������    SaveGameAsWinForBlack# Z 5I   .   
������ $
��Left=�5J   /   $	��0-1��5K   0   ��5L      �� 5M      "	 ��6&   .R�5N      � .+�5O      A� 
������$��1��5P   &   ��5Q   "   
� 5R   &   .��5S   *   � $
 0Q�67   $ 	LightBlue��5T   7   � 5U   *   $
 1Q�67   $ 	LightBlue��5V   7   � 5W   *   $
 1Q�67   $ 	LightBlue��5X   7    5Y   *   V� 5Z   .   .~�5[   2   �� $
 0.65Q�6A   5\   /   5^   '   5`      �� 5a      $��Center=�6/   $��Center=�5b   /   5c      5d      5f      �� 5i      -   5        $��
buttonDraw��5i      $��5q�6'   $��Center=�62   $��Center=�6M   $��32��6j   $��32��5j      
������    SaveGameAsDraw# Z 5k   .   
������ $
��Left=�5l   /   $	��1/2��5m   0   ��5n      �� 5o      "	 ��6&   .R�5p      � .+�5q      A� 
������$��1��5r   &   ��5s   "   
� 5t   &   .��5u   *   � $
 0Q�67   $ 	LightBlue��5v   7   � 5w   *   $
 1Q�67   $ 	LightBlue��5x   7   � 5y   *   $
 1Q�67   $ 	LightBlue��5z   7    5{   *   V� 5|   .   .~�5}   2   �� $
 0.65Q�6A   5~   /   5�   '   5�      �� 5�      $��Center=�6/   $��Center=�5�   /   5�      5�      5�      �� 5�      -	   5        $��buttonPause��5�      $��Right=�6(   $��5,5,10,5q�6?   $��Right=�6Q   
������ 
������ .��5�      �� $+��%../Assets/Icons/control_stop_blue.png��6   $��32��6G   $��32��6R   5�      �� 5�      -
   5        $��buttonContinue��5�      $��Right=�6+   $��5,5,10,5q�6B   $��Right=�6T   $y�	Collapsed=�6q   
������ 
������ .��5�      �� $+��%../Assets/Icons/control_play_blue.png��6   $��32��6G   $��32��6R   5�      �� 5�      -   5        $��buttonClose��5�      $��Right=�6(   $��5,5,10,5q�6?   $��Right=�6Q   $y�	Collapsed=�6m   
������ 
������ .��5�      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   5�      5�      5�      5�      