   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   a5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   D    >www.SoLaNoSoft.com.BearChessWin.TournamentInfoRoundRobinWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   -   5          � SizeToContent$  Height=�5       � Icon$" ../Assets/Icons/cup_gold.png��61   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   EngineTournamentInfo � Title# Z 5	   	   $	��170��6=   $	��350��6J   $	��170��6V   $	��300��6f   .��5
      �� .+�5   
   ]� ��.  .+�5      �� -   5        $��textBlockStatus��5      $
��Left=�6/   $��5q�6E   Current game 1 of 26f   �� 5      -   5        $��checkBoxSwitchColor��5      $��Center=�62   $��Right=�6N   {�.  $��0,0,50,0q�6w   6�   �� 5      $	��100��6   $��Center=�6    $��Right=�6;      SwitchColor#��Z 6R   5      �� 5   
   6   �� 5   
   $��Visible=�6   $
��Auto=�6?   .��5      � -   5        $��gridDuel��5      $��0,0,0,5q�6#    � ColumnDefinitions 5      �� 5      $��30 �6'   �� 5      $
��Auto �6'   �� 5      $��40 �6'    � RowDefinitions 5      �� 5      �� 5      �� 5      .+�5      �� $��5q�6   $��0��6'   $
��Wrap=�64   $��30��6H    �� TextAlignment$ Center=�6S   $��2��6j   $��SemiBold"�6z   $	��∑��6�   6�   5!      5"      �� 5#   
   6   ]� 5$   
   $��5q�6   .+�5%      �� -   5        $��	buttonWin��5%      $��25,5,5,5q�6&   $��Right=�68   $��32��6T   $��32��6_   
������    SaveGameAsWinForWhite	 � ToolTip#	 Z 5&   .   
������ $
��Left=�5'   /   $	��1-0��5(   /   ��5)      �� 5*      
 �� 
TargetType"
 ��6&   .R�5+      � .+�5,      A� 
������$��1��5-   &   ��5.   "   
� 5/   &   .��50   *   �  � Offset$ 0Q�67    � Color$ 	lightBlue��51   7   � 52   *   $ 1Q�67   $ 	lightBlue��53   7   � 54   *   $ 1Q�67   $ 	LightBlue��55   7    �� 	Transform 56   *   V� 57   .   .~�58   2   ��  �� ScaleY$
 0.65Q�6A   59   /   5;   '   5=      �� 5>      $��Center=�6/   $��Center=�5?   /   5@      5A      5C      �� 5E      -   5        $��
buttonLose��5E      $��5q�6'   $��Right=�62   $��32��6N   $��32��6Y   
������    SaveGameAsWinForBlack#	 Z 5F   .   
������ $
��Left=�5G   /   $	��0-1��5H   0   ��5I      �� 5J      "
 ��6&   .R�5K      � .+�5L      A� 
������$��1��5M   &   ��5N   "   
� 5O   &   .��5P   *   � $ 0Q�67   $ 	LightBlue��5Q   7   � 5R   *   $ 1Q�67   $ 	LightBlue��5S   7   � 5T   *   $ 1Q�67   $ 	LightBlue��5U   7    5V   *   V� 5W   .   .~�5X   2   �� $
 0.65Q�6A   5Y   /   5[   '   5]      �� 5^      $��Center=�6/   $��Center=�5_   /   5`      5a      5c      �� 5f      -   5        $��
buttonDraw��5f      $��5q�6'   $��Center=�62   $��Center=�6M   $��32��6j   $��32��5g      
������    SaveGameAsDraw#	 Z 5h   .   
������ $
��Left=�5i   /   $	��1/2��5j   0   ��5k      �� 5l      "
 ��6&   .R�5m      � .+�5n      A� 
������$��1��5o   &   ��5p   "   
� 5q   &   .��5r   *   � $ 0Q�67   $ 	LightBlue��5s   7   � 5t   *   $ 1Q�67   $ 	LightBlue��5u   7   � 5v   *   $ 1Q�67   $ 	LightBlue��5w   7    5x   *   V� 5y   .   .~�5z   2   �� $
 0.65Q�6A   5{   /   5}   '   5      �� 5�      $��Center=�6/   $��Center=�5�   /   5�      5�      5�      �� 5�      -   5        $��buttonPause��5�      $��Right=�6(   $��5,5,10,5q�6?   $��Right=�6Q   
������ 
������ .��5�      �� $+��%../Assets/Icons/control_stop_blue.png��6   $��32��6G   $��32��6R   5�      �� 5�      -	   5        $��buttonContinue��5�      $��Right=�6+   $��5,5,10,5q�6B   $��Right=�6T   $y�	Collapsed=�6q   
������ 
������ .��5�      �� $+��%../Assets/Icons/control_play_blue.png��6   $��32��6G   $��32��6R   5�      �� 5�      -
   5        $��buttonClose��5�      $��Right=�6(   $��5,5,10,5q�6?   $��Right=�6Q   $y�	Collapsed=�6m   
������ 
������ .��5�      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   5�      �� 5�      -   5        $��buttonExport��5�      $��Right=�6)   $��5,5,5,5q�6@   $��Right=�6Q   $y�Visible=�6m   
������ 
������ .��5�      �� $-��'../Assets/Icons/file_extension_html.png��6   $��32��6I   $��32��6T   5�      5�      5�      5�      