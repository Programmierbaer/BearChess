   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   L    Fwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigurePegasusWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	     � Icon$  Assets/Icons/dgt48.PNG��5   	    � WindowStartupLocation$ CenterOwner=�5	       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ Height=�6T   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   ConfigurePegasus � Title# Z 5
   	   $	��370��69   $	��390��6F   .��5      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $��75 �6'   �� 5      $	��120 �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      Current#��Z 6D   $��DemiBold"�6j   $
��Left=�5      $��Center=�64   �� 5      -   5        $��textBlockCurrentPort��5      $��1��68   $��0��6H   $��5q�6U   $
��BTLE��6`   $��Center=�5      $��Center=�66   �� 5      -   5        $��buttonCheck��5      $��2��6,   $��0��6<   $��10,5,5,5q�6I   $	��100��6[   $��Right=�5   2   $��Center=�6N   $y�Visible=�5       .��5!      �� 	 �� Orientation$	 
Horizontal=�6!   .+�5"      �� -   5        $��textBlockCheck��5"   $   $��Center=�6:   
   Check#��Z
 6U   6{   �� 5#      $��Assets/Icons/connect.png��6    $��32��6B   5$      5%      5(      5)      �� 5*   
   -   5        $��
borderLEDs��5*      $��5q�6#   
�������$��1q�6A   ._�5+      �� .+�5,      ]� $��0,5,0,0q�6   ��.  .+�5-      �� $
��Left=�6    $��5q�66      LEDs#��Z 6A   $��DemiBold"�6d   $��70��6z   $
��Left=�5.      $��Center=�68   �� 5/      -   5        $��buttonDecrementDim��5/   (   $��0q�6   
������ $
��Left=�6[   
������ $��32��50   4   $��32��6?   .��51      �� $'��!Assets/Icons/lightbulb_delete.png��6    52      �� 54      -   5        $��	sliderDim��54   3   $
��Left=�6   $��15,5,5,5q�6D    � Minimum$ 1Q�6V    � Maximum$ 5Q�6b   $	��100��55       � Value$ 2Q�6&   56      �� 57      -   5        $��buttonIncrementDim��57   .   $��5,0,0,0q�6   
������ $
��Left=�6a   
������ $��32��58   4   $��32��6?   .��59      �� $$��Assets/Icons/lightbulb_add.png��6    5:      5;      �� 5=      $��0,10,0,5q�6   .+�5>      ]� ��.  .+�5?      �� $
��Left=�6$   $��12,5,0,5q�6:      Flash#��Z 6L   $��DemiBold"�6p   $��70��6�   $
��Left=�5@   !   $��Center=�6<   �� 5A      -   5        $��buttonDelSpeed��5A   c   $
��Left=�6!   $��0,0,0,0q�67   $
��Left=�6H   $��32��5B      $��32��5C      
������ 
������ .��5E      �� $"��Assets/Icons/time_delete.png��6$   5F      �� 5H      -	   5        $��sliderSpeed��5H   7   $
��Left=�6!   $��15,5,5,5q�6J   $ 1Q�6\   $ 7Q�6h   $	��100��5I      $ 2Q�6*   64   �� 5K      -
   5        $��buttonPlusSpeed��5K   c   $
��Left=�6!   $��5,0,0,0q�67   $
��Left=�6H   $��32��5L      $��32��5M      
������ 
������ .��5O      �� $��Assets/Icons/time_add.png��6$   5P      5R      5T      �� 5U      $	 
Horizontal=�6   .+�5V      �� -   5        $��buttonShowDim��5V   /   $��10,5,0,5q�6   
������    ShowLEDs � ToolTip# Z 6]   
������ $��32��5W   4   $��32��6?   .��5X      �� $��Assets/Icons/eye.png��6    5Y      �� 5Z      -   5        $��buttonShowDim2��5Z   /   $��10,5,0,5q�6   
������ $y�	Collapsed=�6^      HideLEDs# Z 5[      
������ $��32��5\   4   $��32��6?   .��5]      �� $��Assets/Icons/eye_half.png��6    5^      5_      5`      5a      �� 5c   
   -   5        $��borderMoveLine��5c      $y�Visible=�6'   $��5q�6<   
�������$��1q�6Z   ._�5d      �� .+�5e      �� -   5        $��checkBoxOwnMoves��5e   &   $��5q�6      ShowOwnMoves#��Z 6�   6�   �� 5f      -   5        $��checkBoxBestMove��5f   &   $��5q�6   '    SelectionIsExclusiveShowOwnMoves# Z 6>   !   ShowBestMoveSelectedFigure#��Z 6�   6  5g      5h      � 5i   
    5j      �� 5k      $��70 �6#   �� 5l      $	��120 �6#   �� 5m      $��* �6#    5p      �� 5q      $
��Auto �6    .+�5u      �� -   5        $��buttonOk��5u   X   $��0��6   $��0��6%   $��5q�62   $
��Left=�6=   $��32��6h   $��32��5v      
������ 
������ .��5x      �� $$��Assets/Icons/accept_button.png��6   5y      �� 5z      -   5        $��buttonCancel��5z   Y   $��2��6   $��0��6%   $��5q�62   $��Right=�6=   $��32��6m   $��32��5{      
������ 
������ .��5}      �� $��Assets/Icons/cancel.png��6   5~      5      5�      5�      