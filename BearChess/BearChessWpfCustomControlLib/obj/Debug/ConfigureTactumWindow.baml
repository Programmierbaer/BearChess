   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   q=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   K    Ewww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigureTactumWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   -   5          � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ Height=�6T   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   ConfigureTactum � Title# Z 5	      $	��330��66   $	��430��6C    � Icon$* $Assets/Icons/tabutronic_logo_def.png��6O   .��5
      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $
��Auto �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      Current#��Z 6D   $��DemiBold"�6j   $
��Left=�5      $��Center=�6/   �� 5      -   5        $��textBlockCurrentPort��5      $��1��68   $��0��6H   $��5q�6U   $
��COM4��6`   $��Center=�5      $��Center=�61   �� 5      $��0��6   $��1��6,   $��5q�69   	   ComPorts#��Z	 6D   $��DemiBold"�6k   $
��Left=�5      $��Center=�6/   �� 5      -   5        $��comboBoxComPorts��5   ,   $��5,5,5,5q�6   $��1��6D   $��1��6T   $��Center=�5      $	��100��60   $��Center=�5       $��Center=�67   $��Center=�5!      $��32��65   �� 5"      -   5        $��buttonCheck��5"      $��2��6,   $��1��6<   $��10,5,5,5q�6I   $	��130��6[   $��Right=�5#   -   $��Center=�6I   
   CheckSelectedCOMPortTip � ToolTip# Z
 6d   $y�Visible=�5$      .��5%      ��  �� Orientation$ 
Horizontal=�6!   .+�5&      �� -   5        $��textBlockCheck��5&   $   $��Center=�6:      Check#��Z 6U   6{   �� 5'      $��Assets/Icons/connect.png��6    $��32��6B   5(      5)      �� 5*      -   5        $��textBlockInformation��5*      $��5,5,5,5q�68   $��0��6I   $��2��6Y   $y�	Collapsed=�5+      $
��Wrap=�6+   
���\\��   NoCOMPortDetected#��Z 5,   +   $��3��5-      $	��Top=�6(   $��DemiBold"�6@   5/      50      �� 52   
   -   5        $��borderMoveLine��52      $y�Visible=�6'   $��5q�6<   
�������$��1q�6Z   ._�53      �� .+�54      �� -   5        $��checkBoxSayLiftUpDown��54   ]   $��Center=�6   $y�Visible=�6=   $��5q�6R      SayLiftUpDown#��Z 6z   6�   �� 55      -	   5        $��checkBoxPossibleMoves��55   ^   $��Center=�6   $y�Visible=�6>   $��5q�6S       SayAllMovesSelectedFigure#��Z 6{   6�   �� 56      -
   5        $��checkBoxHelpRequest��56   ^   $��Center=�6   $y�Visible=�6>   $��5q�6S      SayBestMoveHelpRequested#��Z 6z   6�   �� 57      -   5        $��checkBoxBestMove��57   ^   $��Center=�6   $y�Visible=�6>   $��5q�6S       SayBestMoveSelectedFigure#��Z 6w   6�   58      59      � 5:   
    5;      �� 5<      $��70 �6#   �� 5=      $	��120 �6#   �� 5>      $��* �6#    5A      �� 5B      $
��Auto �6    .+�5F      �� -   5        $��buttonOk��5F   X   $��0��6   $��0��6%   $��5q�62   $
��Left=�6=   $��32��6h   $��32��5G      	   Ok# Z 6   
������ 
������ .��5I      �� $$��Assets/Icons/accept_button.png��6   5J      �� 5K      -   5        $��buttonCancel��5K   Y   $��2��6   $��0��6%   $��5q�62   $��Right=�6=   $��32��6m   $��32��5L         Cancel# Z 6   
������ 
������ .��5N      �� $��Assets/Icons/cancel.png��6   5O      5P      5R      5T      