   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:www.SoLaNoSoft.com.BearChessWinwww.SoLaNoSoft.com.BearChessWin  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   5    /www.SoLaNoSoft.com.BearChessWin.UciConfigWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ?local-clr-namespace:www.SoLaNoSoft.com.BearChessWin     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ 	CanResize=�6>    � Title$ UCI Engine Configuration��5	   	   $	��770��6*   $	��400��6:    � SizeToContent$ WidthAndHeight=�6F   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   .+�5      �� -   5        $��textBlockName��5      $��5q�6-   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   Name#��Z 68   �� 5      -   5        $��textBoxName��5      $	��330��6)   $��5q�65   6B   5      �� 5   
   $��1��6   
�������$��1q�61   $��5q�6E   ._�5      �� .+�5      ]� .+�5      �� -   5        $��checkBoxUseOpeningBook��5      $��Center=�6=   $��Center=�5      $
��Left=�6A   $��5q�5   N      UseOpeningBook#��Z 6Y   �� 5      -   5        $��comboBoxOpeningBooks��5      $��5q�6;   $
��Left=�6F   6^   5      �� 5      $��5q�6   $ 
Horizontal=�6(   .+�5       	� -   5        $��radioButtonBest��5    "   $��10,0,5,0q�69    	� 	GroupName$ 	Variation��6K   $��Center=�5!   "   $��Center=�6=   	   AlwaysChooseBestMove
 � ToolTip#
 Z	 5"   "      Best#��Z 6X   	� 5#      -   5        $��radioButtonFlexible��5#   "   $��5,0,5,0q�6=   $ 	Variation��6N   $��Center=�5$   "   $��Center=�6=      ChooseBetterMove#
 Z 5%   "      Flexible#��Z 6T   	� 5&      -   5        $��radioButtonWide��5&   "   $��5,0,5,0q�69   $ 	Variation��6J   $��Center=�5'   "   $��Center=�6=      ChooseAnyMove#
 Z 5(   "      Wide#��Z 6Q   5)      5*      5+      �� 5,   
   $��2��6   .��5-      �  5.      �� 5/      $
��Auto �6$   64   �� 50      $
��Auto �6$   64   �� 51      $
��Auto �6$   64   �� 52      $
��Auto �6$   64   �� 53      $
��Auto �6$   64   �� 54      $
��Auto �6$   64   �� 55      $
��Auto �6$   64   �� 56      $
��Auto �6$   64   .+�5<      �� $��2��6   
�������$��1q�69   $��5q�6M   ._�5=      �� $	��400��6#   $��Visible=�60   .��5>      � -   5        $��gridMain��5>       � ColumnDefinitions 5?      �� 5@   "   $
��Auto �63   �� 5A   "   $
��Auto �63   �� 5B   "   $
��Auto �63   �� 5C   "   $
��Auto �63   5E      5F      5G      �� 5H      $��3��6   
�������$��1q�69   $��5q�6M   ._�5I      �� $ 
Horizontal=�6!   .+�5J      �� $��5q�6$      WaitForNewGame#��Z 6/   $��Center=�6\   $��DemiBold"�5K   $   5L      �� 5M      -	   5        $��checkBoxWaitForStart��5M   >   $��Center=�6#   $��5q�6Z   5O      >   8www.SoLaNoSoft.com.BearChessWin.NumericUpDownUserControl  5P      -
   5          Name$" numericUpDownUserControlWait��5Q   9   $��30,0,0,0q�5P   9   $��70��6K      MaxValue$	 120��5Q   _      MinValue$ 0��5R   9   {�.  $$
 0 seconds = until button click��5S   9   5T      �� 5U      $��5q�6$   $
��sec.��6/   $��Center=�6;   6X   5V      5W      �� 5X      $��4��6   
�������$��1q�69   $��5q�6M   ._�5Y      �� $ 
Horizontal=�6!   .+�5Z      �� $��5q�6$   $��File:��6/   $��DemiBold"�6<   6T   �� 5[      -   5        $��textBlockFileName��5[   $   $��0,5,5,5q�6=   6P   5\      5]      �� 5^      -   5        $��
BorderLogo��5^      $��5��6,   
�������$��1q�6L   $��5q�6`   ._�5_      ]� ��.  .+�5`      �� $
��Left=�6$   $��5q�6:   $��Logo:��6E   $��DemiBold"�6R   6j   �� 5a      -   5        $��textBlockLogoFileName��5a   :   $
��Left=�6$   $��0,5,5,5q�6W   6j   �� 5b      -   5        $��buttonLogoClear��5b   8   $��Right=�6!   
������ 
������ $��20��5c   T   .��5d      �� $ ��../Assets/Icons/cancel.png��6$   $��16��6H   $��16��6S   6a   5e      �� 5f      -   5        $��buttonLogoFile��5f   8   $��Right=�6!   
������ 
������ .��5h      �� $ ��../Assets/Icons/folder.png��6$   $��16��6H   $��16��6S   6a   5i      5j      5l      �� 5m      $��6��6   
�������$��1q�69   $��5q�6M   ._�5n      ]� .+�5o      �� $
��Left=�6$   $��5q�6:   $��
Parameter:��6E   $��DemiBold"�6W   6o   �� 5p      -   5        $��buttonParameterFile��5p   8   $��Right=�6!   
������ 
������ .��5r      �� $ ��../Assets/Icons/folder.png��6$   $��16��6H   $��16��6S   6a   5s      �� 5t      -   5        $��textBlockFileParameter��5t   9   $��Right=�6"   $	��270��6W   $��0,5,5,5q�6f   6y   5v      5w      5y      5z      ]� 5{   
   $��3��6   $��10q�6!   ��.  .+�5|      �� -   5        $��buttonOk��5|   +   $
��Left=�6   
������ 
������    Save#
 Z 5}   .   .��5~      �� $'��!../Assets/Icons/accept_button.png��6   $��32��6C   $��32��6N   6\   5      �� 5�      -   5        $��buttonSaveAs��5�   >   $
��Left=�6   $��	20,0,10,0q�6+   
������ 
������    AddAsNewConfiguration#
 Z 5�      .��5�      �� $!��../Assets/Icons/cog_add.png��6   $��32��6=   $��32��6H   6V   5�      �� 5�      -   5        $��buttonCancel��5�   ,   $��Right=�6   
������ 
������    Cancel#
 Z 5�   .   .��5�      �� $ ��../Assets/Icons/cancel.png��6   $��32��6<   $��32��6G   6U   5�      �� 5�      -   5        $��buttonDefault��5�   >   $��Right=�6   $��0,0,10,0q�6,   
������ 
������    ResetDefaultValues#
 Z 5�      .��5�      �� $��../Assets/Icons/undo.png��6   $��32��6:   $��32��6E   6S   5�      �� 5�      -   5        $��	buttonLog��5�   ,   $��Right=�6   
������ 
������    ShowLogFiles#
 Z 5�   .   .��5�      �� $,��&../Assets/Icons/file_extension_log.png��6   $��32��6H   $��32��6S   6a   5�      5�      5�      5�      