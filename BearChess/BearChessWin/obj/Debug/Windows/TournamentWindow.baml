   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089M4clr-namespace:System.Globalization;assembly=mscorlibSystem.Globalization 5   	   Q-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5	   	   6    0www.SoLaNoSoft.com.BearChessWin.TournamentWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   Cgl4clr-namespace:System.Globalization;assembly=mscorlib     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5	   	   -   5          � WindowStyle$  SingleBorderWindow=�5
   ?   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   EngineTournament � Title# Z 5   	   $	��450��69   $	��800��6F    � 	Resources 5      E   ?www.SoLaNoSoft.com.BearChessWin.TournamentValueToBrushConverter   ValueToBrushConverter��%   &	          5   
   67   5      .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $��dataGridTournament��5      $��0��6E   $��5q�6R      AutoGenerateColumns .     SelectionMode$ Extended=�5         
IsReadOnly . ��5   -   ��  5       �� Color��%�   (��       5   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� (     5   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5      $ CornflowerBlue��6e   �� 5      $ White��6i   5      	 � ContextMenu	 5      �� 5      .@�5      m� -   5        $��menuItemDelete��5   F   
   Delete#��Z
 6    m� Icon 5      �� 5       $��../Assets/Icons/bin.png��6$   6G   5"      m� 5#      -   5        $��menuItemLoad��5#   R      ContinueTournament#��Z 6    5$      �� 5%      $+��%../Assets/Icons/control_play_blue.png��6$   6U   5'      m� 5(      -   5        $��menuItemClone��5(   S      LoadAsNewTournament#��Z 6    5)      �� 5*      $"��../Assets/Icons/cup_gold.png��6$   6L   5,      m� 5-      -   5        $��menuItemRename��5-   P      RenameTournament#��Z 6    5.      �� 5/      $&�� ../Assets/Icons/text_replace.png��6$   6P   51      52         Columns 54      0  *System.Windows.Controls.DataGridTextColumn  55      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6U      FontSize$ 13Q�6b   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *CurrentTournament.GameEvent+   Header 56      �� 57         Event#��Z 6$   6J   59        5;      $
 Auto 6Z   $ 13Q�6g    6%   �� *" CurrentTournament.TournamentType+ 5<      �� 5=         Type#��Z 6$   6I   5?        5@      $
 Auto 6R   $ 13Q�6_    6%   �� *CurrentTournament.Cycles+ 5A      �� 5B         Cycles#��Z 6$   6K   5D        5E      $
 Auto 6E   $ 13Q�6R    6%   �� *PlayedGames+ 5F      �� 5G         PlayedGames#��Z 6$   6P   5I        5J      $
 Auto 6E   $ 13Q�6R    6%   �� *GamesToPlay+ 5K      �� 5L         GamesToPlay#��Z 6$   6P   5N        5O      $
 Auto 6?   $ 13Q�6L    6%   �� *State+ 5P      �� 5Q         State#��Z 6$   6J   5S        5T      $
 Auto 6�   $ 13Q�6�    6%   �� *	EventDate+ �� StringFormat$ d�� �� CurrentCulture �� ConverterCulture# Z  5U      �� 5V         Date#��Z 6$   6I   5X        5Y      $
 Auto 6F   $ 13Q�6S    6%   �� *Participants+ 5Z      �� 5[         Participants#��Z 6$   6Q   5]      5`      �� 5a   
   $��1��6   $��5,10,5,0q�6#    �� Orientation$ 
Horizontal=�65   .+�5b      ��      GamesOfSelectedTournament#��Z  6   6Q   �� 5c      -   5        $��labelGamesCount��5c      .��6+   06.   5d        5e   
   -   5        $��dataGridGames��5e      $��2��6@   $��5q�6M    .  $ Extended=�5f       . ��5e   (   ��  5g      ��%�   (��       5h   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� (     5i   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5h      $ CornflowerBlue��6e   �� 5i      $ White��6i   5j      	 5k      �� 5l      .@�5m      m� -	   5        $��menuItemCopy��5m   O   !   CopyToClipboard#��Z! 6    5n      �� 5o      $,��&../Assets/Icons/clipboard_sign_out.png��6$   6V   5q      m� 5r      -
   5        $��menuItemReplay��5r   J   "   
RepeatGame#��Z" 6    5s      �� 5t      $-��'../Assets/Icons/control_repeat_blue.png��6$   6W   5v      5w      #   RowStyle# 5y      �� 5z      $ �� 
TargetType)  #System.Windows.Controls.DataGridRow"$  6   .�5{      �� % �� Property% � ��& �� Value& 5|      �� 5}      ' �� Path$' PgnHash�6&   ( �� 	Converter#( [  65   5~      5�      5�       5�        5�      $
 Auto 6'   $ 13Q�64   )   ElementStyle) 5�      �� 5�      "$ ��6    .�5�      �� % � ��& 6;   �� *	PgnHash+#( [  5�      $ =��5�   M   5�        5�      $
 Auto 6?   $ 13Q�6L    6%   �� *White+ 5�      �� 5�      *   White#��Z* 6$   6J   5�        5�      $
 Auto 6B   $ 13Q�6O    6%   �� *
WhiteElo+ 5�      �� 5�      
+   Elo#��Z+ 6$   6H   5�        5�      $
 Auto 6?   $ 13Q�6L    6%   �� *Black+ 5�      �� 5�      ,   Black#��Z, 6$   6J   5�        5�      $
 Auto 6B   $ 13Q�6O    6%   �� *
BlackElo+ 5�      �� 5�      #��Z+ 6$   6H   5�        5�      $
 Auto 6C   $ 13Q�6P    6%   �� *	GameEvent+ 5�      �� 5�      #��Z 6$   6J   5�        5�      $
 Auto 6B   $ 13Q�6O    6%   �� *
GameSite+ 5�      �� 5�      -   Site#��Z- 6$   6I   5�        5�      $
 Auto 6?   $ 13Q�6L    6%   �� *Round+ 5�      �� 5�      .   Round#��Z. 6$   6J   5�        5�      $
 Auto 6@   $ 13Q�6M    6%   �� *Result+ 5�      �� 5�      /   Result#��Z/ 6$   6K   5�        5�      $
 Auto 6�   $ 13Q�6�    6%   �� *
GameDate+$ d��# Z  5�      �� 5�      #��Z 6$   6I   5�        5�      $
 Auto 6B   $ 13Q�6O    6%   �� *
MoveList+ 5�      �� 5�      0   Moves#��Z0 6$   6J   5�      5�      ]� 5�   
   $��5q�6   $��3��6   ��.  .+�5�      �� -   5        $��
buttonLoad��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 1 � ToolTip#1 Z 6�   .��5�      �� $+��%../Assets/Icons/control_play_blue.png��6   $��32��6G   $��32��6R   5�      �� 5�      -   5        $��buttonClone��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ #1 Z 6�   .��5�      �� $"��../Assets/Icons/cup_gold.png��6   $��32��6>   $��32��6I   5�      �� 5�      -   5        $��buttonRepeat��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 2   RepeatTournament#1 Z2 6�   .��5�      �� $-��'../Assets/Icons/control_repeat_blue.png��6   $��32��6I   $��32��6T   6b   5�      �� 5�      -   5        $��buttonDelete��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 3   DeleteTournamentTip#1 Z3 6�   .��5�      �� $��../Assets/Icons/bin.png��6   $��32��69   $��32��6D   6R   5�      �� 5�      -   5        $��buttonExport��5�   <   $��5,5,5,5q�6   $
��Left=�6&   
������ 
������ 4   ExportTournamentGames#1 Z4 6�   .��5�      �� $&�� ../Assets/Icons/text_exports.png��6   $��32��6B   $��32��6M   6[   5�      �� 5�      -   5        $��
buttonInfo��5�   <   $��5,5,5,5q�6   $
��Left=�6&   
������ 
������ 5   TournamentInformation#1 Z5 6�   .��5�      �� $%��../Assets/Icons/information.png��6   $��32��6A   $��32��6L   6Z   5�      �� 5�      -   5        $��
buttonExit��5�   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������ .��5�      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   6W   5�      �� 5�      -   5        $��buttonDeleteDb��5�   >   $��5,5,50,5q�6   $��Right=�6'   
������ 
������ 6   DeleteAllTournamentTip#1 Z6 6�   .��5�      �� $)��#../Assets/Icons/database_delete.png��6   $��32��6E   $��32��6P   6^   5�      5�      5�      5�      