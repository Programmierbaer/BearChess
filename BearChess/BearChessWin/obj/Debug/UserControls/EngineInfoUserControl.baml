   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5      g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5      ;    5www.SoLaNoSoft.com.BearChessWin.EngineInfoUserControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ?local-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5      Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5      
�������$	��662��5   6   .��5
      �� -   5        $��	borderAll��5
      $��1q�6   
�������._�5   
   �� -   5        $��stackPanelMain��5      .+�5      ]� ��.  .+�5      � $
��Left=�6     � RowDefinitions  5      �� 5      6*   �� 5      6*    � ColumnDefinitions 5      �� 5      6-   �� 5      6-   .+�5      �� $��0��6    $��0��6-   $
��Left=�6=   $��5q�6S   $��DemiBold"�6^   $��Name:��6t   $��40��6�   �� 5      -   5        $��textBlockName��5   �   $��0��6    $��1��6.   $
��Left=�6>   $	��200��6T   $
��Left=�6`   $��5q�6{   $��	BearChess��6�   �� 5      -   5        $��textBlockElo��5   V   $��1��6    $��0��6.   $��Bottom=�6>   $
��Left=�6j   $��40��6�   $��5q�6�   $
��Elo:��6�   �� 5      -   5        $��textBlockEloValue��5   W   $��1��6    $��1��6/   $��Bottom=�6?   $
��Left=�6p   $��40��6�   $��5q�6�   $
��----��6�   5      �� 5      -   5        $��buttonPlayStop��5   /   $
��Left=�6   $��32��6E   $��5q�6p   
������ 
������ .��5      �� .+�5      �� -   5        $��	imagePlay��5       $+��%../Assets/Icons/control_play_blue.png��61   6b   �� 5      -   5        $��
imagePause��5       $,��&../Assets/Icons/control_pause_blue.png��62   $y�	Collapsed=�6b   6{   5      5       �� 5!      -	   5        $��
buttonPlus��5!   /   $
��Left=�6   $��32��6A   $��5q�6M   
������ 
������ :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   AddInfoLine � ToolTip# Z 6�   .��5"      �� $'��!../Assets/Icons/toggle_expand.png��6   6I   5#      �� 5$      -
   5        $��buttonMinus��5$   /   $
��Left=�6   $��32��6B   $��5q�6M   
������ 
������    RemoveInfoLine# Z 6�   .��5%      �� $ ��../Assets/Icons/toggle.png��6   6B   5&      �� 5'      -   5        $��buttonClose��5'   /   $
��Left=�6   $��32��6B   $��5q�6M   
������ 
������    CloseEngine# Z 6�   .��5(      �� $,��&../Assets/Icons/control_power_blue.png��6   6N   5)      �� 5*      -   5        $��buttonConfig��5*   /   $
��Left=�6   $��32��6C   $��5q�6N   
������ 
������    ConfigureEngine# Z 6�   .��5+      �� $��../Assets/Icons/cog.png��6   6?   5,      �� 5-      -   5        $��imageBookOpen��5-   #   $��5q�6   $
��Left=�68   $��24��6N   $��24��5.      $#��../Assets/Icons/book_open.png��6$   $y�	Collapsed=�6K      	UsingBook# Z 6b   �� 5/      -   5        $��imageBookClosed��5/   #   $��5q�6   $
��Left=�6:   $��24��6P      NotUsingBook# Z 6[   $��24��50      $��../Assets/Icons/book.png��6$   $y�Visible=�6F   �� 51      -   5        $��imageEngine��51   #   $��5q�6   $��Right=�66   $��64��6M   $��32��52      $#��../Assets/Icons/bearchess.png��6$   $y�Hidden=�6K   �� 53      -   5        $��
imageTeddy��53   #   $��5q�6   $��Right=�65   $��32��6L   $��32��54      $#��../Assets/Icons/BearChess.ico��6$   $y�Hidden=�6K   �� 55      -   5        $��imageColorWhite��55   #   $��5q�6   $��Right=�6:   $��32��6Q   $��32��56      $&�� ../Assets/Stone/Symbol/KingW.png��6$   $y�	Collapsed=�6N   �� 57      -   5        $��imageColorBlack��57   $   $��5q�6   $��Right=�6;   $��32��6R   $��32��58      $&�� ../Assets/Stone/Symbol/KingB.png��6%   $y�Visible=�6O   5;      ]� 5=      ��.  $y�	Collapsed=�6.   .+�5>      �� -   5        $��textBlockElo2��5>   2   $
��Left=�6   $
��Left=�6G   $��40��6b   $��5q�6m   
	   Elo#��Z	 6x   �� 5?      -   5        $��textBlockEloValue2��5?   2   $
��Left=�6   $
��Left=�6L   $��40��6g   $��5q�6r   $
��----��6}   5@      ]� 5B      ��.  .+�5C      �� -   5        $��textBlockDepth��5C   2   $
��Left=�6   $
��Left=�6H   $��90��6c   $��5q�6n   
   Depth#��Z
 6y   �� 5D      -   5        $��textBlockCurrentMove��5D   2   $
��Left=�6   $
��Left=�6N   $	��110��6i   $��5q�6u      Current#��Z 6�   � 5E      $
��Left=�6    5F      �� 5G      $	��110 �6*   68   �� 5H      $��90 �6*   67   �� 5I      $
��Auto �6*   69   .+�5K      �� -   5        $��textBlockCurrentNodes��5K   0   $��0��6    $
��Left=�6M   $��5q�6i   �� 5L      -   5        $!��textBlockCurrentNodesPerSec��5L   0   $��1��6    $
��Left=�6S   $��5q�6o   �� 5M      -   5        $��textBlockCurrentHash��5M   0   $��2��6    $
��Left=�6L   $��5q�6h   5N      �� 5O      -   5        $��
buttonHide��5O   0   $��Right=�6   $��32��6B   $��5q�6M   
������ 
������    HideBestLine# Z 6�   .��5P      �� $��../Assets/Icons/eye.png��6   6?   5Q      �� 5R      -   5        $��buttonHide1��5R   0   $��Right=�6   $��32��6C   $��5q�6N   
������ 
������    HideAll# Z 6�   $y�	Collapsed=�6�   .��5S      �� $"��../Assets/Icons/eye_half.png��6   6D   5T      �� 5U      -   5        $��buttonHide2��5U   0   $��Right=�6   $��32��6C   $��5q�6N   
������ 
������    ShowAll# Z 6�   $y�	Collapsed=�6�   .��5V      �� $#��../Assets/Icons/eye_close.png��6   6E   5W      5X      ?   9www.SoLaNoSoft.com.BearChessWin.EngineInfoLineUserControl  5Y      -   5          Name$  engineInfoLineUserControl1��5Y   /   $
��Left=�6S   $��60��6n   $��1q�6z   5Z      5[      5\      