   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089M4clr-namespace:System.Globalization;assembly=mscorlibSystem.Globalization  5   	   G CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin 5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties 5   	   4   .www.SoLaNoSoft.com.BearChessWin.DatabaseWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Cgl4clr-namespace:System.Globalization;assembly=mscorlib     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   -   5          � WindowStyle$  SingleBorderWindow=�5	   =    � Icon$" ../Assets/Icons/database.png��5
   	   $	��930��6-    � Title$ Games��5   	   $	��450��6   $	��930��6$    � 	Resources 5      @  :www.SoLaNoSoft.com.BearChessWin.GamesValueToBrushConverter   ValueToBrushConverter��%   &	          5   
   62   5       J� CommandBindings 5      )  #System.Windows.Input.CommandBinding  5   
   -   5           Command$
 Copy��5      6s   .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $��dataGridGames��5      $��0��6@   $��5q�6M      AutoGenerateColumns .   %� IsVirtualizing . 	 %� VirtualizationMode$	 	Recycling=�5      
   SelectionMode$
 Extended=�5         
IsReadOnly . ��5   (   ��  5       �� Color��%�   (��       5   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� ,     5   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5      $ CornflowerBlue��5   "   �� 5      $ White��6i   5       � ContextMenu 5       �� 5!      .@�5"      m� -   5        $��menuItemContinue��5"   f   :  4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   LoadAndContinue#��Z 6   $y�	Collapsed=�6O    m� Icon 5#      �� 5$      $+��%../Assets/Icons/control_play_blue.png��6$   5&      m� 5'      -   5        $��menuItemDelete��5'   F      Delete#��Z 6    5(      �� 5)      $��../Assets/Icons/bin.png��6$   5+      m� 5,      -   5        $��menuItemCopy��5,   _    m� Command$
 Copy��6      CopyToClipboard#��Z 6/    5-      �� 5.      $,��&../Assets/Icons/clipboard_sign_out.png��6$   50      m� 51      -   5        $��menuItemDeleteDuplicates��51   R      DeleteMyDuplicates#��Z 6    52      �� 53      $0��*../Assets/Icons/database_repeat_delete.png��6$   55      56         RowStyle 58      �� 59       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6   .�5:      ��  �� Property � �� �� Value 5;      �� 5<       �� Path$ GameHash�6&    �� 	Converter# [  66   5>      5?         Columns 5A      0  *System.Windows.Controls.DataGridTextColumn  5B      -   5        dataGridTextColumnDuplicate��,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 5B   J      FontSize$ 13Q�6W   1	  +System.Windows.Controls.DataGridBoundColumn 	  ElementStyle 5C      �� 5D      " ��6    .�5E      ��  � �� 5F   %   �� *	PgnHash+# [  5G         Header$ =��5I   M   5J        5K      $
 Auto 6?   $ 13Q�6L    	  Binding 6%   �� *White+ 5L      �� 5M          White#��Z  6$   6J   5O        5P      $
 Auto 6B   $ 13Q�6O    6%   �� *
WhiteElo+ 5Q      �� 5R      
!   Elo#��Z! 6$   6H   5T        5U      $
 Auto 6?   $ 13Q�6L    6%   �� *Black+ 5V      �� 5W      "   Black#��Z" 6$   6J   5Y        5Z      $
 Auto 6B   $ 13Q�6O    6%   �� *
BlackElo+ 5[      �� 5\      #��Z! 6$   6H   5^        5_      $
 Auto 6C   $ 13Q�6P    6%   �� *	GameEvent+ 5`      �� 5a      #   Event#��Z# 6$   6J   5c        5d      $
 Auto 6B   $ 13Q�6O    6%   �� *
GameSite+ 5e      �� 5f      $   Site#��Z$ 6$   6I   5h        5i      $
 Auto 6?   $ 13Q�6L    6%   �� *Round+ 5j      �� 5k      %   Round#��Z% 6$   6J   5m        5n      $
 Auto 6@   $ 13Q�6M    6%   �� *Result+ 5o      �� 5p      &   Result#��Z& 6$   6K   5r        5s      $
 Auto 5u      $ 13Q�6"    5t      �� *
GameDate+' �� StringFormat$' d��( �� CurrentCulture) �� ConverterCulture#) Z(  5v      �� 5w      *   Date#��Z* 6$   6I   5y        5z      $
 Auto 6B   $ 13Q�6O    6%   �� *
MoveList+ 5{      �� 5|      +   Moves#��Z+ 6$   6J   5~      5�      �� 5�   
   $��5q�6   $��1��6    , �� Orientation$, 
Horizontal=�6-   .+�5�      �� -	   5        $��buttonNewFolder��5�   6   $��5q�6   $
��Left=�6    
������ 
������ -   NewDatabase. � ToolTip#. Z- 5�   H   .��5�      �� $&�� ../Assets/Icons/database_add.png��6   $��32��6B   $��32��6M   5�      �� 5�      -
   5        $��buttonFileManager��5�   6   $��5q�6   $
��Left=�6    
������ 
������ /   OpenDatabase#. Z/ 5�   H   .��5�      �� $)��#../Assets/Icons/folder_database.png��6   $��32��6E   $��32��6P   5�      �� 5�      -   5        $��buttonDelete��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 0   
DeleteGame#. Z0 5�   H   .��5�      �� $��../Assets/Icons/bin.png��6   $��32��69   $��32��6D   5�      �� 5�      -   5        $��buttonImport��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 1   ImportGames#. Z1 5�   H   .��5�      �� $'��!../Assets/Icons/saved_imports.png��6   $��32��6C   $��32��6N   5�      �� 5�      -   5        $��
buttonCopy��5�   =   $��15,5,5,5q�6   $
��Left=�6'   $
��Copy��6O   
������ 
������ #. Z 5�   H   .��5�      �� $,��&../Assets/Icons/clipboard_sign_out.png��6   $��32��6H   $��32��6S   5�      �� 5�      -   5        $��buttonDeleteDuplicates��5�   S   $��15,5,5,5q�6   $
��Left=�6'   $y�Visible=�6>   
������ 
������ 2   DeleteAllDuplicates#. Z2 5�   H   .��5�      �� $0��*../Assets/Icons/database_repeat_delete.png��6   $��32��6L   $��32��6W   5�      �� 5�      -   5        $��
buttonTwic��5�   S   $��15,5,5,5q�6   $
��Left=�6'   $y�Visible=�6>   
������ 
������ 3   
ManageTWIC#. Z3 5�   ?   .��5�      �� $/��)../Assets/Icons/outlook_calendar_week.png��6   $��32��6K   $��32��6V   5�      �� 5�      -   5        $��buttonContinue��5�   R   $��15,5,5,5q�6   $
��Left=�6'   $y�Hidden=�6>   
������ 
������ #. Z 5�   H   .��5�      �� $+��%../Assets/Icons/control_play_blue.png��6   $��32��6G   $��32��6R   5�      �� 5�      -   5        $��
buttonSync��5�   <   $��5,5,5,5q�6   $
��Left=�6&   
������ 
������ 4   SynchronizeBoard#. Z4 5�   H   .��5�      �� $, 
Horizontal=�6   .+�5�      �� -   5        $��imageLinkClear��5�      $$��../Assets/Icons/link_break.png��62   $��32��6Z   $��32��6e   $y�	Collapsed=�5�      �� 5�      -   5        $��imageLinkApply��5�      $��../Assets/Icons/link.png��62   $��32��6T   $��32��6_   5�      5�      �� 5�      -   5        $��buttonFilter��5�   =   $��15,5,5,5q�6   $
��Left=�6'   
������ 
������ 5   FilterGames#. Z5 5�   H   .��5�      �� $, 
Horizontal=�6   .+�5�      �� -   5        $��imageTableFilter��5�      $&�� ../Assets/Icons/table_filter.png��64   $��32��6^   $��32��6i   5�      5�      �� 5�      -   5        $��imageTableFilterActive��5�      $ ��../Assets/Icons/filter.png��62   $��16��6V   $��16��6a   $y�Hidden=�5�      6   FilterIsActive#. Z6 6(   �� 5�      -   5        $��buttonExport��5�   <   $��5,5,5,5q�6   $
��Left=�6&   
������ 
������ 7   ExportSelectedGames#. Z7 5�   H   .��5�      �� $&�� ../Assets/Icons/text_exports.png��6   $��32��6B   $��32��6M   5�      �� 5�      -   5        $��buttonCompressDb��5�   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������ 8   CompressDatabase#. Z8 5�   H   .��5�      �� $*��$../Assets/Icons/database_cleanup.png��6   $��32��6F   $��32��6Q   5�      �� 5�      -   5        $��buttonSaveDb��5�   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������ 9   SaveDatabase#. Z9 5�   H   .��5�      �� $'��!../Assets/Icons/database_save.png��6   $��32��6C   $��32��6N   5�      �� 5�      -   5        $��buttonRestoreDb��5�   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������ :   RestoreDatabase#. Z: 5�   H   .��5�      �� $*��$../Assets/Icons/database_refresh.png��6   $��32��6F   $��32��6Q   5�      �� 5�      -   5        $��buttonDeleteDb��5�   >   $��5,5,50,5q�6   $��Right=�6'   
������ 
������ ;   DeleteDatabase#. Z; 5�   H   .��5�      �� $)��#../Assets/Icons/database_delete.png��6   $��32��6E   $��32��6P   5�      �� 5�      -   5        $��
buttonExit��5�   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������ <   Exit#. Z< 5�   H   .��5�      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   5�      5�      5�      5�      