   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5      9    3www.SoLaNoSoft.com.BearChessWin.FicsUserUserControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5        � 	Resources  5	      <   6www.SoLaNoSoft.com.BearChessWin.BoolToContentConverter   BoolToContentConverter��%   &	          5
   
   6.   5      .��5      �� $��1q�6   
���   �._�5   
   � 
������� � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��* �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    .+�5      �� $��5q�6   $!��../Assets/Icons/users_3.png��6   $��32��6D   $��32��6O   $��0��6[   $
��Left=�6h   &   System.Windows.Controls.DataGrid  5      -   5        $��dataGridUsers��5      $��1��6D   $��5q�6Q      AutoGenerateColumns .   %� IsVirtualizing .  %� VirtualizationMode$ 	Recycling=�5         SelectionMode$ Extended=�5         
IsReadOnly . ��5   ,   ��   5       �� Color��%�   (��       5   &   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� ,     5   &   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5      $ CornflowerBlue��5   &   �� 5      $ White��5   &   5         Columns 5       0  *System.Windows.Controls.DataGridTextColumn  5!      ,  &System.Windows.Controls.DataGridColumn	   Width5  /System.Windows.Controls.DataGridLengthConverter$		 110 6F   
   FontSize$
 13Q�6R   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6)   �� *
UserName+   Header$
 Name��5"   T   5#        5$      $		 100 6~   $
 13Q�6�    6*   �� *OpenForGames+ �� 	Converter# [  $ Accept games��5%   \      ElementStyle 5&      �� 5'       �� 
TargetType" ��6$   .�5(   "   ��  �� Property � �� �� Value$ Center=�6H   �� 5)   "    � ��$
 Bold"�6?   5*      5,        5-      $		 100 6�   $
 13Q�6�    6)   �� *OnlyUnratedGames+# [  $ Only unrated��5.   \    5/      �� 50      " ��6$   .�51   "   ��  � ��$ Center=�6H   �� 52   "    � ��$
 Bold"�6?   53      55        56      $		 100 6�   $
 13Q�6�    6)   �� *UnregisteredUser+# [  $ Guest��57   U    58      �� 59      " ��6$   .�5:   "   ��  � ��$ Center=�6H   �� 5;   "    � ��$
 Bold"�6?   5<      5>        5?      $		 100 6~   $
 13Q�6�    6)   �� *ComputerUser+# [  $ Computer��5@   X    5A      �� 5B      " ��6$   .�5C   "   ��  � ��$ Center=�6H   �� 5D   "    � ��$
 Bold"�6?   5E      5G        5H      $		 100 6I   $
 13Q�6U    6)   �� *StandardElo+$ Standard��5I   X   5J        5K      $		 100 6F   $
 13Q�6R    6)   �� *
BlitzElo+$ Blitz��5L   U   5M        5N      $		 100 6J   $
 13Q�6V    6)   �� *LightningElo+$ 	Lightning��5O   Y   5P      5R      ]� 5S      $
��Left=�6   $��5q�6.   $��2��69   ��.  .+�5T      �� -   5        $��buttonQueryUser��5T   :   $��5q�6   $
��Left=�6$   
������ 
������  � ToolTip$ Query for users��5U   L   .��5V      �� $'��!../Assets/Icons/table_refresh.png��6   $��32��6G   $��32��6R   5W      �� 5X      $��5q�6   $��Center=�6'   $
��Left=�6B   User:6_   �� 5Y      -   5        $��textBoxUser��5Y      $��5q�6-   $	��200��68   $��Center=�6D   $
��Left=�6�   $ Filter user name��6�   6�   �� 5Z      -   5        $��checkBoxOpen��5Z   X    d� IsThreeState . $��15,5,5,5q�6l   $��Center=�6~   $
��Left=�6�   .��6�   Accept games6�   �� 5[      -   5        $��checkBoxComputer��5[   Y    . $��15,5,5,5q�6q   $��Center=�6�   $
��Left=�6�   .��6�   
Computer6�   �� 5\      -   5        $��checkBoxGuests��5\   Z    . $��15,5,5,5q�6p   $��Center=�6�   $
��Left=�6�   .��6�   Guests6�   5]      �� 5^      $��5q�6   $��3��6%    �� Orientation$ 
Horizontal=�62   .+�5_      �� -   5        $��	labelInfo��5_      $	��5,0q�6)   $��25��66   $��Center=�6B   6_   5`      5a      5b      5c      