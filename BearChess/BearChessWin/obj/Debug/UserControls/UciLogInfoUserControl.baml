   M S B A M L   `   `   `  ����  BearChessWinH    Bwww.SoLaNoSoft.com.BearChessWin.UserControls.UciLogInfoUserControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .��5	      �   � RowDefinitions  5
   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� -   5        $��textBlockName��5   !   $��0��6   $
��Left=�66   $��Center=�6Q   $��5q�6l   �� 5   
   -   5        $��listBoxInfo��5   *   $��5q�6   $��1��6   6*   �� 5   
   $��5q�6   $��2��6     �� Orientation$ 
Horizontal=�6-   .+�5      �� -   5        $��
buttonStop��5      $��Center=�6'   $��5,0,5,0q�6B   $��24��6S   $��24��6^   
������ 
������  � ToolTip$ Stop/Go��5      .��5      �� .+�5      �� -   5        $��	imagePlay��5      $+��%../Assets/Icons/control_play_blue.png��6-   $y�	Collapsed=�6\   6u   �� 5      -   5        $��
imagePause��5      $,��&../Assets/Icons/control_pause_blue.png��6.   6a   5      5      �� 5      -   5        $��buttonClear��5      $��Center=�6(   $��0,0,5,0q�6C   $��24��6T   $��24��6_   
������ 
������ $ Clear��5      .��5      �� $*��$../Assets/Icons/clear_formatting.png��6   6H   5      �� 5       -   5        $��buttonClose��5       $��Center=�6(   $��0,0,5,0q�6C   $��24��6T   $��24��6_   
������ 
������ $ Close��5!      .��5"      �� $"��../Assets/Icons/door_out.png��6   6@   5#      �� 5$      $��Center=�6   $��5,0,5,0q�63   
Command:6N   �� 5%      -   5        $��textBoxCommand��5%   '   $��0,0,5,0q�6   $	��200��6=   $ Enter an UCI command.��6J   �� 5&      -	   5        $��
buttonSend��5&      $��Center=�6'   $��0,0,5,0q�6B   $��24��6S   $��24��6^   
������ 
������ $ Send command to engine��5'      .��5(      �� $#��../Assets/Icons/bullet_go.png��6   6A   5)      �� 5*      -
   5        $��buttonClipboard��5*      $��Center=�6,   $��0,0,5,0q�6G   $��24��6X   $��24��6c   
������ 
������ $ Copy to clipboard��5+      .��5,      �� $(��"../Assets/Icons/clipboard_sign.png��6   6F   5-      5.      5/      51      