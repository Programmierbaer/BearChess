   M S B A M L   `   `   `  ����   BearChessWin;    5www.SoLaNoSoft.com.BearChessWin.ChessBoardSetupWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � WindowStartupLocation$  CenterOwner=� � SizeToContent$ WidthAndHeight=� � Title$ Setup Board and Pieces��$	��250��$	��500��.���  � ColumnDefinitions �� $
��Auto ��� $
��Auto � � RowDefinitions �� $
��Auto ��� $
��Auto �.+��� -   $��groupBoxBoards��$��5q�$��0��$��0��$��Board��.���  �� $
��Auto ��� $
��Auto � �� �� �� �� .+��� -   $��comboBoxBoards��$	��200��$��25��$��Center=�$	��Top=�$��2��$��5q�$��0��$��0���� -   $��buttonOpenBoard��$��5q�$��0��$��1��$
��Left=�
������ 
������ .���� $��5q�$&�� ../Assets/Icons/file_manager.png��$��32��$��32���� -   $��buttonDeleteBoard��$��Right=�$��5q�$��1��$��1��
������ 
������ .���� $��5q�$��../Assets/Icons/bin.png��$��32��$��32���� -   $��checkBoxShowLastMove��$��5q�$��0��$��2��.���� $��1q�
��� � �$	��100��._��� $��1q�$��Show last move���� -   $��checkBoxShowBestMove��$��5q�$��0��$��3��.���� $��1q�
��� ���$	��100��._��� $��1q�$��Show best move���� -   $��groupBoxPieces��$��5q�$��1��$��0��$��Pieces��.���  �� $
��Auto ���  �� �� �� .+��� -   $��comboBoxPieces��$	��200��$��25��$	��Top=�$��5q�$��0��$��2��$��0���� -	   $��buttonOpenPieces��$��5q�$��0�� J� 	AllowDrop . $��1��$
��Left=�
������ 
������ .���� $��5q�$&�� ../Assets/Icons/file_manager.png��$��32��$��32���� -
   $��buttonDeletePieces��$��Right=�$��5q�$��1��$��1��
������ 
������ .���� $��5q�$��../Assets/Icons/bin.png��$��32��$��32���� -   $��5q�$
��Left=�$��0��
������ 
������ $��1��.���� $'��!../Assets/Icons/accept_button.png��$��32��$��32���� -   $��buttonCancel��$��5q�$��Right=�$��1��
������ 
������ $��1��.���� $ ��../Assets/Icons/cancel.png��$��32��$��32��