   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   5    /www.SoLaNoSoft.com.BearChessWin.WinConfigureOSA   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ WidthAndHeight=�6T   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   ConfigureSaitekOSA � Title# Z 5	   	   $	��242��6<   $	��500��6I   .��5
      �� .+�5   
   �� $��5q�6   
�������$��1q�6/   ._�5      �  � ColumnDefinitions 5      �� 5      $��75 �6'   �� 5      $	��120 �6'   �� 5      $��55 �6'   �� 5      $	��120 �6'   �� 5      $��* �6'    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6,   $��5q�69      Current#��Z 6D   $��DemiBold"�6j   $
��Left=�5      $��Center=�67   �� 5      -   5        $��textBlockCurrentPort��5      $��1��68   $��0��6H   $��5q�6U   $
��COM4��6`   $��Center=�5      $��Center=�69   �� 5      -   5        $��textBlockCurrentBaud��5      $��3��68   $��0��6H   $��5q�6U   $
��9600��6`   $��Center=�5      $��Center=�69   �� 5       $��0��6   $��1��6,   $��5q�69      ComPorts#��Z 6D   $��DemiBold"�6k   $
��Left=�5!      $��Center=�67   �� 5"      -   5        $��comboBoxComPorts��5"   ,   $��5,5,5,5q�6   $��1��6D   $��1��6T   $��Center=�5#      $	��100��68   $��Center=�5$      $��Center=�6?   $��Center=�5%      $��32��6=   �� 5&      $��2��6   $��1��6,   $��5q�69   	   Baud#��Z	 6D   $��DemiBold"�6g   $
��Left=�5'      $��Center=�67   �� 5(      -   5        $��comboBoxBaud��5(   ,   $��5,5,5,5q�6   $��3��6@   $��1��6P   $��Center=�5)      $	��100��68   $��Center=�5*      $��Center=�6?   $��Center=�5+      $��32��6=   �� 5,      -   5        $��buttonCheck��5,      $��4��6,   $��1��6<   $��10,5,5,5q�6I   $	��100��6[   $��Right=�5-   5   $��Center=�6Q   $y�Visible=�5.      .��5/      �� 
 �� Orientation$
 
Horizontal=�6!   .+�50      �� -   5        $��textBlockCheck��50   $   $��Center=�6:      Check#��Z 6U   6{   �� 51      $!��../Assets/Icons/connect.png��6    $��32��6E   52      53      �� 54      -   5        $��textBlockInformation��54      $��5,5,5,5q�68   $��0��6I   $��2��6Y   $y�	Collapsed=�55      $
��Wrap=�63   
���\\��   NoCOMPortDetected#��Z 56   3   $��3��57      $	��Top=�60   $��DemiBold"�6H   59      5:      � 5;   
    5<      �� 5=      $��70 �6#   �� 5>      $	��120 �6#   �� 5?      $��* �6#    5B      �� 5C      $
��Auto �6    .+�5G      �� -   5        $��buttonOk��5G   X   $��0��6   $��0��6%   $��5q�62   $
��Left=�6=   $��32��6h   $��32��5H      
������ 
������ .��5J      �� $'��!../Assets/Icons/accept_button.png��6   5K      �� 5L      -	   5        $��buttonCancel��5L   Y   $��2��6   $��0��6%   $��5q�62   $��Right=�6=   $��32��6m   $��32��5M      
������ 
������ .��5O      �� $ ��../Assets/Icons/cancel.png��6   5P      5Q      5S      5T      