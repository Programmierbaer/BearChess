   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   3    -www.SoLaNoSoft.com.BearChessWin.NewGameWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ?local-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5      :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   NewGame � Title# Z 5	   	   $	��590��60   $	��695��6=    � 	Resources 5
         CtrEnter �� Text�� 5   
   %(   &	        �� 5      $
 Exit��6/   5       J� InputBindings 5      �� 5   
   
 �� Key$ Enter��6    �� 	Modifiers$
 Ctrlf�6!    �� Command# [  62   	 J� CommandBindings	 5      )  #System.Windows.Input.CommandBinding  5   
   -   5        
   Command#
 [  5      6=   .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��2q�6   $��0��6   
�������$��1q�6<   ._�5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'    5       �� 5!      $
��Auto �6$   �� 5"      $
��Auto �6$   �� 5#      $
��Auto �6$   �� 5$      $
��Auto �6$   �� 5%      $
��Auto �6$   �� 5&      $
��Auto �6$   �� 5'      $
��Auto �6$   .+�5*      � $��0��6   $��0��6'   $��5q�64   $��2��6?    5+      �� 5,      �� 5-       5/      �� 50      $	��470 �6+   �� 51      $��45 �6+   �� 52      $��45 �6+   �� 53      $��45 �6+   .+�55      �� $��0��6!   $��0��61   $��5q�6>   .+�56      ��  �� Orientation$ 
Horizontal=�6%   .+�57      �� $��0,0,58,0q�6$   $&�� ../Assets/Stone/Symbol/KingW.png��66   $��32��6`   $��32��6k   �� 59      -   5        $��buttonPlayerWhiteEngine��59   %   $
��Left=�6D   $	��370��6f      SelectWhitePlayerButton4  .System.Windows.Automation.AutomationProperties   HelpText# Z 5;   %   .��5<   "   �� -   5        $ ��textBlockPlayerWhiteEngine��5<   7   $��5q�6,   6[   5=      5>      �� 5?      $ 
Horizontal=�6%   .+�5A      �� -   5        $��textBlockPonderWhite��5A   (   $��90,0,5,0q�6D   $��Center=�6V   	Ponder:6z   �� 5B      -   5        $��imagePonderWhite��5B   6   $��5,0,10,0q�6$   $��16��6N   $��16��6Y   $��../Assets/Icons/tick.png��5C   $   $y�Visible=�6F   �� 5D      -   5        $��imagePonderWhite2��5D   6   $��5,0,10,0q�6$   $��16��6O   $��16��6Z   $ ��../Assets/Icons/delete.png��5E   $   $y�	Collapsed=�6H   �� 5F      -   5        $��textBlockEloWhite��5F   (   $��5,0,18,0q�6A   $��70��6S   $��Center=�6^   	Elo: 33336�   �� 5G      -   5        $��imageBookWhite��5G   5   $��5,0,0,0q�6$   $��24��6K   $��24��6V   $#��../Assets/Icons/book_open.png��5H   $   �� 5I      -	   5        $��imageBookWhite2��5I   5   $��5,0,0,0q�6$   $��24��6L   $��24��6W   $��../Assets/Icons/book.png��5J   $   $y�	Collapsed=�6F   �� 5K      -
   5        $��
buttonSwap��5K   %      SwitchColor � ToolTip# Z 67   $��	100,1,0,0q�6d   $��24��5L   %   
������ 
������ # Z 5N   %   .��5O   "   �� $ 
Horizontal=�6-   .+�5P   &   �� $��5,0,0,0q�6,   $'��!../Assets/Icons/arrow_refresh.png��6=   5Q   #   5R      5S      5U      �� 5V      -   5        $��buttonConfigureWhite��5V      $��5q�69   $��1��6D   $��0��6T   $	��Top=�6a   $
��Left=�5W      
������ 
������    	Configure# Z 5X   6   .��5Y      �� $��32��6    $��32��6,   $��../Assets/Icons/cog.png��67   5Z      �� 5[      -   5        $��buttonPlayerWhite��5[      $��5q�66   $��2��6A   $��0��6Q   $	��Top=�6^   $
��Left=�5\      
������ 
������    SelectPlayer# Z 5]   6   !   DirectSelectionPlayerWhite# Z 5^      .��5_      �� $��32��6    $��32��6,   $)��#../Assets/Icons/user_silhouette.png��67   5`      �� 5a      -   5        $��buttonPlayerWhiteBCS��5a      $��5q�69   $��3��6D   $��0��6T   $	��Top=�6a   $
��Left=�5b      
������ 
������ # Z 5c   6   # Z 5d      .��5e      �� $��32��6    $��32��6,   $#��../Assets/BearChessServer.png��67   5f      �� 5g      $��0��6!   $��1��61   $��5q�6>   .+�5h      �� $ 
Horizontal=�6%   .+�5i      �� $��0,0,58,0q�6$   $&�� ../Assets/Stone/Symbol/KingB.png��66   $��32��6`   $��32��6k   �� 5k      -   5        $��buttonPlayerBlackEngine��5k   %   $
��Left=�6D   $	��370��6f      SelectBlackPlayerButton# Z 5m   %   .��5n   "   �� -   5        $ ��textBlockPlayerBlackEngine��5n   7   $��5q�6,   6[   5o      5p      �� 5q      $ 
Horizontal=�6%   .+�5r      �� -   5        $��textBlockPonderBlack��5r   (   $��90,0,5,0q�6D   $��Center=�6V   	Ponder:6z   �� 5s      -   5        $��imagePonderBlack2��5s   6   $��5,0,10,0q�6$   $��16��6O   $��16��6Z   $ ��../Assets/Icons/delete.png��5t   $   $y�	Collapsed=�6H   �� 5u      -   5        $��imagePonderBlack��5u   6   $��5,0,10,0q�6$   $��16��6N   $��16��6Y   $��../Assets/Icons/tick.png��5v   $   $y�Visible=�6F   �� 5w      -   5        $��textBlockEloBlack��5w   (   $��5,0,18,0q�6A   $��70��6S   $��Center=�6^   	Elo: 33336�   �� 5x      -   5        $��imageBookBlack��5x   5   $��5,0,0,0q�6$   $��24��6K   $��24��6V   $#��../Assets/Icons/book_open.png��5y   $   �� 5z      -   5        $��imageBookBlack2��5z   5   $��5,0,0,0q�6$   $��24��6L   $��24��6W   $��../Assets/Icons/book.png��5{   $   $y�	Collapsed=�6F   5}      5      �� 5�      -   5        $��buttonConfigureBlack��5�      $��5q�69   $��1��6D   $��1��6T   $	��Top=�6a   $
��Left=�5�      
������ 
������ # Z 5�   6   .��5�      �� $��32��6    $��32��6,   $��../Assets/Icons/cog.png��67   5�      �� 5�      -   5        $��buttonPlayerBlack��5�      $��5q�66   $��2��6A   $��1��6Q   $	��Top=�6^   $
��Left=�5�      
������ 
������ # Z 5�   6   !   DirectSelectionPlayerBlack# Z 5�      .��5�      �� $��32��6    $��32��6,   $)��#../Assets/Icons/user_silhouette.png��67   5�      �� 5�      -   5        $��buttonPlayerBlackBCS��5�      $��5q�69   $��3��6D   $��1��6T   $	��Top=�6a   $
��Left=�5�      
������ 
������ # Z 5�   6   # Z 5�      .��5�      �� $��32��6    $��32��6,   $#��../Assets/BearChessServer.png��67   5�      5�      �� 5�      -   5        $��stackPanelRelaxed��5�      $��0��66   $��1��6F   $ 
Horizontal=�6S   .+�5�      �� -   5        $��
imageTeddy��5�   E   $��15,0,0,0q�6   $��32��6.   $��32��69   $#��../Assets/Icons/BearChess.ico��5�      $y�Visible=�6C   �� 5�      .+�5�      �� -   5        $��checkBoxAlternateMove��5�   #   $	��Top=�6@   $��Center=�5�   #   $
��Left=�6F      AlternateMove# Z 5�   #   $��55,5,5,5q�5�   #   #��Z 65   5�      �� 5�      -   5        $��checkBoxRelaxed��5�   #   $��Center=�6:   $��Center=�6U   $��55,5,5,5q�5�   #      Relaxed# Z 5�   #   #��Z 5�   #   5�      �� 5�      -   5        $��checkBox2TimeControls��5�   #   $��Center=�6@   $��Center=�5�   #      SeparateTimeControl# Z 5�   #   $��55,5,5,5q�5�   #   #��Z 65   5�      5�      5�      � 5�      -   5        $��gridTimeControlEmu1��5�      $��0��62   $��2��6B   $��2��6O   $y�	Collapsed=�6c   $��5q�6z   .+�5�      �� -   5        $��stackPanelTCEmu1��5�   !   $ 
Horizontal=�69   $��5q�6R   .+�5�      �� -    5        $��textBlockTimeControlEmu1��5�   ?   $��Center=�6$   $��Hallo��6_   $��90��6l   �� 5�      -!   5        $��textBlockTimeControlEmu11��5�   f   $��Center=�6$   
���  ��$��DemiBold"�6P   $��hallo��6�   5�      5�      � 5�      -"   5        $��gridTimeControl1��5�      $��0��6/   $��2��6?   $��5q�6L   $y�Visible=�6W   .+�5�      �� -#   5        $��stackPanelTC1��5�   !   $ 
Horizontal=�66   $��5q�6O   .+�5�      �� -$   5        $��textBlockTimeControl1��5�   $      TimeControlDialog#��Z 6A   $��Center=�5�   $   $��90��6?   �� 5�      -%   5        $��comboBoxTimeControl��5�   #   $	��200��6>   $��30��6J   $��Center=�6V   .@�5�      ��  �� 
IsSelected . .��6=   Time per game6L   �� 5�      .��6+   Time per game with increment6I   �� 5�      .��6+   Time per given moves6A   �� 5�      .��6+   Average time per move6B   �� 5�      .��6+   Adapted time69   �� 5�      .��6+   Depth62   �� 5�      .��6+   Nodes62   �� 5�      .��6+   Exact time per move6@   �� 5�      .��6+   
No control67   5�      5�      5�      �� 5�      -&   5        $��borderTimePerGame��5�      $��1��62   $��2��6B   
�������$��0q�6b   $��5q�5�      $y�Visible=�5�      $
��Left=�6.   ._�5�      �� $ 
Horizontal=�6!   $
��Left=�6:   .+�5�      >   8www.SoLaNoSoft.com.BearChessWin.NumericUpDownUserControl  -'   5          Name$) #numericUpDownUserControlTimePerGame��5�   9   >   8www.SoLaNoSoft.com.BearChessWin.NumericUpDownUserControl   ValueChanged$- 'NumericUpDownUserControl_OnValueChanged��5�   9   $��40��5�   9      MinValue$ 1��5�   9      MaxValue$	 999��6F       Value$  5��6U   �� 5�      $��Center=�6    !   Minutes#��Z! 6;   5�      5�      �� 5�      -(   5        $$��borderTimePerGameWithIncrement��5�      $��1��6?   $��2��6O   
�������$��5q�5�      $��0q�5�      $y�	Collapsed=�6-   ._�5�      �� $ 
Horizontal=�6!   .+�5�        -)   5        $- 'numericUpDownUserControlTimePerGameWith��5�   9   $- 'NumericUpDownUserControl_OnValueChanged��5�   9   $��40��5�   j   $ 1��5�   9   $	 999��6F   $  5��6U   �� 5�      $��Center=�6    "   Plus#��Z" 6;     5�      -*   5        $2 ,numericUpDownUserControlTimePerGameIncrement��5�   9   $- 'NumericUpDownUserControl_OnValueChanged��5�   9   $��40��5�   9   $ 1��5�   9   $ 60��6F   $  3��6T   �� 5�      $��Center=�6    #   SecondsPerMove#��Z# 6;   5�      5�      �� 5�      -+   5        $��borderTimePerGivenMoves��5�      $��1��68   $��2��6H   
�������$��0q�5�      $��5q�6-   $y�	Collapsed=�5�      ._�5�      �� $ 
Horizontal=�6!   .+�5�        -,   5        $/ )numericUpDownUserControlTimePerGivenMoves��5�   9   $- 'NumericUpDownUserControl_OnValueChanged��5�   9   $��40��5�   l   $ 1��5�   9   $	 999��6F   $  1��6U   �� 5�      $��Center=�6    $   MovesIn#��Z$ 6;     5�      --   5        $3 -numericUpDownUserControlTimePerGivensMovesMin��5�   9   $- 'NumericUpDownUserControl_OnValueChanged��5�   9   $��40��5�   9   $ 1��5�   9   $	 999��6F   $  1��6U   �� 5�      $��Center=�6    #��Z! 6;   5�      5�      �� 5�      -.   5        $��borderAverageTimePerMove��5�      $��1��69   $��2��6I   
�������$��0q�5�      $��5q�6-   $y�	Collapsed=�5�      ._�5�      �� $ 
Horizontal=�6!   .+�5�        -/   5        $) #numericUpDownUserControlAverageTime��5�   9   $- 'NumericUpDownUserControl_OnValueChanged��5�   9   $��40��5�   f   $ 1��5�   9   $	 999��6F   $  3��6U   	� 5�      -0   5        $��radioButtonSecond��5�   &   $��10,0,0,0q�6?   % d� 	IsChecked$
% TrueY�6Q   $��Center=�5�   &   & 	� 	GroupName$& averageTime��6A   
'   Sec#��Z' 5�   &   	� 5      -1   5        $��radioButtonMinute��5   &   $��10,0,0,0q�6?   $��Center=�6Q   $& averageTime��5  &   #��Z! 6>   5     5     �� 5     -2   5        $��borderDepth��5     $��1��6,   $��2��6<   
�������$��0q�6\   $��5q�5     $y�	Collapsed=�5     $
��Left=�60   ._�5     �� $ 
Horizontal=�6!   $
��Left=�6:   .+�5       -3   5        $# numericUpDownUserControlDepth��5  9   $- 'NumericUpDownUserControl_OnValueChanged��5	  9   $��40��5  `   $ 1��5  9   $	 999��6F   $  1��6U   �� 5     $��Center=�6    (   Plies#��Z( 6;   5     5     �� 5     -4   5        $��borderNodes��5     $��1��6,   $��2��6<   
�������$��0q�6\   $��5q�5     $y�	Collapsed=�5     $
��Left=�60   ._�5     �� $ 
Horizontal=�6!   $
��Left=�6:   .+�5       -5   5        $# numericUpDownUserControlNodes��5  9   $- 'NumericUpDownUserControl_OnValueChanged��5  9   $��40��5  `   $ 1��5  9   $ 	999999999��6F   $  1��6[   �� 5     $��Center=�6    )   Nodes#��Z) 6;   5     5     �� 5     -6   5        $��borderExactTime��5     $��1��60   $��2��6@   
�������$��0q�6`   $��5q�5     $y�	Collapsed=�5     $
��Left=�60   ._�5     �� $ 
Horizontal=�6!   $
��Left=�6:   .+�5       -7   5        $' !numericUpDownUserControlExactTime��5  9   $- 'NumericUpDownUserControl_OnValueChanged��5  9   $��40��5  d   $ 1��5!  9   $	 999��6F   $  1��6U   �� 5"     $��Center=�6    #��Z' 6;   5#     5$     �� 5&     -8   5        $��textBlockTCMessChess2��5&  C   
���  ��$��DemiBold"�6-   $��Center=�5'     $��0��67   $��3��6G   $��2��6T   $y�Hidden=�5(     $��Hallo��60   5)     � 5*     -9   5        $��gridTimeControlEmu2��5*     $��0��62   $��3��6B   $��2��6O   $y�	Collapsed=�6c   $��5q�6z   .+�5+     �� -:   5        $��stackPanelTCEmu2��5+  !   $ 
Horizontal=�69   $��5q�6R   .+�5,     �� -;   5        $��textBlockTimeControlEmu2��5,  =   $��Center=�6"   $��Time control ♚:��6]   $��90��6{   �� 5-     -<   5        $��textBlockTimeControlEmu22��5-  d   $��Center=�6"   
���  ��$��DemiBold"�6N   $��hallo��6�   5.     5/     � 50     -=   5        $��gridTimeControl2��50     $��0��6/   $��3��6?   $y�Hidden=�6L   $��5q�6`   .+�51     �� ->   5        $��stackPanelTC2��51  !   $ 
Horizontal=�66   $��5q�6O   .+�52     �� -?   5        $��textBlockTimeControl2��52  ?   $��Center=�6$   $��Time control ♚:��53  $   $��90��6B   54     �� 56     -@   5        $��comboBoxTimeControl2��56  #   $	��200��6?   $��30��6K   $��Center=�6W   .@�58     ��  . .��6=   Time per game6L   �� 59     .��6+   Time per game with increment6I   �� 5:     .��6+   Time per given moves6A   �� 5;     .��6+   Average time per move6B   �� 5<     .��6+   Adapted time69   �� 5=     .��6+   Depth62   �� 5>     .��6+   Nodes62   �� 5?     .��6+   Exact time per move6@   �� 5@     .��6+   
No control67   5A     5B     5C     �� 5D     -A   5        $��borderTimePerGame2��5D     $��1��63   $��3��6C   
�������$��0q�6c   $��5q�5E     $y�	Collapsed=�5F     $
��Left=�60   ._�5G     �� $ 
Horizontal=�6!   $
��Left=�6:   .+�5H       -B   5        $* $numericUpDownUserControlTimePerGame2��5H  9   $- 'NumericUpDownUserControl_OnValueChanged��5I  9   $��40��5H  g   $ 1��5K  9   $	 999��6F   $  5��6U   �� 5L     $��Center=�6    #��Z! 6;   5M     5N     �� 5O     -C   5        $%��borderTimePerGameWithIncrement2��5O     $��1��6@   $��3��6P   
�������$��5q�5P     $��0q�5Q     $y�	Collapsed=�6-   ._�5R     �� $ 
Horizontal=�6!   .+�5S       -D   5        $. (numericUpDownUserControlTimePerGameWith2��5S  9   $��40��6k   $ 1��5U  9   $	 999��6F   $  5��6U   �� 5V     $��Center=�6    #��Z" 6;     5W     -E   5        $3 -numericUpDownUserControlTimePerGameIncrement2��5W  9   $- 'NumericUpDownUserControl_OnValueChanged��5X  9   $��40��5Y  9   $ 1��5[  9   $ 60��6F   $  3��6T   �� 5\     $��Center=�6    #��Z# 6;   5]     5^     �� 5_     -F   5        $��borderTimePerGivenMoves2��5_     $��1��69   $��3��6I   
�������$��0q�5`     $��5q�6-   $y�	Collapsed=�5a     ._�5b     �� $ 
Horizontal=�6!   .+�5c       -G   5        $0 *numericUpDownUserControlTimePerGivenMoves2��5c  9   $- 'NumericUpDownUserControl_OnValueChanged��5d  9   $��40��5c  m   $ 1��5f  9   $	 999��6F   $  1��6U   �� 5g     $��Center=�6    #��Z$ 6;     5h     -H   5        $4 .numericUpDownUserControlTimePerGivensMovesMin2��5h  9   $- 'NumericUpDownUserControl_OnValueChanged��5i  9   $��40��5j  9   $ 1��5l  9   $	 999��6F   $  1��6U   �� 5m     $��Center=�6    #��Z! 6;   5n     5o     �� 5p     -I   5        $��borderAverageTimePerMove2��5p     $��1��6:   $��3��6J   
�������$��0q�5q     $��5q�6-   $y�	Collapsed=�5r     ._�5s     �� $ 
Horizontal=�6!   .+�5t       -J   5        $* $numericUpDownUserControlAverageTime2��5t  9   $- 'NumericUpDownUserControl_OnValueChanged��5u  9   $��40��5t  g   $ 1��5w  9   $	 999��6F   $  3��6U   	� 5x     -K   5        $��radioButtonSecond2��5x  &   $��10,0,0,0q�6@   $
% TrueY�6R   $��Center=�5y  &   $& averageTime2��6A   #��Z' 5z  &   	� 5{     -L   5        $��radioButtonMinute2��5{  &   $��10,0,0,0q�6@   $��Center=�6R   $& averageTime2��5|  &   #��Z! 6?   5}     5~     �� 5     -M   5        $��borderDepth2��5     $��1��6-   $��3��6=   
�������$��0q�6]   $��5q�5�     $y�	Collapsed=�5�     $
��Left=�60   ._�5�     �� $ 
Horizontal=�6!   $
��Left=�6:   .+�5�       -N   5        $$ numericUpDownUserControlDepth2��5�  9   $- 'NumericUpDownUserControl_OnValueChanged��5�  9   $��40��5�  a   $ 1��5�  9   $	 999��6F   $  1��6U   �� 5�     $��Center=�6    #��Z( 6;   5�     5�     �� 5�     -O   5        $��borderNodes2��5�     $��1��6-   $��3��6=   
�������$��0q�6]   $��5q�5�     $y�	Collapsed=�5�     $
��Left=�60   ._�5�     �� $ 
Horizontal=�6!   $
��Left=�6:   .+�5�       -P   5        $$ numericUpDownUserControlNodes2��5�  9   $- 'NumericUpDownUserControl_OnValueChanged��5�  9   $��40��5�  a   $ 1��5�  9   $ 	999999999��6F   $  1��6[   �� 5�     $��Center=�6    #��Z) 6;   5�     5�     �� 5�     -Q   5        $��borderExactTime2��5�     $��1��61   $��3��6A   
�������$��0q�6a   $��5q�5�     $y�	Collapsed=�5�     $
��Left=�60   ._�5�     �� $ 
Horizontal=�6!   $
��Left=�6:   .+�5�       -R   5        $( "numericUpDownUserControlExactTime2��5�  9   $- 'NumericUpDownUserControl_OnValueChanged��5�  9   $��40��5�  e   $ 1��5�  9   $	 999��6F   $  1��6U   �� 5�     $��Center=�6    #��Z' 6;   5�     5�     �� 5�     $��4��6   $��1��6*   .+�5�     �� -S   5        $��checkBoxTournamentMode��5�     $��5,5,5,5q�6=   $��Center=�6N   *   TournamentMode# Z* 5�     #��Z* 5�     5�     �� 5�     -T   5        $��checkBoxAllowTakeMoveBack��5�     $��5,5,5,5q�6@   $��Center=�6Q   +   AllowToTakeBack#��Z+ 5�     # Z+ 5�     5�     5�     �� 5�     -U   5        $��stackPanelExtraTime��5�     $ 
Horizontal=�68   $��0��6Q   $��4��6a   $��10,5,5,5q�5�     $	��Top=�6/   .+�5�     �� $��Center=�6   ,   ExtraTimeForHuman#��Z, 67     5�     -V   5        $  numericUpDownUserExtraTime��5�  5   $- 'NumericUpDownUserControl_OnValueChanged��5�  5   $��40��5�  Y   $ 0��6e   $	 999��5�  5   $  0��6D   �� 5�     $��Center=�6   #��Z! 67   5�     �� 5�     -W   5        $#��checkBoxStartAfterMoveOnBoard��5�     $��Center=�6@   $��5q�6[   $��1��6f   -   StartClockAfterMove# Z- 5�     $��5��5�     .��5�     �� $
��Wrap=�6    #��Z- 64   6h   5�     � 5�     $��0��6   $��6��6'   $��5q�64   .+�5�     �  5�     �� 5�     $��90 �6/   �� 5�     $
��Auto �6/   .+�5�     �� $��Center=�6$   $
��10,5q�6?   $��0��6M   .   	StartFrom#��Z. 5�  $   �� 5�     -X   5        $��stackPanelStartFrom��5�  %   $��Center=�6@   $��5q�6[   $��1��6f   .+�5�     	� -Y   5        $��radioButtonStartPosition��5�  *   $��0q�6J   $& start��6U   $
% TrueY�6g   /   BasePosition#��Z/ 5�  *   	� 5�     -Z   5        $ ��radioButtonCurrentPosition��5�  *   $��0,5,0,0q�6L   $& start��6]   0   CurrentPosition#��Z0 5�  *   	� 5�     -[   5        $��radioButtonContinueGame��5�  *   $��0,5,0,0q�6I   $& start��6Z   1   ContinueTheGame#��Z1 5�  *   5�     5�     5�     5�     5�     ]� 5�  
   $��0��6   $��1��6$   ��.  $��5q�6G   .+�5�     �� -\   5        $��buttonOk��5�     $��15,5,5,5q�6>   $
��Left=�6P   	2   Ok# Z2 5�     # Z2 5�     
������ 
������ .��5�     �� $'��!../Assets/Icons/accept_button.png��6   $��32��6C   $��32��6N   5�     �� 5�     -]   5        $��
buttonSave��5�     $��15,5,5,5q�6B   $
��Left=�6T   3   SaveConfigAsStartup# Z3 5�     
������ 
������ # Z3 5�     .��5�     �� $$��../Assets/Icons/layer_save.png��6   $��32��6@   $��32��6K   5�     �� 5�     -^   5        $��
buttonOpen��5�     $��15,5,5,5q�6B   $
��Left=�6T   4   LoadStartupGameConfig# Z4 5�     
������ 
������ # Z4 5�     .��5�     �� $$��../Assets/Icons/layer_open.png��6   $��32��6@   $��32��6K   5�     �� 5�     -_   5        $��buttonCancel��5�     $��5,5,20,5q�6F   $��Right=�6X   5   Cancel# Z5 5�     # Z5 5�     
������ 
������ .��5�     �� $ ��../Assets/Icons/cancel.png��6   $��32��6<   $��32��6G   5�     5�     5�     5�     