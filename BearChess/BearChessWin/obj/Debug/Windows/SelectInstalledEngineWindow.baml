   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5	   	   A    ;www.SoLaNoSoft.com.BearChessWin.SelectInstalledEngineWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   FbearChessWin-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5	   	   -   5          � Icon$  ../Assets/Icons/robot.png��5   	   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   LoadAndManageEngines � Title# Z 5
   	   $	��390��6=   $	��550��6J   $	��500��6W    � 	Resources 5      <   6www.SoLaNoSoft.com.BearChessWin.BoolToContentConverter   BoolToContentConverter��%   &	          5   
   6.   5      .��5      �  � RowDefinitions 5   
   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $��dataGridEngine��5      $��0��6A   $��5q�6N      AutoGenerateColumns .     SelectionMode$ Extended=�5         
IsReadOnly .  J� 	AllowDrop . ��5   )   �� 	 � ContextMenu	 5      �� 5      .@�5      m� 
   SetAs#��Z
 6   .@�5      m� -   5           Buddy#��Z 5   #    m� Icon 5      �� 5      $&�� ../Assets/Icons/user_comment.png��6$   5      m� 5      -   5        $��	BearChess��5   #    5      �� 5   "   $&�� ../Assets/Icons/user_comment.png��6(   5!      5"      5#         RowStyle 5%      �� 5&       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6   .�5'      -   5        5(       5*       �� Color��%�   (��       5+   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� (     5,   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5+      $ CornflowerBlue��6e   �� 5,      $ White��6i   5-         Columns 5.      0  *System.Windows.Controls.DataGridTextColumn  5/      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6>   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *Name+   Header 50      �� 51         Name#��Z 6$   53        54      $
 Auto 6@    6%   �� *Author+ 55      �� 56         Author#��Z 6$   58        59      $
 Auto 6u    6%   �� *	IsBuddy+ �� 	Converter# [   5:      �� 5;      #��Z 6$   5=        5>      $
 Auto 6w    6%   �� *	IsProbing+# [  $ 	BearChess��5?   U   5@      5B      ]� 5D   
   $��5q�6   $��1��6   ��.  .+�5E      �� -   5        $��buttonOk��5E   6   $��5q�6   $
��Left=�6    
������ 
������    LoadSelectedEngineTip � ToolTip# Z 6�   .��5F      �� $��../Assets/Icons/robot.png��6   $��32��6;   $��32��6F   5G      �� 5H      $��Center=�6   $��5q�63      Filter#��Z 6>   �� 5I      -   5        $��textBoxFilter��5I   R   $��Center=�6   $��5q�61   $
��Left=�6<   $��25��6�   $	��130��6�   6�   �� 5J      -   5        $��buttonCancel��5J   >   $��25,5,5,5q�6   $��Right=�6'   
������ 
������    Exit# Z 6�   .��5K      �� $"��../Assets/Icons/door_out.png��6   $��32��6>   $��32��6I   5L      �� 5M      -	   5        $��buttonDelete��5M   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������ !   UninstallSelectedEngineTip# Z 6�   .��5N      �� $��../Assets/Icons/bin.png��6   $��32��69   $��32��6D   5O      �� 5P      -
   5        $��buttonAddConfigure��5P   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������    AddNewEngineTip# Z 6�   .��5Q      �� $!��../Assets/Icons/cog_add.png��6   $��32��6=   $��32��6H   5R      �� 5T      -   5        $��buttonInstall��5T   7   $��5q�6   $��Right=�6    
������ 
������    SelectedEngineFile# Z 6�    . .��5V      �� $,��&../Assets/Icons/file_extension_exe.png��6   $��32��6H   $��32��6S   5W      �� 5Y      -   5        $��buttonConfigure��5Y   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������    ConfigureSelectedEngine# Z 6�   .��5Z      �� $"��../Assets/Icons/cog_edit.png��6   $��32��6>   $��32��6I   6W   5[      �� 5\      -   5        $��buttonBuddy��5\   =   $��5,5,5,5q�6   $��Right=�6&   
������ 
������    
SetAsBuddy# Z 6�   .��5]      �� $&�� ../Assets/Icons/user_comment.png��6   $��32��6B   $��32��6M   6[   5^      5`      5a      5b      