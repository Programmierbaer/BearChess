   M S B A M L   `   `   `  ����   BearChessWin<    6www.SoLaNoSoft.com.BearChessWin.WinConfigureMChessLink   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � WindowStartupLocation$  CenterOwner=� � 
ResizeMode$ NoResize=� � Icon$. (../Assets/Icons/Millennium ChessLink.png�� � Title$$ Configure Millennium ChessLink��$	��280��$	��390�� � SizeToContent$ Height=�.���� .+��� $��5q�
�������$��1q�._��  � ColumnDefinitions �� $��75 ��� $	��120 ��� $��* � � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��5q�$��Current:��$��DemiBold"�$
��Left=�$��Center=��� -   $��textBlockCurrentPort��$��1��$��0��$��5q�$
��COM4��$��Center=�$��Center=��� $��0��$��1��$��5q�$��
COM-Ports:��$��DemiBold"�$
��Left=�$��Center=��� -   $��comboBoxComPorts��$��5,5,5,5q�$��1��$��1��$��Center=�$	��100��$��Center=�$��Center=�$��Center=�$��32���� -   $��buttonCheck��$��2��$��1��$��10,5,5,5q�$	��100��$��Center=�$��Center=�.����  �� Orientation$ 
Horizontal=�.+��� -   $��textBlockCheck��$��Center=�
Check...�� $!��../Assets/Icons/connect.png��$��32���� -   $��textBlockInformation��$��5,5,5,5q�$��0��$��2��$y�	Collapsed=�
���\\��$C��=No COM-Port detected. Check the connection to the chessboard.��$��3��$	��Top=�$��DemiBold"��� $��5q�
�������$��1q�._�]� .+��� $��5q�$��LEDs:��$��DemiBold"�$��70��$
��Left=�$��Center=�	� -   $��radioButtonBright��$��5q� 	� 	GroupName$	 dim��$	��110��$��Center=�.���� $ 
Horizontal=�.+��� $��Center=�Bright�� $��5,0,0,0q�$'��!../Assets/Icons/lightbulb_add.png��$��32��$��32��	� -   $��radioButtonDim��$��25,5,5,5q�$
��Left=�$��Center=�$	 dim��.���� $ 
Horizontal=�.+��� $��Center=�Dim�� $��5,0,0,0q�$*��$../Assets/Icons/lightbulb_delete.png��$��32��$��32���� $��5q�
�������$��1q�._�]� .+��� $��5q�$��
Move LEDs:��$��DemiBold"�$��70��$
��Left=�$��Center=�	� -   $��radioButtonSync��$��5q�$
 sync��$	��110��$��Center=�.���� $ 
Horizontal=�.+��� $��Center=�
Synchron�� $��10,0,0,0q�$+��%../Assets/Icons/images_flickr - 2.png��$��32��$��32��	� -	   $��radioButtonAlternate��$��25,5,5,5q�$
��Left=�$��Center=�$
 sync��.���� $ 
Horizontal=�.+��� $��Center=�	Alternate�� $��10,0,0,0q�$'��!../Assets/Icons/images_flickr.png��$��32��$��32��]� .+��� -
   $��buttonOk��$��10,5,5,5q�$
��Left=�$��32��$��32��
������ 
������ 	 � ToolTip$	 Ok��.���� $'��!../Assets/Icons/accept_button.png���� -   $��buttonCancel��$��5,5,10,5q�$��Right=�$��32��$��32��
������ 
������ $	 Cancel��.���� $ ��../Assets/Icons/cancel.png��