   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089M4clr-namespace:System.Globalization;assembly=mscorlibSystem.Globalization 5   	   Q-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5	   	   0    *www.SoLaNoSoft.com.BearChessWin.DuelWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   Cgl4clr-namespace:System.Globalization;assembly=mscorlib     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5	   	   -   5          � WindowStyle$  SingleBorderWindow=�5
      :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   Duels � Title# Z 5   	   $	��450��6.   $	��800��6;    � 	Resources 5      ?   9www.SoLaNoSoft.com.BearChessWin.DuelValueToBrushConverter   ValueToBrushConverter��%   &	          5   
   61   5      .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $��dataGridDuel��5      $��0��6?   $��5q�6L      AutoGenerateColumns .     SelectionMode$ Extended=�5         
IsReadOnly . ��5   '   ��  5       �� Color��%�   (��       5   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� (     5   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5      $ CornflowerBlue��6e   �� 5      $ White��6i   5      	 � ContextMenu	 5      �� 5      .@�5      m� -   5        $��menuItemDelete��5   F   
   Delete#��Z
 6    m� Icon 5      �� 5       $��../Assets/Icons/bin.png��6$   6G   5"      m� 5#      -   5        $��menuItemLoad��5#   L      ContinueDuel#��Z 6    5$      �� 5%      $+��%../Assets/Icons/control_play_blue.png��6$   6U   5'      m� 5(      -   5        $��menuItemClone��5(   M      LoadAsNewDuel#��Z 6    5)      �� 5*      $'��!../Assets/Icons/6-2-chess-png.png��6$   6Q   5,      m� 5-      -   5        $��menuItemRename��5-   J      
RenameDuel#��Z 6    5.      �� 5/      $&�� ../Assets/Icons/text_replace.png��6$   6P   51      52         Columns 54      0  *System.Windows.Controls.DataGridTextColumn  55      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6O      FontSize$ 13Q�6\   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *CurrentDuel.GameEvent+   Header 56      �� 57         Name#��Z 6$   6I   59        5;      $
 Auto 6E   $ 13Q�6R    6%   �� *PlayedGames+ 5<      �� 5=         PlayedGames#��Z 6$   6P   5?        5@      $
 Auto 6E   $ 13Q�6R    6%   �� *GamesToPlay+ 5A      �� 5B         GamesToPlay#��Z 6$   6P   5D        5E      $
 Auto 6?   $ 13Q�6L    6%   �� *State+ 5F      �� 5G         State#��Z 6$   6J   5I        5J      $
 Auto 6�   $ 13Q�6�    6%   �� *	EventDate+ �� StringFormat$ d�� �� CurrentCulture �� ConverterCulture# Z  5K      �� 5L         Date#��Z 6$   6I   5N        5O      $
 Auto 6F   $ 13Q�6S    6%   �� *Participants+ 5P      �� 5Q         Participants#��Z 6$   6Q   5S      5V      �� 5W   
   $��1��6   $��5,10,5,0q�6#   .+�5X      ��    GamesOfSelectedDuel#��Z 6   5Y        5Z   
   -   5        $��dataGridGames��5Z      $��2��6@   $��5q�6M    .  $ Extended=�5[       . ��5Z   (   ��  5\      ��%�   (��       5]   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� (     5^   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5]      $ CornflowerBlue��6e   �� 5^      $ White��6i   5_      	 5a      �� 5b      .@�5c      m� -   5        $��menuItemCopy��5c   O      CopyToClipboard#��Z 6    5d      �� 5e      $,��&../Assets/Icons/clipboard_sign_out.png��6$   5g      m� 5h      -	   5        $��menuItemReplay��5h   J      
RepeatGame#��Z 6    5i      �� 5j      $-��'../Assets/Icons/control_repeat_blue.png��6$   5l      5m          RowStyle  5o      �� 5p      ! �� 
TargetType)  #System.Windows.Controls.DataGridRow"!  6   .�5q      �� " �� Property" � ��# �� Value# 5r      �� 5s      $ �� Path$$ PgnHash�6&   % �� 	Converter#% [  65   5t      5v      5w       5y        5z      $
 Auto 6'   $ 13Q�64   &   ElementStyle& 5{      �� 5|      "! ��6    .�5}      �� " � ��# 6;   �� *	PgnHash+#% [  5~      $ =��5�   M   5�        5�      $
 Auto 6?   $ 13Q�6L    6%   �� *White+ 5�      �� 5�      '   White#��Z' 6$   6J   5�        5�      $
 Auto 6B   $ 13Q�6O    6%   �� *
WhiteElo+ 5�      �� 5�      
(   Elo#��Z( 6$   6H   5�        5�      $
 Auto 6?   $ 13Q�6L    6%   �� *Black+ 5�      �� 5�      )   Black#��Z) 6$   6J   5�        5�      $
 Auto 6B   $ 13Q�6O    6%   �� *
BlackElo+ 5�      �� 5�      #��Z( 6$   6H   5�        5�      $
 Auto 6C   $ 13Q�6P    6%   �� *	GameEvent+ 5�      �� 5�      *   Event#��Z* 6$   6J   5�        5�      $
 Auto 6B   $ 13Q�6O    6%   �� *
GameSite+ 5�      �� 5�      +   Site#��Z+ 6$   6I   5�        5�      $
 Auto 6?   $ 13Q�6L    6%   �� *Round+ 5�      �� 5�      ,   Round#��Z, 6$   6J   5�        5�      $
 Auto 6@   $ 13Q�6M    6%   �� *Result+ 5�      �� 5�      -   Result#��Z- 6$   6K   5�        5�      $
 Auto 6�   $ 13Q�6�    6%   �� *
GameDate+$ d��# Z  5�      �� 5�      #��Z 6$   6I   5�        5�      $
 Auto 6B   $ 13Q�6O    6%   �� *
MoveList+ 5�      �� 5�      .   Moves#��Z. 6$   6J   5�      5�      ]� 5�   
   $��5q�6   $��3��6   ��.  .+�5�      �� -
   5        $��
buttonLoad��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ / � ToolTip#/ Z 6�   .��5�      �� $+��%../Assets/Icons/control_play_blue.png��6   $��32��6G   $��32��6R   5�      �� 5�      -   5        $��buttonClone��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ #/ Z 6�   .��5�      �� $'��!../Assets/Icons/6-2-chess-png.png��6   $��32��6C   $��32��6N   5�      �� 5�      -   5        $��buttonRepeat��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 0   
RepeatDuel#/ Z0 6�   .��5�      �� $-��'../Assets/Icons/control_repeat_blue.png��6   $��32��6I   $��32��6T   5�      �� 5�      -   5        $��	buttonAdd��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 1   IncreaseNumberOfGames#/ Z1 6�   .��5�      �� $)��#../Assets/Icons/plus_light_blue.png��6   $��32��6E   $��32��6P   5�      �� 5�      -   5        $��buttonDelete��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 2   
DeleteDuel#/ Z2 6�   .��5�      �� $��../Assets/Icons/bin.png��6   $��32��69   $��32��6D   5�      �� 5�      -   5        $��buttonExport��5�   <   $��5,5,5,5q�6   $
��Left=�6&   
������ 
������ 3   ExportDuelGames#/ Z3 6�   .��5�      �� $&�� ../Assets/Icons/text_exports.png��6   $��32��6B   $��32��6M   5�      �� 5�      -   5        $��
buttonInfo��5�   <   $��5,5,5,5q�6   $
��Left=�6&   
������ 
������ 4   DuelInformation#/ Z4 6�   .��5�      �� $%��../Assets/Icons/information.png��6   $��32��6A   $��32��6L   5�      �� 5�      -   5        $��
buttonExit��5�   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������ 5   Exit#/ Z5 6�   .��5�      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   5�      �� 5�      -   5        $��buttonDeleteDb��5�   >   $��5,5,50,5q�6   $��Right=�6'   
������ 
������ 6   DeleteAllDuels#/ Z6 6�   .��5�      �� $)��#../Assets/Icons/database_delete.png��6   $��32��6E   $��32��6P   5�      5�      5�      5�      