   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   H    Bwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigureDGTWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ Height=�6T   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   ConfigureDGT � Title# Z 5	   	   $	��330��65   $	��390��6B   .��5
      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $��75 �6'   �� 5      $	��120 �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      Current#��Z 6D   $��DemiBold"�6j   $
��Left=�5      $��Center=�65   �� 5      -   5        $��textBlockCurrentPort��5      $��1��68   $��0��6H   $��5q�6U   $
��COM4��6`   $��Center=�5      $��Center=�67   �� 5      $��0��6   $��1��6,   $��5q�69      ComPorts#��Z 6D   $��DemiBold"�6k   $
��Left=�5      $��Center=�65   �� 5      -   5        $��comboBoxComPorts��5   ,   $��5,5,5,5q�6   $��1��6D   $��1��6T   $��Center=�5      $	��100��66   $��Center=�5       $��Center=�6=   $��Center=�5!      $��32��6;   �� 5"      -   5        $��buttonCheck��5"      $��2��6,   $��1��6<   $��10,5,5,5q�6I   $	��100��6[   $��Right=�5#   3   $��Center=�6O   $y�Visible=�5$      .��5%      �� 	 �� Orientation$	 
Horizontal=�6!   .+�5&      �� -   5        $��textBlockCheck��5&   $   $��Center=�6:   
   Check#��Z
 6U   6{   �� 5'      $��Assets/Icons/connect.png��6    $��32��6B   5(      5)      �� 5*      -   5        $��textBlockInformation��5*      $��5,5,5,5q�68   $��0��6I   $��2��6Y   $y�	Collapsed=�5+      $
��Wrap=�61   
���\\��   NoCOMPortDetected#��Z 5,   1   $��3��5-      $	��Top=�6.   $��DemiBold"�6F   5/      50      �� 51   
   $��5q�6   
�������$��1q�6/   ._�52      �  53      �� 54      $
��Auto �6'   66   �� 55      $
��Auto �6'   66    57      �� 58      $��30 �6$   62   �� 59      $��30 �6$   62   �� 5:      $��30 �6$   62   �� 5;      $��40 �6$   62   �� 5<      $��45 �6$   62   .+�5>      �� -   5        $��checkBoxUseClock��5>      $��5q�63   $��Center=�6>   $��0��6Y   $��0��6f   $��DemiBold"�5?         UseDGT3000Clock � ToolTip# Z 5@   >      Clock#��Z 6o   5B      �� 5C      -   5        $��checkBoxSwitchSide��5C      $��Center=�65   $��25,5,5,5q�6P   $��1��6b   $��0��5D         LeftSideShowsForWhite# Z 5E   D      
SwitchSide#��Z 6{   5G      �� 5H      -   5        $��textBlockClock��5H      $��20(�62   $	��Top=�6@   $
��Left=�6X   $��1��5I      $��5,0,0,0q�6'   $��1��68   C5K      �� 5L      -	   5        $��checkBoxShowOnlyMoves��5L      $��Center=�68   $��25,5,5,5q�6S   $��2��6e   $��0��5M         ShowOnlyMoves#��Z 6)   5N      �� 5O      -
   5        $��stackPanelStyle��5O      $��3��64   $��0��6A   $��25,5,5,5q�6Q   $	 
Horizontal=�5P      .+�5Q      �� $��Center=�6    $
��Left=�6;      Style#��Z 6V   6|   	� 5R      -   5        $��radioButtonLongFormat��5R   "    	� 	GroupName$ 
moveFormat��6?   $��Center=�6V   $
��Left=�5S       $��5,5,5,5q�6;    d� 	IsChecked$
 TrueY�6L   .��6]   e2-e45U      	� 5V      -   5        $��radioButtonShortFormat��5V   "   $ 
moveFormat��6@   $��Center=�6W   $
��Left=�5W       $��5,5,5,5q�6;   .��6L   e45Y      5Z      �� 5[      -   5        $��checkBoxUpperCase��5[      $��5,0,0,0q�64   $��3��6E   $��1��6R   $��Center=�5\         AllCharactersInUpperCase# Z 64      	UpperCase#��Z 6n   5]      �� 5^      -   5        $��checkBoxBeep��5^      $��Center=�6/   $��25,5,5,5q�6J   $��4��6\   $��0��6i      
BeepOnMove#��Z 6y   �� 5_      $��4��6   $��1��6*   $	 
Horizontal=�6:   .+�5`      �� $��Center=�6    $
��Left=�6;      Duration#��Z 6V   6   N   Hwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.NumericUpDownUserControl  5a      -   5          Name$&  numericUpDownUserControlDuration��5a   @   $��5q�65      MinValue$ 1��6j      Value$ 1��6w      MaxValue$ 32��5b   :      TextWidthProperty$ 30��6H   5c      5d      5e      5f      � 5g   
    5h      �� 5i      $��70 �6#   �� 5j      $	��120 �6#   �� 5k      $��* �6#    5n      �� 5o      $
��Auto �6    .+�5s      �� -   5        $��buttonOk��5s   X   $��0��6   $��0��6%   $��5q�62   $
��Left=�6=   $��32��6h   $��32��5t      
������ 
������ .��5v      �� $$��Assets/Icons/accept_button.png��6   5w      �� 5x      -   5        $��buttonCancel��5x   Y   $��2��6   $��0��6%   $��5q�62   $��Right=�6=   $��32��6m   $��32��5y      
������ 
������ .��5{      �� $��Assets/Icons/cancel.png��6   5|      5}      5      5�      