   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   :    4www.SoLaNoSoft.com.BearChessWin.SerialPortTestWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	     � Title$  Serial Port Test��5   	   $	��450��6"   $	��800��6/   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� $��0��6   $��5q�6"    �� Orientation$ 
Horizontal=�6-   $y�	Collapsed=�6F   .+�5      �� -   5        $��buttonConnectPipe��5      $	��100��6.   $��5q�6:   .��6e   	Connect6n   �� 5      -   5        $��buttonDisConnectPipe��5      $	��100��61   $��5q�6=   .��6k   
Disconnect6w   5      �� 5   
   $��0��6   $��5q�6"   $ 
Horizontal=�6-   $y�Visible=�6F   .+�5      �� $��5q�6   
Address:6/   �� 5      -   5        $��textBoxAddress��5      $	��150��6,   $��5q�68   .��6D   192.168.178.49:88886Y   �� 5      -   5        $��buttonConnectIP��5      $	��100��6,   $��5q�68   .��6a   	Connect6j   �� 5      -   5        $��buttonDicConnectIP��5      $	��100��6/   $��5q�6;   .��6g   
Disconnect6s   5      �� 5      $��0��6   $��5q�6&   $ 
Horizontal=�61   $y�	Collapsed=�6J   .+�5      �� $��5q�6   COM:6+   �� 5      -   5        $��textBoxPort��5      $��50��6)   $��5q�64   .��6@   COM126G   �� 5      $��5q�6   BAUD:6,   �� 5      -   5        $��textBoxBaud��5      $��80��6)   $��5q�64   .��6@   1152006H   �� 5      -   5        $��buttonConnect��5      $	��100��6*   $��5q�66   .��6]   	Connect6f   �� 5       -	   5        $��checkBoxChesstimation��5    :   $��5q�6   $��Chesstimation��6"   $��Center=�6W   $��Center=�6t   5!      �� 5"   
   -
   5        $��
listBoxLog��5"   *   $��5q�6   $��1��6   6>   �� 5#   
   $��2��6   $��5q�6"   $ 
Horizontal=�6-   .+�5$      �� -   5        $��	buttonGet��5$   V   $��5q�6   $��50��6     �� 	IsDefault . .��6g   Send6m   �� 5%      -   5        $��textBoxFrom��5%      $��5q�6)   $	��400��64   6B   5'      �� 5(   
   $��3��6   $��5q�6"   $ 
Horizontal=�6-   .+�5)      �� -   5        $��
buttonSend��5)   W   $��5q�6   $��50��6     . .��6i   Send6o   �� 5*      -   5        $��textBoxSend��5*      $��5q�6)   $	��400��64   6B   �� 5+      -   5        $��buttonClear��5+   J   $
��15,5q�6   $��50��6#   .��6]   Clear6d   �� 5,      -   5        $��
buttonCopy��5,   I   $
��15,5q�6   $��50��6#   .��6[   Copy6a   �� 5-      -   5        $��checkBoxRTS��5-   u   $
��15,5q�6   $��50��6%   .��6�   RTS6�   �� 5.      -   5        $��checkBoxDTR��5.   t   $	��5,5q�6   $��50��6$   .��6�   DTR6�   5/      50      51      