   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:www.SoLaNoSoft.com.BearChessWin.UserControls,www.SoLaNoSoft.com.BearChessWin.UserControls  5      Q-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5      8    2www.SoLaNoSoft.com.BearChessWin.FicsAdsUserControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Llocal:clr-namespace:www.SoLaNoSoft.com.BearChessWin.UserControls     5      FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5        � 	Resources  5
      <   6www.SoLaNoSoft.com.BearChessWin.BoolToContentConverter   BoolToContentConverter��%   &	          5   
   6.   5      .��5      �� $��1q�6   
���   �._�5   
   � 
������� � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��* �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    .+�5      �� $��5q�6   $��0��6    $
��Left=�6-   $)��#../Assets/Icons/chessboard_base.png��6H   $��32��6u   $��32��6�   &   System.Windows.Controls.DataGrid  5      -   5        $��dataGridGamesAd��5      $��1��6F   $��5q�6S      AutoGenerateColumns .   %� IsVirtualizing .  %� VirtualizationMode$ 	Recycling=�5         SelectionMode$ Single=�5         
IsReadOnly . ��5   .   ��   5       �� Color��%�   (��       5   &   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� ,     5   &   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5      $ CornflowerBlue��5   &   �� 5      $ White��5   &   5          Columns 5!      0  *System.Windows.Controls.DataGridTextColumn  5"      ,  &System.Windows.Controls.DataGridColumn	   Width5  /System.Windows.Controls.DataGridLengthConverter$	 50 6H   
   FontSize$
 13Q�6S   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6)   �� *
GameNumber+   Header$ No��5#   R   5$        5%      $		 110 6F   $
 13Q�6R    6)   �� *
UserName+$ Username��5&   X   5'        5(      $		 110 6D   $
 13Q�6P    6)   �� *Rating+$ Rating��5)   V   5*        5+      $		 110 6I   $
 13Q�6U    6)   �� *TimeControl+$ Time (min./inc.)��5,   a   5-        5.      $		 110 6F   $
 13Q�6R    6)   �� *
GameType+$
 Type��5/   T   50        51      $		 100 6{   $
 13Q�6�    6*   �� *	RatedGame+ �� 	Converter# [  $ Rated��52   U      ElementStyle 53      �� 54       �� 
TargetType" ��6$   .�55   "   ��  �� Property � �� �� Value$ Center=�6H   �� 56   "    � ��$
 Bold"�6?   57      59        5:      $		 110 6G   $
 13Q�6S    6)   �� *	GameColor+$ Color��5;   U   5<        5=      $		 110 6I   $
 13Q�6U    6)   �� *RatingRange+$ Rating range��5>   \   5?        5@      $		 110 6G   $
 13Q�6S    6)   �� *	StartType+$ Start��5A   U   5B      5D      ]� 5E      $
��Left=�6   $��5q�6.   $��2��69   ��.  .+�5F      �� -   5        $��buttonQueryAds��5F   :   $��5q�6   $
��Left=�6$   
������ 
������  � ToolTip$$ Query for games advertisements��5G   L   .��5H      �� $'��!../Assets/Icons/table_refresh.png��6   $��32��6G   $��32��6R   5I      �� 5J      $��5q�6   $��Center=�6'   $
��Left=�6B   User:6_   �� 5K      -   5        $��textBoxUser��5K      $��5q�6-   $	��200��68   $��Center=�6D   $
��Left=�6�   6�   �� 5L      -   5        $��checkBoxRated��5L   X    d� IsThreeState . $��15,5,5,5q�6m   $��Center=�6   $
��Left=�6�   .��6�   Rated6�   �� 5M      -   5        $��checkBoxComputer��5M   Y    . $��15,5,5,5q�6q   $��Center=�6�   $
��Left=�6�   .��6�   
Computer6�   5N      �� 5O      $��5q�6   $��3��6%    �� Orientation$ 
Horizontal=�62   .+�5Q      �� -   5        $��	labelInfo��5Q      $	��5,0q�6)   $��25��66   $��Center=�6B   6_   5R      5S      5T      5U      