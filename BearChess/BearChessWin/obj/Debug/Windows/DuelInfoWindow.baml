   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   4    .www.SoLaNoSoft.com.BearChessWin.DuelInfoWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          � WindowStyle$  
ToolWindow=�5   P   
������� � Icon$' !../Assets/Icons/6-2-chess-png.png��5   	   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   DuelInformation � Title# Z 5	   	   $	��220��68   $	��380��6E   $	��220��6Q   $	��380��6a   .��5
      �� .+�5   
   ]� ��.  .+�5      �� -   5        $��textBlockStatus��5      $
��Left=�6/   $��5q�6E   Current game 1 of 26e   �� 5      -   5        $��checkBoxSwitchColor��5      $��Center=�62   $��Right=�6M   {�.  $��0,0,50,0q�5      �� 5      $	��100��6   $��Center=�6    $��Right=�6;      SwitchColor#��Z 6R   6�   5      �� 5   
   �� 5   
   $��Visible=�6   $
��Auto=�6?   .��5      � -   5        $��gridDuel��5       � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��40 �6'    � RowDefinitions 5      �� 5      �� 5      �� 5      .+�5      � $��0��6   $��0��6$    5      �� 5       �� 5!       5#      �� 5$      �� 5%      5'      �� 5)      $��5q�6   $��0��6'   $
��Wrap=�64   $��0��6H   $��DemiBold"�6X      
EngineGame#��Z 6o   �� 5*      $��5q�6   $��0��6'   $
��Wrap=�64   $��1��6H   $��SemiBold"�6X   $	��∑��6n   �� 5+      -   5        $��textBlockEngine1��5+      $��5q�64   $��1��6?   $��0��6L   
Engine 16f   �� 5,      -   5        $��textBlockEngine2��5,      $��5q�64   $��2��6?   $��0��6L   
Engine 26f   �� 5-      -   5        $��textBlockResult1��5-      $��5q�64   $��1��6?   $��1��6L   $��SemiBold"�6\   06u   �� 5.      -   5        $��textBlockResult2��5.      $��5q�64   $��2��6?   $��1��6L   $��SemiBold"�6\   06u   5/      50      �� 51   
   ]� 52   
   $��5q�6   .+�53      �� -	   5        $��checkBoxPauseAfterGame��53      $��5q�65   $
��Left=�6@   .��55      ��    PauseAfterGame#��Z 6   56      �� 59      -
   5        $��	buttonWin��59      $��25,5,5,5q�6&   $��Right=�68   $��32��6T   $��32��6_   
������ 	   SaveGameAsWinForWhite
 � ToolTip#
 Z	 5:   .   
������ $
��Left=�5;   /   $	��1-0��5<   /   ��5=      �� 5>       �� 
TargetType" ��6&   .R�5?      � .+�5@      A� 
������$��1��5A   &   ��5B   "   
� 5C   &   .��5D   *   �  � Offset$ 0Q�67    � Color$ 	lightBlue��5E   7   � 5F   *   $ 1Q�67   $ 	lightBlue��5G   7   � 5H   *   $ 1Q�67   $ 	LightBlue��5I   7    �� 	Transform 5J   *   V� 5K   .   .~�5L   2   ��  �� ScaleY$
 0.65Q�6A   5M   /   5O   '   5Q      �� 5R      $��Center=�6/   $��Center=�5S   /   5T      5U      5W      �� 5Y      -   5        $��
buttonLose��5Y      $��5q�6'   $��Right=�62   $��32��6N   $��32��6Y   
������    SaveGameAsWinForBlack#
 Z 5Z   .   
������ $
��Left=�5[   /   $	��0-1��5\   0   ��5]      �� 5^      " ��6&   .R�5_      � .+�5`      A� 
������$��1��5a   &   ��5b   "   
� 5c   &   .��5d   *   � $ 0Q�67   $ 	LightBlue��5e   7   � 5f   *   $ 1Q�67   $ 	LightBlue��5g   7   � 5h   *   $ 1Q�67   $ 	LightBlue��5i   7    5j   *   V� 5k   .   .~�5l   2   �� $
 0.65Q�6A   5m   /   5o   '   5q      �� 5r      $��Center=�6/   $��Center=�5s   /   5t      5u      5w      �� 5z      -   5        $��
buttonDraw��5z      $��5q�6'   $��Center=�62   $��Center=�6M   $��32��6j   $��32��5{      
������    SaveGameAsDraw#
 Z 5|   .   
������ $
��Left=�5}   /   $	��1/2��5~   0   ��5      �� 5�      " ��6&   .R�5�      � .+�5�      A� 
������$��1��5�   &   ��5�   "   
� 5�   &   .��5�   *   � $ 0Q�67   $ 	LightBlue��5�   7   � 5�   *   $ 1Q�67   $ 	LightBlue��5�   7   � 5�   *   $ 1Q�67   $ 	LightBlue��5�   7    5�   *   V� 5�   .   .~�5�   2   �� $
 0.65Q�6A   5�   /   5�   '   5�      �� 5�      $��Center=�6/   $��Center=�5�   /   5�      5�      5�      �� 5�      -   5        $��
buttonStop��5�      $��5,5,10,5q�6+   $��Right=�6=   $��Right=�6Y   
������ 
������    StopDuel#
 Z 5�   6   .��5�      �� $+��%../Assets/Icons/control_stop_blue.png��6   $��32��6K   $��32��6V   5�      �� 5�      -   5        $��buttonContinue��5�      $��5,5,10,5q�6/   $��Right=�6A   $��Right=�6]   $y�	Collapsed=�5�      
������ 
������    ContinueDuel#
 Z 5�   8   .��5�      �� $+��%../Assets/Icons/control_play_blue.png��6   $��32��6K   $��32��6V   5�      �� 5�      -   5        $��buttonClose��5�      $��5,5,10,5q�6,   $��Right=�6>   $��Right=�6Z   $y�	Collapsed=�5�      
������ 
������    Exit#
 Z 5�   5   .��5�      �� $"��../Assets/Icons/door_out.png��6   $��32��6B   $��32��6M   5�      �� 5�      -   5        $��buttonStopOnPaused��5�      $��5q�6/   $��Right=�6:   $��Right=�6V   
������ 
������ $y�	Collapsed=�5�   /   #
 Z 5�   :   .��5�      �� $+��%../Assets/Icons/control_stop_blue.png��6   $��32��6G   $��32��6R   5�      5�      5�      5�      