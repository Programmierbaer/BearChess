   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   0    *www.SoLaNoSoft.com.BearChessWin.FicsWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   -   5          � Icon$'  !../Assets/Icons/freechess.org.png��5   9    � Title$  Free Internet Chess Server��5   	   $	��650��6,   $
��1000��69   .��5	      ]� ��. .+�5
   
   �� $��	15,10,5,5q�6   $	��Top=�6(    �� Orientation$ 
Horizontal=�6=   .+�5      �� $��5q�6   $��SemiBold"�6#   Connected as:6I   �� 5      -   5        $��textBlockUser��5   #   $��5q�6   6:   5      ]� 5   
   $��Bottom=�6   $��5,0,5,0q�6,   ��.  .+�5      �� -   5        $��buttonGetGame��5   6   $��5q�6   $
��Left=�6    
������  � ToolTip$" Start a blitz game (getgame)��5   /   .��5      �� $&�� ../Assets/Icons/lightning_go.png��6   $��32��6B   $��32��6M   5      �� 5      -   5        $��buttonResign��5   =   $��15,5,5,5q�6   $
��Left=�6'   
������ $ Resign��5   /   .��5      �� $'��!../Assets/Icons/6-2-chess-png.png��6   $��32��6C   $��32��6N   5      �� 5      -   5        $��
buttonDraw��5   =   $��15,5,5,5q�6   $
��Left=�6'   
������ $
 Draw��5   /   .��5      �� $#��../Assets/Icons/handshake.png��6   $��32��6?   $��32��6J   5      �� 5      -   5        $��buttonAbort��5   =   $��15,5,5,5q�6   $
��Left=�6'   
������ $ 
Abort game��5   /   .��5      �� $ ��../Assets/Icons/cancel.png��6   $��32��6<   $��32��6G   5      �� 5       -   5        $��buttonLogoff��5    7   $��5q�6   $��Right=�6    
������ 
������ $ Logoff��5!   I   .��5"      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   5#      5$      ]� 5%   
   $��Bottom=�6   $��5,0,5,0q�6,   ��.  .+�5&      �� $��5q�6   $��SemiBold"�6#   $
��Left=�69   Current action:6a   �� 5'      -   5        $��textBlockAction��5'   #   $��5q�6   $
��Left=�6:   .....6X   5(      �� 5)   
   -	   5        $��tabControlMain��5)      $��Bottom=�6+   $��5q�6C   .@�5*      �� -
   5        $��tabItemMain��5*      $
��Main��6)   $��Stretch=�67   $��Stretch=�6Z   .��5+      9   3www.SoLaNoSoft.com.BearChessWin.FicsMainUserControl  -   5          Name$ ficsMainUserControl��5+   3   $��Stretch=�6Q   $��Stretch=�6p   $��5q�6�   5,      �� 5-      -   5        $��tabItemGames��5-      $��Advertisements��6*   $��Stretch=�6B   $��Stretch=�6e   .��5.      8   2www.SoLaNoSoft.com.BearChessWin.FicsAdsUserControl  -   5          Name$ ficsAdsUserControl��5.   y   $��Stretch=�63   $��Stretch=�6R   $��5q�6n   6y   5/      �� 50      -   5        $��tabItemUsers��50      $��Users��6*   $��Stretch=�69   $��Stretch=�6\   .��51      9   3www.SoLaNoSoft.com.BearChessWin.FicsUserUserControl  -   5          Name$ ficsUserUserControl��51   y   $��Stretch=�63   $��Stretch=�6R   $��5q�6n   6y   52      �� 53      -   5        $��
tabItemLog��53      $	��Log��6)   $��Stretch=�66   $��Stretch=�6Y   .��54      �  � ColumnDefinitions 55      �� 56      $��* �6+   67    � RowDefinitions 58      �� 59      $��* �6(   �� 5:      $
��Auto �6(   .+�5=      �� -   5        $��listBoxInfo��5=   6   $��5q�6   $��0��6)   $��Consolas)�6I   �� 5?      $��5q�6!   $��1��6,   $ 
Horizontal=�69   .+�5@      �� -   5        $��
buttonStop��5@   !   $��Center=�63   $��5,0,5,0q�6N   $��24��6_   $��24��6j   
������ 
������ $ Stop/Go��5A      .��5B      �� .+�5C   "   �� -   5        $��	imagePlay��5C   (   $+��%../Assets/Icons/control_play_blue.png��69   $y�	Collapsed=�6h   6�   �� 5D   "   -   5        $��
imagePause��5D   (   $,��&../Assets/Icons/control_pause_blue.png��6:   6m   5E      5G      �� 5H      -   5        $��buttonClear��5H   !   $��Center=�64   $��0,0,5,0q�6O   $��24��6`   $��24��6k   
������ 
������ $ Clear��5I      .��5J      �� $*��$../Assets/Icons/clear_formatting.png��6$   6T   5K      �� 5M      -   5        $��textBoxCommand��5M   3   $��0,0,5,0q�6"   $	��200��6I   $ Enter a FICS command.��6V   �� 5N      -   5        $��
buttonSend��5N   !   $��Center=�63   $��0,0,5,0q�6N   $��24��6_   $��24��6j   
������ 
������ $ Send command to FICS��5O      .��5P      �� $#��../Assets/Icons/bullet_go.png��6$   6M   5Q      �� 5R      -   5        $��buttonClipboard��5R   !   $��Center=�68   $��0,0,5,0q�6S   $��24��6d   $��24��6o   
������ 
������ $ Copy to clipboard��5S      .��5T      �� $(��"../Assets/Icons/clipboard_sign.png��6$   6R   5U      5V      5W      5X      5Y      5[      5\      