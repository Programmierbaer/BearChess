   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5      O    Iwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.ConfigureMChessLinkWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5        � Icon$+  %Assets/Icons/Millennium ChessLink.png��5      J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   ConfigureMillennium � Title# Z 5	      $	��730��67   $	��390��6D    � SizeToContent$ WidthAndHeight=�6P   .��5
      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $��75 �6'   �� 5      $	��120 �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      Current#��Z 6D   $��DemiBold"�6j   $
��Left=�5      $��Center=�64   �� 5      -   5        $��textBlockCurrentPort��5      $��1��68   $��0��6H   $��5q�6U   $
��COM4��6`   $��Center=�5      $��Center=�66   �� 5      $��0��6   $��1��6,   $��5q�69      ComPorts#��Z 6D   $��DemiBold"�6k   $
��Left=�5      $��Center=�64   �� 5      -   5        $��comboBoxComPorts��5   ,   $��5,5,5,5q�6   $��1��6D   $��1��6T   $��Center=�5      $	��100��65   $��Center=�5       $��Center=�6<   $��Center=�5!      $��32��6:   �� 5"      -   5        $��buttonCheck��5"      $��2��6,   $��1��6<   $��10,5,5,5q�6I   $	��100��6[   $��Center=�5#      $��Center=�63   .��5$      ��  �� Orientation$ 
Horizontal=�6!   .+�5%      �� -   5        $��textBlockCheck��5%   $   $��Center=�6:   	   Check#��Z	 6U   �� 5&      $��Assets/Icons/connect.png��6    $��32��6B   5'      5(      �� 5)      -   5        $��textBlockInformation��5)      $��5,5,5,5q�68   $��0��6I   $��2��6Y   $y�	Collapsed=�5*      $
��Wrap=�60   
���\\��
   NoCOMPortDetected#��Z
 5+   0   $��3��5,      $	��Top=�6-   $��DemiBold"�6E   5.      5/      �� 50   
   -   5        $��
borderLEDs��50      $��5q�6#   
�������$��1q�6A   ._�51      �� .+�52      ]� $��0,5,0,0q�6   ��.  .+�53      �� $
��Left=�6    $��5q�66      LEDs#��Z 6A   $��DemiBold"�6d   $��70��6z   $
��Left=�54      $��Center=�68   �� 55      -   5        $��buttonDecrementDim��55   (   $��0q�6   
������ $
��Left=�6[   
������ $��32��56   4   $��32��6?   .��57      �� $'��!Assets/Icons/lightbulb_delete.png��6    58      �� 5:      -   5        $��	sliderDim��5:   3   $
��Left=�6   $��15,5,5,5q�6D    � Minimum$ 0Q�6V    � Maximum$ 14Q�6b   $	��100��5;      5<      �� 5=      -	   5        $��buttonIncrementDim��5=   .   $��5,0,0,0q�6   
������ $
��Left=�6a   
������ $��32��5>   4   $��32��6?   .��5?      �� $$��Assets/Icons/lightbulb_add.png��6    5@      5A      ]� 5B      .+�5C      �� $
��Left=�6    $��5q�66      MoveLEDs#��Z 6A   $��DemiBold"�6h   $��70��6~   $
��Left=�5D      $��Center=�68   	� 5E      -
   5        $��radioButtonSync��5E   `   $
��Left=�6"   $��5q�68    	� 	GroupName$
 sync��6C   $	��125��6T   $��Center=�5F      .��5G      �� $ 
Horizontal=�6%   .+�5H      �� $��Center=�6$      Synchron#��Z 6?   �� 5I      $��10,0,0,0q�6$   $(��"Assets/Icons/images_flickr - 2.png��66   $��32��6b   $��32��5J   !   5K      5M      	� 5N      -   5        $��radioButtonAlternate��5P      $
��Left=�5N   "   $��25,5,5,5q�68   $
��Left=�6J   $��Center=�5O      $
 sync��5P   ;   .��5Q      �� $ 
Horizontal=�6%   .+�5R      �� $��Center=�6$      	Alternate#��Z 6?   �� 5S      $��10,0,0,0q�6$   $$��Assets/Icons/images_flickr.png��66   $��32��6^   $��32��6i   5T      5U      5W      	� 5X      -   5        $��radioButtonNoFlash��5Z      $��Right=�5X      $��85,5,5,5q�65   $
��Left=�6G   $��Center=�5Y      $
 sync��5Z   5   .��5[      �� $ 
Horizontal=�6!   .+�5\      �� $��Center=�6       NoFlash#��Z 6;   �� 5]      $��10,0,0,0q�6    $��Assets/Icons/image.png��62   $��32��6R   $��32��6]   5^      5_      �� 5`      $ 
Horizontal=�6   .+�5a      �� -   5        $��buttonShowDim��5a   /   $��10,5,0,5q�6   
������    ConnectTestLEDs � ToolTip# Z 6]   
������ $��32��5b   4   $��32��6?   .��5c      �� $��Assets/Icons/eye.png��6    5d      �� 5e      -   5        $��buttonShowDim2��5e   /   $��10,5,0,5q�6   
������ $y�	Collapsed=�6^      
Disconnect# Z 5f      
������ $��32��5g   4   $��32��6?   .��5h      �� $��Assets/Icons/eye_half.png��6    5i      5j      5k      5l      �� 5o   
   -   5        $��borderDelay��5o      $��5q�6$   
�������$��1q�6B   ._�5p      �� $��0,5,0,5q�6   .+�5q      ]� ��.  .+�5r      �� $
��Left=�6    $��5q�66      Delay#��Z 6A   $��DemiBold"�6e   $��70��6{   $
��Left=�5s      $��Center=�68      HigherValueIncrease# Z 5t      �� 5u      -   5        $��buttonDelDebounce��5u   _   $
��Left=�6   $��0,0,0,0q�63   $
��Left=�6D   $��32��5v      $��32��5w      
������ 
������    DecrementDelay# Z 5y      .��5z      �� $"��Assets/Icons/time_delete.png��6    5{      �� 5}      -   5        $��sliderDebounce��5}   3   $
��Left=�6   $��15,5,5,5q�6I   $ 0Q�6[   $ 4Q�6g   $	��100��5~       � Value$ 20Q�6&   5      �� 5�      -   5        $��buttonPlusDebounce��5�   _   $
��Left=�6   $��5,0,0,0q�63   $
��Left=�6D   $��32��5�      $��32��5�      
������ 
������    IncrementDelay# Z 5�      .��5�      �� $��Assets/Icons/time_add.png��6    5�      �� 5�      -   5        $��textBlockDebounce��5�       $
��Left=�69   $
��Left=�6O   $��5,0,0,0q�5�      $��Center=�5�      $��0��68   5�      �� 5�      $ 
Horizontal=�6   .+�5�      �� -   5        $��buttonDebounceDefault��5�   0   $��	10,5,10,5q�6   
������    ResetDefault# Z 5�      
������ $��32��5�   4   $��32��6?   .��5�      �� $��Assets/Icons/time.png��6    5�      �� 5�      $��Center=�6       ResetDefaultWithout#��Z 6;   5�      5�      5�      �� 5�   
   -   5        $��borderScans��5�      $��5q�6$   
�������$��1q�6B   ._�5�      �� $��0,5,0,5q�6   .+�5�      ]� ��.  .+�5�      �� $
��Left=�6    $��5q�66      Scans#��Z 6A   $��DemiBold"�6e   $��70��6{   $
��Left=�5�      $��Center=�68      HowManyTimerPerSecond# Z 5�      �� 5�      -   5        $��buttonDelScan��5�   _   $
��Left=�6   $��0,0,0,0q�63   $
��Left=�6D   $��32��5�      $��32��5�      
������ 
������    DecrementScanTime# Z 5�      .��5�      �� $"��Assets/Icons/time_delete.png��6    5�      �� 5�      -   5        $��sliderScanTime��5�   3   $
��Left=�6   $��15,5,5,5q�6I   $ 15Q�6[   $	 255Q�6h   $	��100��5�      $ 20Q�6&   5�      �� 5�      -   5        $��buttonPlusScan��5�   _   $
��Left=�6   $��5,0,0,0q�63   $
��Left=�6D   $��32��5�      $��32��5�      
������ 
������     IncrementScanTime# Z  5�      .��5�      �� $��Assets/Icons/time_add.png��6    5�      �� 5�      -   5        $��textBlockScansPerSec��5�       $
��Left=�6<   $
��Left=�6R   $��5,0,0,0q�5�      $��Center=�5�      !   PerSec#��Z! 68   5�      �� 5�      $ 
Horizontal=�6   .+�5�      �� -   5        $��buttonScanDefault��5�   0   $��	10,5,10,5q�6   
������ # Z 5�      
������ $��32��5�   4   $��32��6?   .��5�      �� $��Assets/Icons/time.png��6    5�      �� 5�      $��Center=�6    "   ResetDefaultPerSec#��Z" 6;   5�      5�      5�      �� 5�   
   -   5        $��borderMoveLine��5�      $y�Visible=�6'   $��5q�6<   
�������$��1q�6Z   ._�5�      �� .+�5�      �� .+�5�      �� -   5        $��checkBoxMoveLine��5�   *   $��5q�6   #   ActivateAllLEDsAlongMove#��Z# 6B   6~   �� 5�      -   5        $��checkBoxOwnMoves��5�   *   $��5q�6   $   SelectionIsExclusive# Z$ 6B   %   ShowOwnMoves#��Z% 6�   �� 5�      -   5        $��checkBoxPossibleMoves��5�   *   $��5q�6   '&    SelectionIsExclusiveShowOwnMoves# Z& 6G   !'   ShowAllMovesSelectedFigure#��Z' 6�   �� 5�      -   5        $��checkBoxBestMove��5�   *   $��5q�6   # Z& 6B   !(   ShowBestMoveSelectedFigure#��Z( 6�   5�      �� 5�      -    5        $��checkBoxCurrentValue��5�   &   $��5q�6   )   ShowValuation#��Z) 6�   �� 5�      -!   5        $��stackPanelValuation��5�      $
��15,5q�68   $ 
Horizontal=�6F   .+�5�      	� -"   5        $��radioButtonValueLeft��5�   "   $ 	Valuation��6>   $��5q�6T   *   Left#��Z* 6_   	� 5�      -#   5        $��radioButtonValueLeftRight��5�   "   $ 	Valuation��6C   $��5q�6Y   +   	LeftRight#��Z+ 6d   	� 5�      -$   5        $��radioButtonValueBottom��5�   "   $ 	Valuation��6@   , d� 	IsChecked$
, TrueY�6V   $��5q�6g   -   Bottom#��Z- 6r   	� 5�      -%   5        $��radioButtonValueBottomTop��5�   "   $ 	Valuation��6C   $��5q�6Y   .   	BottomTop#��Z. 6d   5�      5�      5�      �� 5�   
   -&   5        $��borderChesstimation��5�      $y�Visible=�6,   $��5q�6A   
�������$��1q�6_   {�.  ._�5�      �� $ Vertical=�6   .+�5�      �� $
��Wrap=�6   $��5,5,5,0q�60   /   UsingChesstimation#��Z/ 6A   5�      5�      �� 5�   
   -'   5        $��borderElfacun��5�      $y�Visible=�6&   $��5q�6;   
�������$��1q�6Y   {�.  ._�5�      �� $ Vertical=�6   .+�5�      �� $
��Wrap=�6   $��5,5,5,0q�60   0   UsingElfacun#��Z0 6A   5�      5�      ]� 5�   
   $��0,0,0,5q�6   .+�5�      �� -(   5        $��buttonOk��5�   X   $
��Left=�6   $��10,5,5,5q�6+   $
��Left=�6=   $��32��6h   $��32��5�      
������ 
������ .��5�      �� $$��Assets/Icons/accept_button.png��6   5�      �� 5�      -)   5        $��buttonCancel��5�   Z   $��Right=�6   $��5,5,10,5q�6,   $��Right=�6>   $��32��6n   $��32��5�      
������ 
������ .��5�      �� $��Assets/Icons/cancel.png��6   5�      5�      5�      5�      