   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089M4clr-namespace:System.Globalization;assembly=mscorlibSystem.Globalization 5   	   Q-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5	   	   0    *www.SoLaNoSoft.com.BearChessWin.TwicWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   Cgl4clr-namespace:System.Globalization;assembly=mscorlib     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5	   	     � WindowStartupLocation$  CenterOwner=�5
   	    � WindowStyle$ SingleBorderWindow=�5       � Icon$/ )../Assets/Icons/outlook_calendar_week.png��6;    � Title$ The Week in Chess TWIC��5   	   $	��450��6(   $	��800��65   .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $��dataGridDownloads��5      $��0��6D   $��5q�6Q      AutoGenerateColumns .   %� IsVirtualizing .  %� VirtualizationMode$ 	Recycling=�5         SelectionMode$ Extended=�5      	   
IsReadOnly	 . ��5   ,   �� 
 � 	Resources
 5       �� Color��%�   (��       5   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� ,     5   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5      $ CornflowerBlue��5   "   �� 5      $ White��6i   5       � ContextMenu 5      �� 5      .@�5      m� -   5        $��menuItemDelete��5   F   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   Delete#��Z 6    m� Icon 5      �� 5      $��../Assets/Icons/bin.png��6$   5!      5"         Columns 5$      0  *System.Windows.Controls.DataGridTextColumn  5%      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6D      FontSize$ 13Q�6Q   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *
TwicNumber+   Header 5&      �� 5'         Number#��Z 6$   6K   5)        5*      $
 Auto 6B   $ 13Q�6O    6%   �� *
FileName+ 5+      �� 5,         FileName#��Z 6$   6M   5.        5/      $
 Auto 6G   $ 13Q�6T    6%   �� *NumberOfGames+ 50      �� 51         NumberOfGames#��Z 6$   6R   53        54      $
 Auto 55   %   $ 13Q�62    54   %   �� *
FileDate+ �� StringFormat$ d�� �� CurrentCulture �� ConverterCulture# Z  56      �� 57         FileDate#��Z 6$   6M   59        5:      $
 Auto 5;   &   $ 13Q�63    5:   %   �� *
ImportDate+$ d��# Z  5<      �� 5=         
ImportDate#��Z 6$   6O   5?      5A      ]� 5B   
   $��5q�6   $��1��6   ��.  .+�5C      �� -   5        $��buttonDownloadAll��5C   <   $��5,5,5,5q�6   $
��Left=�6&   
������ 
������    DownloadAllLatest � ToolTip# Z 5D   H   .��5E      �� $"��../Assets/Icons/download.png��6   $��32��6>   $��32��6I   5F      �� 5G      $��5q�6   $��1q�6!   
�������._�5H      �� $
��Left=�6    �� Orientation$ 
Horizontal=�6/   .+�5J      �� $��5q�6    $��Center=�6,      DownloadFromNumber#��Z 6G   6z   >   8www.SoLaNoSoft.com.BearChessWin.NumericUpDownUserControl  5K      -   5           Name$,  &numericUpDownUserControlTwicNumberFrom��5L   9   >   8www.SoLaNoSoft.com.BearChessWin.NumericUpDownUserControl!   ValueChanged$;! 5NumericUpDownUserControlTwicNumberFrom_OnValueChanged��5K   Q   $��5q�69   "   Value$
" 1000��6D   #   MinValue$	# 920��5L   i   $   MaxValue$
$ 9999��5M   9   %   TextWidthProperty$% 30��6I   6b   �� 5N      $��5q�6    $��Center=�6,   &   ToNumber#��Z& 6G   6p     5O      -   5        $*  $numericUpDownUserControlTwicNumberTo��5P   9   $9! 3NumericUpDownUserControlTwicNumberTo_OnValueChanged��6v   $��5q�5O   9   $
" 1000��6D   $	# 920��5P   g   $
$ 9999��5Q   9   $% 30��6I   6b   �� 5R      -   5        $��buttonDownloadSingle��5R   D   $��5,5,5,5q�6   $
��Left=�6.   
������ 
������ '   Download# Z' 5S   L   .��5T      �� $*��$../Assets/Icons/mark_to_download.png��6   $��32��6J   $��32��6U   5U      5X      5Y      �� 5Z      -   5        $��
buttonExit��5Z   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������ (   Exit# Z( 5[   H   .��5\      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   5]      5^      5_      5`      