   M S B A M L   `   `   `  ����   BearChessWin9    3www.SoLaNoSoft.com.BearChessWin.WinConfigureCertabo   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � WindowStartupLocation$  CenterOwner=� � 
ResizeMode$ NoResize=� � SizeToContent$ Height=� � Title$" Configure Certabo Chessboard��$	��230��$	��390�� � Icon$&  ../Assets/Icons/Certabo_icon.ico��.���� .+��� $��5q�
�������$��1q�._��  � ColumnDefinitions �� $��75 ��� $	��120 ��� $��* � � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��5q�$��Current:��$��DemiBold"�$
��Left=�$��Center=��� -   $��textBlockCurrentPort��$��1��$��0��$��5q�$
��COM4��$��Center=�$��Center=��� $��0��$��1��$��5q�$��
COM-Ports:��$��DemiBold"�$
��Left=�$��Center=��� -   $��comboBoxComPorts��$��5,5,5,5q�$��1��$��1��$��Center=�$	��100��$��Center=�$��Center=�$��Center=�$��32���� -   $��buttonCheck��$��2��$��1��$��10,5,5,5q�$	��100��$��Right=�$��Center=�$y�Visible=�.����  �� Orientation$ 
Horizontal=�.+��� -   $��textBlockCheck��$��Center=�
Check...�� $!��../Assets/Icons/connect.png��$��32���� -   $��textBlockInformation��$��5,5,5,5q�$��0��$��2��$y�	Collapsed=�
���\\��$C��=No COM-Port detected. Check the connection to the chessboard.��$��3��$	��Top=�$��DemiBold"��� $��5q�
�������$��1q�._��  �� $��75 ��� $	��120 ��� $��* � �� $
��Auto �.+��� $��0��$��0��$��5q�$��Calibration:��$��DemiBold"�$
��Left=�$��Center=��� -   $��textBlockCalibrate��$��1��$��0��$��5q�$��Is not calibrated��$��Center=�$��Center=��� -   $��buttonCalibrate��$��2��$��0��$��10,5,5,5q�$��Right=�$	��100��.���� $ 
Horizontal=�.+��� $��Center=�Calibrate...�� $��2,0,0,0q�$)��#../Assets/Icons/chessboard_base.png��$��32���  �� $��70 ��� $	��120 ��� $��* � �� $
��Auto �.+��� -   $��buttonOk��$��0��$��0��$��5q�$
��Left=�$��32��$��32��
������ 
������  � ToolTip$ Ok��.���� $'��!../Assets/Icons/accept_button.png���� -	   $��buttonCancel��$��2��$��0��$��5q�$��Right=�$��32��$��32��
������ 
������ $ Cancel��.���� $ ��../Assets/Icons/cancel.png��